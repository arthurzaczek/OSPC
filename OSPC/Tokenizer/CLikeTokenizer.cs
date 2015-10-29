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
