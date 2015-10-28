using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public class OSPCResult
    {
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
