using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    class Program
    {
        static void Main(string[] args)
        {
            var filter = new List<string>();
            var dirs = new List<string>();
            var cfg = new Configuration();
            bool showHelp = false;
            Reporter.IReporter html = null;
            Reporter.IReporter console = new Reporter.ConsoleReporter();

            Console.WriteLine("Open Software Plagiarism Checker");
            Console.WriteLine("================================");
            Console.WriteLine();

            var p = new OptionSet()
            {
                { "h|?|help", "Prints this help", v => showHelp = true },

                { "f=", "File filter. If -d is specified, then -f defaults to \"*.*.\"", v => filter.Add(v) },
                { "d=", "Specifies a directory where the filer applies. If -f is specified, then -d defaults to \".\"", v => dirs.Add(v) },

                { "detailed", "Print a detailed report to the console", v => console = new Reporter.DetailedConsoleReporter() },
                { "summary", "Print only a summay to the console. Usefull if --html is used.", v => console = new Reporter.SummaryConsoleReporter() },
                { "html:", "Saves a html report to the specified directory. Defaults to \"report\"", v => html = new Reporter.Html.HtmlReporter(v) },

                { "min-match-length=", "Minimum count of matching tokens, including non-matching tokens.", v => cfg.MIN_MATCH_LENGTH = int.Parse(v) },
                { "max-match-distance=", "Maximum distance between tokens to count as a match. 1 = exact match.", v => cfg.MAX_MATCH_DISTANCE = int.Parse(v) },
                { "min-common-token=", "Percent of token that must match to count as a match. 1 = every token must match.", v =>  cfg.MIN_COMMON_TOKEN = double.Parse(v) },
            };

            var extra = p.Parse(args);

            if(showHelp)
            {
                ShowHelp(p);
                return;
            }

            if (filter.Count == 0 && dirs.Count > 0)
            {
                filter.Add("*.*");
            }
            if (dirs.Count == 0 && filter.Count > 0)
            {
                dirs.Add(".");
            }

            var tokenizer = new Tokenizer();
            var comparer = new Comparer(cfg);
            var friendfinder = new FriendFinder(cfg);
            var result = new OSPCResult();

            var files = dirs
                .SelectMany(d => filter.Select(f => new Tuple<string, string>(d, f))) // TODO: Change to Submission!
                .SelectMany(t => Directory.GetFiles(t.Item1, t.Item2))
                .Concat(extra)
                .ToArray();

            var compareList = new List<Tuple<string, string>>();
            for (int a = 0; a < files.Length; a++)
            {
                for (int b = a + 1; b < files.Length; b++)
                {
                    if (Path.GetExtension(files[a]) != Path.GetExtension(files[b])) continue;

                    compareList.Add(new Tuple<string, string>(files[a], files[b]));
                }
            }

            var compareResult = new List<CompareResult>();

            Console.Write("Comparing {0} files ", files.Length);

            var watch = new Stopwatch();
            watch.Start();
            int progressCounter = 0;
            object _lock = new object();

            Parallel.ForEach(compareList, pair =>
            {
                if (Path.GetExtension(pair.Item1) != Path.GetExtension(pair.Item2)) return;

                var s1 = new Submission(pair.Item1, tokenizer);
                s1.Parse();

                var s2 = new Submission(pair.Item2, tokenizer);
                s2.Parse();

                var r = comparer.Compare(s1, s2);

                lock (_lock)
                {
                    compareResult.Add(r);
                    if (++progressCounter % 100 == 0) Console.Write(".");
                }
            });

            Console.WriteLine();

            Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);

            Console.WriteLine("Creating statistics");
            result.Results = compareResult
                .Where(r => r.MatchCount > 0)
                .OrderByDescending(r => Math.Max(r.SimilarityA, r.SimilarityB))
                .ToList();

            double[] lst;

            lst = result.Results.SelectMany(i => new[] { i.SimilarityA, i.SimilarityB }).OrderBy(i => i).ToArray();
            result.AVG_Similarity = lst.Average();
            result.POI_Similarity = lst[lst.CalcDerv2().MaxIndex()];

            lst = result.Results.Select(i => (double)i.TokenCount).OrderBy(i => i).ToArray();
            result.AVG_TokenCount = lst.Average();
            result.POI_TokenCount = lst[lst.CalcDerv2().MaxIndex()];

            lst = result.Results.Select(i => (double)i.TokenCount / (double)i.MatchCount).OrderBy(i => i).ToArray();
            result.AVG_TokenPerMatch = lst.Average();
            result.POI_TokenPerMatch = lst[lst.CalcDerv2().MaxIndex()];

            if(cfg.MIN_FRIEND_FINDER_SIMILARITY < 0)
            {
                cfg.MIN_FRIEND_FINDER_SIMILARITY = result.POI_Similarity - 0.2;
            }

            result.Friends = friendfinder.Find(compareResult);

            Console.WriteLine("  finished; time: {0:n2} sec.", watch.Elapsed.TotalSeconds);

            Console.WriteLine("Creating reports");
            if (html != null)
            {
                html.Create(result);
            }
            console.Create(result);

            Console.WriteLine("  finished in total {0:n2} sec.", watch.Elapsed.TotalSeconds);
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
