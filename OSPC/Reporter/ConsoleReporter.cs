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
    public class ConsoleReporter : IReporter
    {
        public void Create(Configuration cfg, OSPCResult r)
        {
            Console.WriteLine("{0, -20} {1,8} {2, -20} {3,8} {4, 8} {5, 8}", "A", "% A", "B", "% B", "Matches", "Tokens");

            foreach (var result in r.Results.Take(100))
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
