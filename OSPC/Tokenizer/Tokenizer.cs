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
            bool inComment = false;

            string quoteTerminator = null;
            string commentTerminator = null;

            int pos = 0;

            while ((read = rd.Read()) != -1)
            {
                var c = (char)read;
                var current_pos = pos;
                if(!inQuote && !inComment && CheckCommentStart(c, rd, ref pos, out commentTerminator))
                {
                    inComment = true;
                    continue;
                }
                else if(!inQuote && inComment && CheckCommentEnd(c, rd, ref pos, commentTerminator))
                {
                    inComment = false;
                    continue;
                }
                else if(!inQuote && inComment)
                {
                    // ignore and continue
                    pos++;
                    continue;
                }

                if (!inQuote && !inComment && CheckQuoteStart(c, rd, ref pos, out quoteTerminator))
                {
                    inQuote = true;
                    Reset(current, result);
                    current = new Token(current_pos);
                    current.Append(c);
                    continue;
                }
                else if (inQuote && !inComment && CheckQuoteEnd(c, rd, ref pos, quoteTerminator))
                {
                    inQuote = false;
                    current.Append(c);
                    current = Reset(current, result);
                    continue;
                }

                if (!inQuote && seperators.Contains(c))
                {
                    current = Reset(current, result);
                }
                else if (!inQuote && symbols.Contains(c))
                {
                    Reset(current, result);

                    current = new Token(current_pos);
                    current.Append(c);
                    
                    current = Reset(current, result);
                }
                else
                {
                    if (current == null)
                    {
                        current = new Token(current_pos);
                    }
                    current.Append(c);
                }                
                pos++;
            }

            return result.ToArray();
        }

        private Token Reset(Token current, List<Token> result)
        {
            if (current != null)
            {
                current.Seal();
                result.Add(current);
            }

            return null;
        }

        protected abstract bool CheckCommentStart(char c, TextReader rd, ref int pos, out string terminator);
        protected abstract bool CheckCommentEnd(char c, TextReader rd, ref int pos, string terminator);

        protected virtual bool CheckQuoteStart(char c, TextReader rd, ref int pos, out string terminator)
        {
            terminator = null;
            if(c == '\"')
            {
                pos++;
                terminator = "\"";
                return true;
            }
            else if (c == '\'')
            {
                pos++;
                terminator = "\'";
                return true;
            }
            return false;
        }
        protected virtual bool CheckQuoteEnd(char c, TextReader rd, ref int pos, string terminator)
        {
            var result = c == terminator.First();
            if (result) pos++;
            return result;
        }
    }
}
