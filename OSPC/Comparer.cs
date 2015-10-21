using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public class Match
    {
        public Match(CompareResult parent)
        {
            this.Result = parent;
            this.TokensA = new LinkedList<Token>();
            this.TokensB = new LinkedList<Token>();
        }
        public CompareResult Result { get; private set; }
        public LinkedList<Token> TokensA { get; private set; }
        public LinkedList<Token> TokensB { get; private set; }

        public override string ToString()
        {
            return string.Join(" ", TokensA.Select(t => t.Text));
        }
    }

    public class CompareResult
    {
        public CompareResult(Submission a, Submission b)
        {
            this.A = a;
            this.B = b;
            Matches = new LinkedList<Match>();
        }
        public Submission A { get; set; }
        public Submission B { get; set; }

        public LinkedList<Match> Matches { get; private set; }

        public void Seal()
        {
            MatchCount = Matches.Count;
            TokenCount = Matches.Sum(m => m.TokensA.Count);
            MatchA = (double)TokenCount / (double)A.Tokens.Count;
            MatchB = (double)TokenCount / (double)B.Tokens.Count;
        }

        public int MatchCount { get; private set; }
        public int TokenCount { get; private set; }
        public double MatchA { get; private set; }
        public double MatchB { get; private set; }
    }

    public class Comparer
    {
        public static readonly int MIN_MATCH_LENGTH = 5;
        public static readonly int MAX_MATCH_GAP = 1; // 0 is exact match
        public static readonly double MIN_COMMON_TOKEN = 0.85; // 1 is every token must match
        public CompareResult Compare(Submission a, Submission b)
        {
            LinkedListNode<Token> working_a, working_b;
            LinkedListNode<Token> current_a;
            int inMatch = 0;

            Match currentMatch = null;
            CompareResult result = new CompareResult(a, b);
            var matches = new LinkedList<Match>();

            working_a = current_a = a.Tokens.First;
            working_b = b.Tokens.First;

            while (current_a != null)
            {
                working_a = current_a;
                working_b = b.Tokens.First;
                inMatch = 0;
                currentMatch = null;

                while (working_b != null)
                {
                    if (working_a.Value.Text == working_b.Value.Text)
                    {
                        currentMatch = ProcessMatch(working_a, working_b, currentMatch, result);

                        working_a = working_a.Next;
                        if (working_a == null) break;

                        inMatch = 1;
                    }
                    else if (inMatch > 0 && working_a.Value.Text != working_b.Value.Text)
                    {
                        if (inMatch >= MAX_MATCH_GAP + 1)
                        {
                            FinishMatch(currentMatch, matches);
                            currentMatch = null;
                            inMatch = 0;
                        }
                        else
                        {
                            currentMatch = ProcessMatch(working_a, working_b, currentMatch, result);

                            working_a = working_a.Next;
                            if (working_a == null) break;

                            inMatch++;
                        }
                    }
                    working_b = working_b.Next;
                } // while(b)
                FinishMatch(currentMatch, matches);
                current_a = current_a.Next;
            } // while(a)

            // Find longest match
            foreach(var match in matches.OrderByDescending(i => i.TokensA.Count))
            {
                if(!result.Matches.Any(m => m.TokensA.Any(t => match.TokensA.Contains(t)) 
                                         || m.TokensB.Any(t => match.TokensB.Contains(t))))
                {
                    result.Matches.AddLast(match);
                }
            }
            result.Seal();
            return result;
        }

        private static Match ProcessMatch(LinkedListNode<Token> working_a, LinkedListNode<Token> working_b, Match currentMatch, CompareResult result)
        {
            currentMatch = EnsureMatch(currentMatch, result);
            currentMatch.TokensA.AddLast(working_a.Value);
            currentMatch.TokensB.AddLast(working_b.Value);
            return currentMatch;
        }

        private static Match EnsureMatch(Match currentMatch, CompareResult result)
        {
            if (currentMatch == null)
            {
                currentMatch = new Match(result);
            }

            return currentMatch;
        }

        private static Match FinishMatch(Match currentMatch, LinkedList<Match> matches)
        {
            if (currentMatch == null) return null;

            var allMatches = currentMatch.TokensA.Select(a => a.Text).Distinct().Count();
            var realMatches = currentMatch.TokensA.Select(a => a.Text).Intersect(currentMatch.TokensB.Select(b => b.Text)).Count();
            var p = (double)realMatches / (double)allMatches;

            if (allMatches >= MIN_MATCH_LENGTH && p >= MIN_COMMON_TOKEN)
            {
                matches.AddLast(currentMatch);
            }
            return null;
        }
    }
}