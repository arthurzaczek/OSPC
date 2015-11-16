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
