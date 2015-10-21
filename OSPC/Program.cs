using NDesk.Options;
using System;
using System.Collections.Generic;
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
            Reporter.IReporter html = null;
            Reporter.IReporter console = new Reporter.ConsoleReporter();

            Console.WriteLine("Open Software Plagiarism Checker");
            Console.WriteLine("================================");
            Console.WriteLine();

            var p = new OptionSet()
            {
                { "h|?|help", v => ShowHelp () },
                { "f=", v => filter.Add(v) },
                { "d=", v => dirs.Add(v) },
                { "detailed", v => console = new Reporter.DetailedConsoleReporter() },
                { "summary", v => console = new Reporter.SummaryConsoleReporter() },
                { "html:", v => html = new Reporter.HtmlReporter(v) },
            };

            var extra = p.Parse(args);
            if (filter.Count == 0)
            {
                filter.Add("*.*");
            }
            if (dirs.Count == 0)
            {
                dirs.Add(".");
            }

            var tokenizer = new Tokenizer();
            var comparer = new Comparer();

            var files = dirs
                .SelectMany(d => filter.Select(f => new Tuple<string, string>(d, f)))
                .SelectMany(t => Directory.GetFiles(t.Item1, t.Item2))
                .Concat(extra)
                .ToArray();

            var results = new List<CompareResult>();

            Console.Write("Comparing {0} files ", files.Length);
            int progressCounter = 0;

            for (int a = 0; a < files.Length; a++)
            {
                for (int b = a + 1; b < files.Length; b++)
                {
                    if (Path.GetExtension(files[a]) != Path.GetExtension(files[b])) continue;

                    var s1 = new Submission(files[a], tokenizer);
                    s1.Parse();

                    var s2 = new Submission(files[b], tokenizer);
                    s2.Parse();

                    results.Add(comparer.Compare(s1, s2));
                }
                if (++progressCounter % 10 == 0) Console.Write(".");
            }

            Console.WriteLine();
            Console.WriteLine();

            results = results
                .Where(r => r.MatchCount > 0)
                .OrderByDescending(r => Math.Max(r.MatchA, r.MatchB))
                .ToList();

            if (html != null)
            {
                html.Create(results);
            }
            console.Create(results);
        }

        private static void ShowHelp()
        {
            throw new NotImplementedException();
        }
    }
}
