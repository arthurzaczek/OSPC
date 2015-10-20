using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Reporter
{
    public class SummaryConsoleReporter : IReporter
    {
        public void Create(List<CompareResult> results)
        {
            Console.WriteLine("Resultset: {0}", results.Count);
            Console.WriteLine("Max Matches: {0}", results.Max(m => m.MatchCount));
            Console.WriteLine("Max Tokens: {0}", results.Max(m => m.TokenCount));
        }
    }
}
