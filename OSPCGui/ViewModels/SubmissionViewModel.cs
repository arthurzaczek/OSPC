using OSPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPCGui.ViewModels
{
    public class SubmissionViewModel : ViewModel
    {
        public SubmissionViewModel(Submission s)
        {
            if (s == null) throw new ArgumentNullException("s");
            this.Submission = s;
        }

        public Submission Submission { get; private set; }

        public override string ToString()
        {
            return Submission.ToString();
        }
    }
}
