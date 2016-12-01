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

namespace OSPC
{
    public class OSPCResult
    {
        private OSPCResult()
        {

        }

        public static OSPCResult Create(List<CompareResult> compareResult)
        {
            var result = new OSPCResult();
            result.Results = compareResult
                .Where(r => r.MatchCount > 0)
                .OrderByDescending(r => Math.Max(r.SimilarityA, r.SimilarityB))
                .ThenByDescending(r => Math.Min(r.SimilarityA, r.SimilarityB))
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
            return result;
        }
        public List<CompareResult> Results { get; set; }

        public List<FriendOf> Friends { get; set; }
        public double POI_Similarity { get; internal set; }
        public double POI_TokenCount { get; internal set; }
        public double AVG_Similarity { get; internal set; }
        public double AVG_TokenCount { get; internal set; }
        public double AVG_TokenPerMatch { get; internal set; }
        public double POI_TokenPerMatch { get; internal set; }
    }
}
