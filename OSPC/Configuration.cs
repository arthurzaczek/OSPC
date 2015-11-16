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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OSPC
{
    [XmlRoot("OSPC")]
    public class Configuration
    {
        public Configuration()
        {
            MIN_MATCH_LENGTH = 1000;
            MAX_MATCH_DISTANCE = 1;
            MIN_COMMON_TOKEN = 1;
            MIN_FRIEND_FINDER_SIMILARITY = -1;

            Filter = new List<string>();
            Dirs = new List<string>();
            Include = new List<string>();
            Exclude = new List<string>();
            IncludeDir = new List<string>();
            ExcludeDir = new List<string>();
        }
        /// <summary>
        /// Minimum count of matching tokens, including non-matching tokens.
        /// </summary>
        public int MIN_MATCH_LENGTH { get; set; }
        /// <summary>
        /// Maximum distance between tokens to count as a match. 1 = exact match.
        /// </summary>
        public int MAX_MATCH_DISTANCE { get; set; }
        /// <summary>
        /// Percent of token that must match to count as a match. 1 = every token must match.
        /// </summary>
        public double MIN_COMMON_TOKEN { get; set; }

        /// <summary>
        /// Min. similarity of the other submission to count as contribution by a friend. if &lt; 0 then the value will be calculated automatically.
        /// </summary>
        public double MIN_FRIEND_FINDER_SIMILARITY { get; set; }

        [XmlIgnore]
        public bool Verbose { get; set; }

        [XmlIgnore]
        public List<string> Filter { get; private set; }
        [XmlIgnore]
        public List<string> Dirs { get; private set; }
        [XmlIgnore]
        public bool Recurse { get; set; }
        [XmlIgnore]
        public List<string> ExtraFiles { get; set; }
        [XmlElement]
        public List<string> Include { get; private set; }
        [XmlElement]
        public List<string> Exclude { get; private set; }
        [XmlElement]
        public List<string> IncludeDir { get; private set; }
        [XmlElement]
        public List<string> ExcludeDir { get; private set; }
    }
}
