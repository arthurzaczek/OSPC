using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public class Token
    {
        private StringBuilder _text = new StringBuilder();
        private bool _sealed = false;

        public Token(int start)
        {
            this.Start = start;
        }
        public void Append(char c)
        {
            if (_sealed) throw new InvalidOperationException("Token is already sealed");
            _text.Append(c);
        }

        public void Seal()
        {
            Text = _text.ToString();
            this.Length = Text.Length;
            this.End = Start + Length;
            _sealed = true;
        }

        public string Text { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public int Length { get; private set; }
        public override string ToString()
        {
            return Text ?? _text.ToString();
        }
    }

    public class Tokenizer
    {
        public static readonly char[] SEPERATORS = new char[] { ' ', '\n', '\r', '\t' };
        public static readonly char[] SYMBOLS = new char[] { ',', ';', '(', ')', '[', ']', '{', '}', '&', '|', '=', '<', '>', '!', '~', '+', '-', '/', '*' };
        public Token[] Split(TextReader rd)
        {
            int read;
            Token current = null;
            var result = new List<Token>();

            bool inQuote = false;
            int pos = 0;

            while ((read = rd.Read()) != -1)
            {
                var c = (char)read;
                if (SEPERATORS.Contains(c) && !inQuote)
                {
                    if (current != null)
                    {
                        current.Seal();
                        result.Add(current);
                    }

                    current = null;
                }
                else if (SYMBOLS.Contains(c) && !inQuote)
                {
                    if (current != null)
                    {
                        current.Seal();
                        result.Add(current);
                    }

                    current = new Token(pos);
                    current.Append(c);
                    current.Seal();
                    result.Add(current);

                    current = null;
                }
                else
                {
                    if (current == null)
                    {
                        current = new Token(pos);
                    }
                    current.Append(c);
                }

                if (c == '\"')
                {
                    inQuote = !inQuote;
                }
                pos++;
            }

            return result.ToArray();
        }
    }
}
