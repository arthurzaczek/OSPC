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
using System.Collections.ObjectModel;

namespace OSPCGui.ViewModels
{
    public abstract class SearchViewModel : ViewModel
    {
        public SearchViewModel()
        {
            Items = new ObservableCollection<ViewModel>();
            SelectedViewModels = new ObservableCollection<ViewModel>();
        }

        private string _label;
        public string Label
        {
            get
            {
                return _label ?? "Suche:";
            }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged("Label");
                }
            }
        }

        private string _seachText;
        public string SearchText
        {
            get
            {
                return _seachText;
            }
            set
            {
                if (_seachText != value)
                {
                    _seachText = value;
                    //SearchCommand.OnCanExecuteChanged();
                    OnPropertyChanged("SearchText");
                }
            }
        }

        private ICommandViewModel _searchCommand;
        public ICommandViewModel SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new SimpleCommandViewModel(
                        "Suchen", 
                        "Startet eine Suche", 
                        Search,
                        () => !string.IsNullOrEmpty(SearchText));
                }
                return _searchCommand;
            }
        }

        public abstract void Search();

        public abstract GridDisplayConfiguration DisplayedColumns { get; }

        public ObservableCollection<ViewModel> Items { get; private set; }
        public ObservableCollection<ViewModel> SelectedViewModels { get; private set; }

        public virtual void ActivateItems()
        {
        }
    }
}
