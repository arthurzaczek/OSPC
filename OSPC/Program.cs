//#define SINGLE_THREADED

using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OSPC
{
    class Program
    {
        static void Main(string[] args)
        {
            var cfg = new Configuration();
            bool showHelp = false;

            // changable through config or arguments
            Reporter.IReporter html = null;
            Reporter.IReporter console = new Reporter.ConsoleReporter();
            Tokenizer.ITokenizer tokenizer = new Tokenizer.CLikeTokenizer();

            Console.WriteLine("Open Software Plagiarism Checker");
            Console.WriteLine("================================");
            Console.WriteLine();

            var p = new OptionSet()
            {
                { "h|?|help", "Prints this help", v => showHelp = true },
                { "c=", "Reads the given configuration. Note, this switch should be the first argument as it overrides any other argument parsed yet.", v => cfg = LoadConfig(v) },
                { "write-config=", "Write the current configuration to the given file. Note, this switch should be the last argument.", v => SaveConfig(cfg, v) },

                { "f=", "File filter. If -d is specified, then -f defaults to \"*.*.\"", v => cfg.Filter.Add(v) },
                { "d=", "Specifies a directory where the filer applies. If -f is specified, then -d defaults to \".\"", v => cfg.Dirs.Add(v) },
                { "include=", "Specifies a regular expression that every file must match. More than one expression is allowed. A file must match any of these expressions.", v => cfg.Include.Add(v) },
                { "exclude=", "Specifies a regular expression to exclude files. More than one expression is allowed. If a file must match any of these expressions it will be excluded.", v => cfg.Exclude.Add(v) },
                { "recurse", "Traversals all directories recurse. Use include-dir and exclude-dir to have more control over the selected directories.", v => cfg.Recurse = true },
                { "include-dir=", "Specifies a regular expression that every file must match. More than one expression is allowed. A file must match any of these expressions.", v => cfg.IncludeDir.Add(v) },
                { "exclude-dir=", "Specifies a regular expression to exclude files. More than one expression is allowed. If a file must match any of these expressions it will be excluded.", v => cfg.ExcludeDir.Add(v) },

                { "detailed", "Print a detailed report to the console", v => console = new Reporter.DetailedConsoleReporter() },
                { "summary", "Print only a summay to the console. Usefull if --html is used.", v => console = new Reporter.SummaryConsoleReporter() },
                { "html:", "Saves a html report to the specified directory. Defaults to \"report\"", v => html = new Reporter.Html.HtmlReporter(v) },

                { "min-match-length=", "Minimum count of matching tokens, including non-matching tokens.", v => cfg.MIN_MATCH_LENGTH = int.Parse(v) },
                { "max-match-distance=", "Maximum distance between tokens to count as a match. 1 = exact match.", v => cfg.MAX_MATCH_DISTANCE = int.Parse(v) },
                { "min-common-token=", "Percent of token that must match to count as a match. 1 = every token must match.", v => cfg.MIN_COMMON_TOKEN = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture) },

                { "v|verbose", "Verbose output.", v =>  cfg.Verbose = true },
            };

            var extraFiles = p.Parse(args);
            // split that, as during parse the reference of cfg may change. 
            // The compiler might save the reference to the old, overridden 
            // config instance.
            cfg.ExtraFiles = extraFiles;

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            var comparer = new Comparer(cfg);
            var friendfinder = new FriendFinder(cfg);            
            var watch = new Stopwatch();

            watch.Start();

            var files = CollectFiles(cfg, tokenizer);
            if (files.Length == 0)
            {
                Console.WriteLine("No files found to compare!");
            }
            else
            {
                Console.Write("Comparing {0} files ", files.Length);
                var compareResult = comparer.Compare(files);
                Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);

                Console.WriteLine("Creating statistics");
                var result = OSPCResult.Create(compareResult);
                friendfinder.Find(result, compareResult);
                Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);

                Console.WriteLine("Creating reports");
                CreateReports(html, console, result);
                Console.WriteLine("  finished in total {0:n2} sec.", watch.Elapsed.TotalSeconds);
            }
        }        

        private static void CreateReports(Reporter.IReporter html, Reporter.IReporter console, OSPCResult result)
        {
            if (html != null)
            {
                html.Create(result);
            }
            console.Create(result);
        }

        private static void SaveConfig(Configuration cfg, string file)
        {
            Console.WriteLine("Writing current configuration.\n");
            using (var fs = new FileStream(file, FileMode.Create))
            {
                fs.SetLength(0);
                cfg.ToXmlStream(fs);
                fs.Flush();
            }
        }

        private static Configuration LoadConfig(string file)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            {
                return fs.FromXmlStream<Configuration>();
            }
        }

        private static Submission[] CollectFiles(Configuration cfg, Tokenizer.ITokenizer tokenizer)
        {
            if (cfg.Filter.Count == 0 && cfg.Dirs.Count > 0)
            {
                cfg.Filter.Add("*.*");
            }
            if (cfg.Dirs.Count == 0 && cfg.Filter.Count > 0)
            {
                cfg.Dirs.Add(".");
            }

            var files = new List<string>();
            var include = cfg.Include.Select(i => new Regex(i)).ToList();
            var exclude = cfg.Exclude.Select(i => new Regex(i)).ToList();
            var includeDir = cfg.IncludeDir.Select(i => new Regex(i)).ToList();
            var excludeDir = cfg.ExcludeDir.Select(i => new Regex(i)).ToList();

            foreach(var dir in cfg.Dirs)
            { 
                CollectFilesRecurse(files, Directory.GetDirectories(dir), cfg.Filter, include, exclude, includeDir, excludeDir, cfg.Recurse);
            }

            files.AddRange(cfg.ExtraFiles);

            if (cfg.Verbose)
            {
                Console.WriteLine("Files:");
                foreach (var f in files)
                {
                    Console.WriteLine(f);
                }
                Console.WriteLine();
            }

            return files
                    .Select(f =>
                    {
                        var s = new Submission(f, tokenizer);
                        s.Parse();
                        return s;
                    })
                    .ToArray();
        }

        private static void CollectFilesRecurse(List<string> files, IEnumerable<string> directories, IEnumerable<string> filters, IEnumerable<Regex> include, IEnumerable<Regex> exclude, IEnumerable<Regex> includeDir, IEnumerable<Regex> excludeDir, bool recurse)
        {
            foreach(var dir in directories)
            {
                if (includeDir.Any() && !includeDir.Any(r => r.Match(dir).Success)) continue;
                if (excludeDir.Any() && excludeDir.Any(r => r.Match(dir).Success)) continue;

                foreach (var pattern in filters)
                {
                    foreach(var f in Directory.GetFiles(dir, pattern))
                    {
                        if (include.Any() && !include.Any(r => r.Match(f).Success)) continue;
                        if (exclude.Any() && exclude.Any(r => r.Match(f).Success)) continue;
                        files.Add(f);
                    }
                }

                if(recurse)
                {
                    CollectFilesRecurse(files, Directory.GetDirectories(dir), filters, include, exclude, includeDir, excludeDir, recurse);
                }
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: OSPC [options] { file1 file2 ... }");
            Console.WriteLine();
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine(@"  file1 file2                Optional. Files or additional files, if -f or -d 
                               is not used or not applicalable.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine(@"  OSPC - d c:\somedir - f *.c

    Checks all *.c files in somedir with the default settings.

  OSPC c:\somedir\file1.c c:\somedir\file2.c

    Checks file1.c and file2.c using absolute paths with the default settings.

  OSPC a.c b.c

    Checks file1.c and file2.c using relative paths with the default settings.

  OSPC -c basic_profile.xml --summay --html -f *.c

    Checks all c - files in the current directory and output a html report to.\report\index.html.

   OSPC --write-config default.xml

    Writes the default configuration to default.xml

  OSPC --min-match-length=100 --max-match-distance=2 --min-common-token=0.95 --write-config basic.xml

    Writes the current configuration to basic.xml");
        }
    }
}
