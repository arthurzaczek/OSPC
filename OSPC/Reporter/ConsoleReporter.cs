using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Reporter
{
    public class ConsoleReporter : IReporter
    {
        public void Create(List<CompareResult> results)
        {
            Console.WriteLine("{0, -20} {1,8} {2, -20} {3,8} {4, 8} {5, 8}", "A", "% A", "B", "% B", "Matches", "Tokens");

            foreach (var result in results.Take(100))
            {
                Console.WriteLine("{0, -20} {1,8:n2} {2, -20} {3,8:n2} {4, 8} {4, 8}",
                    result.A.FilePath.MaxLength(17, "...", true),
                    100.0 * result.SimilarityA,
                    result.B.FilePath.MaxLength(17, "...", true),
                    100.0 * result.SimilarityB,
                    result.MatchCount,
                    result.TokenCount);
            }
        }
    }
}
