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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Reporter
{
    public class DetailedConsoleReporter : IReporter
    {
        public void Create(Configuration cfg, OSPCResult r)
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
