using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Reporter
{
    public class DetailedConsoleReporter : IReporter
    {
        public void Create(OSPCResult r)
        {
            foreach (var result in r.Results.Take(10))
            {
                Console.WriteLine(result.A.FilePath);
                Console.WriteLine(result.B.FilePath);
                Console.WriteLine("----------------------------");
                Console.WriteLine("Matches: {0}", result.MatchCount);
                Console.WriteLine("Tokens: {0}", result.TokenCount);
                Console.WriteLine("% A: {0:n2}", 100.0 * result.SimilarityA);
                Console.WriteLine("% B: {0:n2}", 100.0 * result.SimilarityB);
                foreach (var m in result.Matches)
                {
                    Console.WriteLine("  !: " + m.ToString());
                }
                Console.WriteLine();
            }
        }
    }
}
