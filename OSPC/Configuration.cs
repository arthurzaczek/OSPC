using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public class Configuration
    {
        /// <summary>
        /// Minimum count of matching tokens, including non-matching tokens.
        /// </summary>
        public int MIN_MATCH_LENGTH = 10;
        /// <summary>
        /// Maximum distance between tokens to count as a match. 1 = exact match.
        /// </summary>
        public int MAX_MATCH_DISTANCE = 2;
        /// <summary>
        /// Percent of token that must match to count as a match. 1 = every token must match.
        /// </summary>
        public double MIN_COMMON_TOKEN = 0.85;

        /// <summary>
        /// Min. similarity of the other submission to count as contribution by a friend. if &lt; 0 then the value will be calculated automatically.
        /// </summary>
        public double MIN_FRIEND_FINDER_SIMILARITY = -1;
    }
}
