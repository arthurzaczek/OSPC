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
