using System.Collections.Generic;

namespace OSPC.Reporter
{
    public interface IReporter
    {
        void Create(OSPCResult result);
    }
}