
namespace OSPCGui.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using System.Collections.ObjectModel;

    public class ColumnDisplayModel
    {
        public ColumnDisplayModel(string header, string name)
            : this(header, name, -1)
        {
        }

        public ColumnDisplayModel(string header, string name, int requestedWidth)
        {
            this.Header = header;
            this.Name = name;
            this.RequestedWidth = requestedWidth;
        }

        public string Header { get; set; }
        public string Name { get; set; }
        public int RequestedWidth { get; set; }

        public override string ToString()
        {
            return Header;
        }
    }

    public class GridDisplayConfiguration
    {
        public ObservableCollection<ColumnDisplayModel> Columns { get; private set; }

        public GridDisplayConfiguration()
        {
            Columns = new ObservableCollection<ColumnDisplayModel>();
        }

        public static GridDisplayConfiguration BuildColumns(Type viewModelType)
        {
            if (viewModelType == null) throw new ArgumentNullException("viewModelType");
           
            // TODO: Implement auto resolve of columns
            throw new NotImplementedException();
        }
    }
}
