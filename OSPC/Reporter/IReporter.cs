using System.Collections.Generic;

namespace OSPC.Reporter
{
    public interface IReporter
    {
        void Create(List<CompareResult> results);
    }
}