using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Reporter
{
    public class SummaryConsoleReporter : IReporter
    {
        public void Create(OSPCResult r)
        {
            Console.WriteLine("Resultset: {0}", r.Results.Count);
            Console.WriteLine("Max Matches: {0}", r.Results.Max(m => m.MatchCount));
            Console.WriteLine("Max Tokens: {0}", r.Results.Max(m => m.TokenCount));
        }
    }
}
