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

                { "detailed", "Print a detailed report to the console", v => console = new Reporter.DetailedConsoleReporter() },
                { "summary", "Print only a summay to the console. Usefull if --html is used.", v => console = new Reporter.SummaryConsoleReporter() },
                { "html:", "Saves a html report to the specified directory. Defaults to \"report\"", v => html = new Reporter.Html.HtmlReporter(v) },

                { "min-match-length=", "Minimum count of matching tokens, including non-matching tokens.", v => cfg.MIN_MATCH_LENGTH = int.Parse(v) },
                { "max-match-distance=", "Maximum distance between tokens to count as a match. 1 = exact match.", v => cfg.MAX_MATCH_DISTANCE = int.Parse(v) },
                { "min-common-token=", "Percent of token that must match to count as a match. 1 = every token must match.", v =>  cfg.MIN_COMMON_TOKEN = double.Parse(v) },

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
            var result = new OSPCResult();
            var compareList = new List<Tuple<Submission, Submission>>();
            var compareResult = new List<CompareResult>();
            var watch = new Stopwatch();

            watch.Start();

            var files = CollectFiles(cfg, tokenizer);
            CreateCompareList(files, compareList);

            Console.Write("Comparing {0} files ", files.Length);
            Compare(comparer, compareList, compareResult);
            Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);

            Console.WriteLine("Creating statistics");
            CalcStatistics(result, compareResult);
            StartFriendFinder(cfg, friendfinder, result, compareResult);
            Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);

            Console.WriteLine("Creating reports");
            CreateReports(html, console, result);
            Console.WriteLine("  finished in total {0:n2} sec.", watch.Elapsed.TotalSeconds);
        }

        private static void CreateCompareList(Submission[] files, List<Tuple<Submission, Submission>> compareList)
        {
            for (int a = 0; a < files.Length; a++)
            {
                for (int b = a + 1; b < files.Length; b++)
                {
                    if (Path.GetExtension(files[a].FilePath) != Path.GetExtension(files[b].FilePath)) continue;

                    compareList.Add(new Tuple<Submission, Submission>(files[a], files[b]));
                }
            }
        }

        private static void Compare(Comparer comparer, List<Tuple<Submission, Submission>> compareList, List<CompareResult> compareResult)
        {
            int progressCounter = 0;
            object _lock = new object();

#if SINGLE_THREADED
            foreach(var pair in compareList)
#else
            Parallel.ForEach(compareList, pair =>
#endif
            {
                var r = comparer.Compare(pair.Item1, pair.Item2);

                lock (_lock)
                {
                    compareResult.Add(r);
                    if (++progressCounter % 100 == 0) Console.Write(".");
                }
            }
#if !SINGLE_THREADED
            );
#endif
            Console.WriteLine();
        }

        private static void CreateReports(Reporter.IReporter html, Reporter.IReporter console, OSPCResult result)
        {
            if (html != null)
            {
                html.Create(result);
            }
            console.Create(result);
        }

        private static void StartFriendFinder(Configuration cfg, FriendFinder friendfinder, OSPCResult result, List<CompareResult> compareResult)
        {
            if (cfg.MIN_FRIEND_FINDER_SIMILARITY < 0)
            {
                cfg.MIN_FRIEND_FINDER_SIMILARITY = result.POI_Similarity - 0.2;
            }

            result.Friends = friendfinder.Find(compareResult);
        }

        private static void CalcStatistics(OSPCResult result, List<CompareResult> compareResult)
        {
            result.Results = compareResult
                .Where(r => r.MatchCount > 0)
                .OrderByDescending(r => Math.Max(r.SimilarityA, r.SimilarityB))
                .Select((item, idx) =>
                {
                    item.Seal(idx);
                    return item;
                })
                .ToList();

            double[] lst = result.Results.SelectMany(i => new[] { i.SimilarityA, i.SimilarityB }).OrderBy(i => i).ToArray();
            if (lst.Length > 0)
            {
                result.AVG_Similarity = lst.Average();
                result.POI_Similarity = lst[lst.CalcDerv2().MaxIndex()];
            }

            lst = result.Results.Select(i => (double)i.TokenCount).OrderBy(i => i).ToArray();
            if (lst.Length > 0)
            {
                result.AVG_TokenCount = lst.Average();
                result.POI_TokenCount = lst[lst.CalcDerv2().MaxIndex()];
            }

            lst = result.Results.Select(i => (double)i.TokenCount / (double)i.MatchCount).OrderBy(i => i).ToArray();
            if (lst.Length > 0)
            {
                result.AVG_TokenPerMatch = lst.Average();
                result.POI_TokenPerMatch = lst[lst.CalcDerv2().MaxIndex()];
            }
        }

        private static void SaveConfig(Configuration cfg, string file)
        {
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

            var qry = cfg.Dirs
                .SelectMany(d => cfg.Filter.Select(f => new Tuple<string, string>(d, f))) // TODO: Change to Submission!
                .SelectMany(t => Directory.GetFiles(t.Item1, t.Item2))
                .Concat(cfg.ExtraFiles);

            if(cfg.Include.Count > 0)
            {
                var tmp = cfg.Include.Select(i => new Regex(i)).ToList();
                qry = qry
                    .Where(f => tmp.Any(r => r.Match(f).Success));
            }

            if (cfg.Exclude.Count > 0)
            {
                var tmp = cfg.Exclude.Select(i => new Regex(i)).ToList();
                qry = qry
                    .Where(f => !tmp.Any(r => r.Match(f).Success));
            }

            var files = qry
                .Select(f =>
                {
                    var s = new Submission(f, tokenizer);
                    s.Parse();
                    return s;
                })
                .ToArray();

            if (cfg.Verbose)
            {
                Console.WriteLine("Files:");
                foreach (var f in files)
                {
                    Console.WriteLine(f.FilePath);
                }
                Console.WriteLine();
            }

            return files;
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
            Console.WriteLine("  OSPC -d c:\\somedir -f *.c");
            Console.WriteLine();
            Console.WriteLine("    Checks all *.c files in somedir.");
            Console.WriteLine();
            Console.WriteLine("  OSPC c:\\somedir\\file1.c c:\\somedir\\file2.c");
            Console.WriteLine();
            Console.WriteLine("    Checks file1.c and file2.c using absolute paths.");
            Console.WriteLine();
            Console.WriteLine("  OSPC a.c b.c");
            Console.WriteLine();
            Console.WriteLine("    Checks file1.c and file2.c using relative paths.");
            Console.WriteLine();
            Console.WriteLine("  OSPC --summay --html -f *.c");
            Console.WriteLine();
            Console.WriteLine("    Checks all c-files in the current directory and output a html report to .\\report\\index.html.");
        }
    }
}
