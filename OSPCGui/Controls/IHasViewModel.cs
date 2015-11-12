using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSPCGui.Controls
{
    /// <summary>
    /// Marker interface for the expected ViewModel of the marked Control
    /// </summary>
    public interface IHasViewModel<T>
    {
        T ViewModel { get; }
    }
}
