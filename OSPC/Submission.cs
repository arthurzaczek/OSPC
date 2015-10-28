using OSPC.Tokenizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public class Submission
    {
        private ITokenizer _tokenizer;
        public Submission(string filePath, ITokenizer tokenizer)
        {
            this.FilePath = filePath;
            this._tokenizer = tokenizer;
        }

        public string FilePath { get; private set; }

        public Token[] Tokens { get; private set; }

        public void Parse()
        {
            using (var rd = new StreamReader(FilePath))
            {
                Tokens = _tokenizer.Split(rd);
            }
        }

        public override string ToString()
        {
            return FilePath.MaxLength(20, trimStart: true);
        }

        public override int GetHashCode()
        {
            return FilePath.GetHashCode();
        }
    }
}
