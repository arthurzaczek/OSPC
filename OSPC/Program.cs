// OSPC - Open Software Plagiarism Checker
// Copyright(C) 2015 Arthur Zaczek at the UAS Technikum Wien


// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

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
            try
            {
                var cfg = new Configuration();
                bool showHelp = false;

                // common dependencies
                var progress = new ConsoleProgressReporter();

                // changable through config or arguments
                Reporter.IReporter html = null;
                Reporter.IReporter console = new Reporter.ConsoleReporter();
                Tokenizer.ITokenizer tokenizer = new Tokenizer.CLikeTokenizer();

                Console.WriteLine("Open Software Plagiarism Checker");
                Console.WriteLine("================================");
                Console.WriteLine("Version {0}", typeof(OSPC.Program).Assembly.GetName().Version);
                Console.WriteLine();
                Console.WriteLine("Copyright (C) 2015 Arthur Zaczek at the UAS Technikum Wien, GPL V3");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY; see the GPL V3 for details.");
                Console.WriteLine("This is free software, and you are welcome to redistribute it");
                Console.WriteLine("under certain conditions; see the GPL V3 for details.");
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
                    { "html:", "Saves a html report to the specified directory. Defaults to \"report\"", v => html = new Reporter.Html.HtmlReporter(v, progress) },
                    { "min-similarity=", "Minimum similarity of match reports (0 - 1). Default is 0.5 (=50%).", v => cfg.MIN_SIMILARITY = double.Parse(v, System.Globalization.CultureInfo.InvariantCulture) },

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

                var comparer = new Comparer(cfg, progress);
                var friendfinder = new FriendFinder(cfg);
                var watch = Stopwatch.StartNew();

                Console.WriteLine("Collecting files");
                var files = CollectFiles(cfg, tokenizer);
                Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);
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
                    CreateReports(cfg, html, console, result);
                    Console.WriteLine("  finished in total {0:n2} sec.", watch.Elapsed.TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("********** Error occurred, exiting **********");
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }

        private static void CreateReports(Configuration cfg, Reporter.IReporter html, Reporter.IReporter console, OSPCResult result)
        {
            if (html != null)
            {
                html.Create(cfg, result);
            }
            console.Create(cfg, result);
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

            CollectFilesRecurse(files, cfg.Dirs, true, include, exclude, includeDir, excludeDir, cfg);

            files.AddRange(cfg.ExtraFiles.Where(f =>
            {
                if (File.Exists(f))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("  ** Warning, file \"{0}\" not found.", f);
                    return false;
                }
            }));

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

        private static void CollectFilesRecurse(List<string> files, IEnumerable<string> directories, bool isFirstLevel, IEnumerable<Regex> include, IEnumerable<Regex> exclude, IEnumerable<Regex> includeDir, IEnumerable<Regex> excludeDir, Configuration cfg)
        {
            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    // At the first level, this may occour
                    Console.WriteLine("  ** Warning, directory \"{0}\" not found.", dir);
                    continue;
                }
                if (cfg.Verbose) Console.WriteLine("D: {0}", dir);
                if (!isFirstLevel)
                {
                    if (includeDir.Any() && !includeDir.Any(r => r.Match(dir).Success)) continue;
                    if (excludeDir.Any() && excludeDir.Any(r => r.Match(dir).Success)) continue;
                }

                foreach (var pattern in cfg.Filter)
                {
                    string[] dirFiles;
                    try
                    {
                        dirFiles = Directory.GetFiles(dir, pattern);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  ** Warning: Unable to list directory with patern \"{0}\": {1}", pattern, ex.Message);
                        continue;
                    }

                    foreach (var f in dirFiles)
                    {
                        if (cfg.Verbose) Console.WriteLine("  F: {0}", f);

                        if (include.Any() && !include.Any(r => r.Match(f).Success)) continue;
                        if (exclude.Any() && exclude.Any(r => r.Match(f).Success)) continue;

                        files.Add(f);
                    }
                }

                if (cfg.Recurse)
                {
                    CollectFilesRecurse(files, Directory.GetDirectories(dir), false, include, exclude, includeDir, excludeDir, cfg);
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
