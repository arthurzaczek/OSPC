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

        public List<FriendOf> Find(List<CompareResult> results)
        {
            var friends = new Dictionary<Submission, FriendOf>();
            foreach(var result in results)
            {
                ProcessMatch(friends, result, result.A, result.SimilarityB);
                ProcessMatch(friends, result, result.B, result.SimilarityA);
            }
            
            return friends.Values
                .Where(i => i.InMatches.Count > 1)
                .OrderByDescending(i => i.SumSimilarity).ToList();
        }

        private void ProcessMatch(Dictionary<Submission, FriendOf> friends, CompareResult result, Submission submission, double similarity)
        {
            FriendOf f;
            if (similarity > _cfg.MIN_FRIEND_FINDER_SIMILARITY)
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
