using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC.Tokenizer
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

    public interface ITokenizer
    {
        Token[] Split(TextReader rd);
    }

    public abstract class BaseTokenizer : ITokenizer
    {
        public static readonly char[] DEFAULT_SEPERATORS = new char[] { ' ', '\n', '\r', '\t' };
        public static readonly char[] DEFAULT_SYMBOLS = new char[] { ',', ';', '(', ')', '[', ']', '{', '}', '&', '|', '=', '<', '>', '!', '~', '+', '-', '/', '*' };

        protected virtual List<char> GetSeperators()
        {
            return new List<char>(DEFAULT_SEPERATORS);
        }

        protected virtual List<char> GetSymbols()
        {
            return new List<char>(DEFAULT_SYMBOLS);
        }

        public Token[] Split(TextReader rd)
        {
            int read;
            Token current = null;
            var result = new List<Token>();
            var seperators = GetSeperators();
            var symbols = GetSymbols();

            bool inQuote = false;
            int pos = 0;

            while ((read = rd.Read()) != -1)
            {
                var c = (char)read;
                if (seperators.Contains(c) && !inQuote)
                {
                    if (current != null)
                    {
                        current.Seal();
                        result.Add(current);
                    }

                    current = null;
                }
                else if (symbols.Contains(c) && !inQuote)
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
