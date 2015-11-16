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
