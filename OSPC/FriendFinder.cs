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
    public class FriendOf
    {
        public FriendOf(Submission s)
        {
            this.Submission = s;
            InMatches = new HashSet<CompareResult>();
        }
        public Submission Submission { get; private set; }

        public double SumSimilarity { get; set; }

        public HashSet<CompareResult> InMatches { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}; {1:n2}%; {2} Friends",
                Submission,
                100.0 * SumSimilarity,
                InMatches.Count);
        }
    }

    public class FriendFinder
    {
        private readonly Configuration _cfg;

        public FriendFinder(Configuration cfg)
        {
            this._cfg = cfg;
        }

        public void Find(OSPCResult result, List<CompareResult> compareResult)
        {
            var friends = new Dictionary<Submission, FriendOf>();
            var min_friend_finder_similarity = _cfg.MIN_FRIEND_FINDER_SIMILARITY >= 0
                ? _cfg.MIN_FRIEND_FINDER_SIMILARITY
                : result.POI_Similarity - 0.2;
            min_friend_finder_similarity = Math.Max(min_friend_finder_similarity, _cfg.MIN_SIMILARITY);

            // if result.POI_Similarity is 0, min_friend_finder_similarity may become negative.
            // So every submission is a group and every match is in that group.
            if (min_friend_finder_similarity > 0)
            {
                foreach (var cp in compareResult)
                {
                    ProcessMatch(friends, cp, cp.A, cp.SimilarityB, min_friend_finder_similarity);
                    ProcessMatch(friends, cp, cp.B, cp.SimilarityA, min_friend_finder_similarity);
                }
            }

            result.Friends = friends.Values
                .Where(i => i.InMatches.Count > 1)
                .OrderByDescending(i => i.SumSimilarity)
                .ToList();
        }

        private void ProcessMatch(Dictionary<Submission, FriendOf> friends, CompareResult result, Submission submission, double similarity, double min_friend_finder_similarity)
        {
            FriendOf f;
            if (similarity > min_friend_finder_similarity)
            {
                if (!friends.TryGetValue(submission, out f))
                {
                    friends.Add(submission, f = new FriendOf(submission));
                }
                f.SumSimilarity += similarity;
                f.InMatches.Add(result);
            }
        }
    }
}
