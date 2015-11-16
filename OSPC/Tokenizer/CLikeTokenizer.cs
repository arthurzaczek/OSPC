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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Tokenizer
{
    public class CLikeTokenizer : BaseTokenizer
    {
        protected override bool CheckCommentStart(char c, TextReader rd, ref int pos, out string terminator)
        {
            terminator = null;
            var peek = rd.Peek();
            if(c == '/' && peek == '*')
            {
                rd.Read();
                pos += 2;
                terminator = "*/";
                return true;
            }
            else if (c == '/' && peek == '/')
            {
                rd.Read();
                pos += 2;
                terminator = "\n";
                return true;
            }
            return false;
        }

        protected override bool CheckCommentEnd(char c, TextReader rd, ref int pos, string terminator)
        {
            var peek = rd.Peek();

            if (terminator == "*/" && c == '*' && peek == '/')
            {
                rd.Read();
                pos += 2;
                return true;
            }
            else if (terminator == "\n" && c == '\n')
            {
                rd.Read();
                pos += 1;
                return true;
            }
            return false;
        }
    }
}
