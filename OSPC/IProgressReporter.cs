using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public interface IProgressReporter
    {
        void Start();
        void Progress(double p);
        void End();
    }

    public class ConsoleProgressReporter : IProgressReporter
    {
        private int progressCounter = 0;
        public void Start()
        {
            progressCounter = 0;
        }


        public void Progress(double p)
        {
            if (++progressCounter % 100 == 0) Console.Write(".");
        }

        
        public void End()
        {
            Console.WriteLine();
        }
    }
}
