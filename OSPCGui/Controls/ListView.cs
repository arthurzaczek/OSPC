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


namespace OSPCGui.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using OSPC;
    using OSPCGui.ViewModels;
    using System.Windows.Data;

    public class ListView : System.Windows.Controls.ListView
    {
        public SearchViewModel ViewModel
        {
            get
            {
                return DataContext as SearchViewModel;
            }
        }

        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ActivateItems();
                e.Handled = true;
            }
        }

        #region Columns Support
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (ViewModel != null && e.Property == FrameworkElement.DataContextProperty)
            {
                RefreshGridView(ViewModel.DisplayedColumns);
            }
        }

        public void RefreshGridView(GridDisplayConfiguration cfg)
        {
            cfg.Columns.CollectionChanged += (s, e) => RefreshGridView(cfg);

            GridView view = new GridView() { AllowsColumnReorder = true };
            this.View = view;

            foreach (var desc in cfg.Columns)
            {
                var col = new GridViewColumn() { Header = desc.Header };
                if (desc.RequestedWidth > 0) col.Width = desc.RequestedWidth;

                DataTemplate result = new DataTemplate();
                var cpFef = new FrameworkElementFactory(typeof(ContentPresenter));
                cpFef.SetBinding(ContentPresenter.ContentProperty, new Binding() { Path = new PropertyPath(desc.Name), Mode = BindingMode.OneWay });
                cpFef.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                result.VisualTree = cpFef;
                col.CellTemplate = result;
                view.Columns.Add(col);
            }
        }
        #endregion

        #region SelectionChanged
        public object SelectedViewModels
        {
            get { return (object)GetValue(SelectedViewModelsProperty); }
            set { SetValue(SelectedViewModelsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedZBoxItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedViewModelsProperty =
            DependencyProperty.Register("SelectedViewModels", typeof(object), typeof(ListView), new UIPropertyMetadata(null, new PropertyChangedCallback(OnSelectedViewModelsChanged)));

        public static void OnSelectedViewModelsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ListView)
            {
                ((ListView)obj).AttachSelectedViewModelsCollectionChanged();
            }
        }

        private void AttachSelectedViewModelsCollectionChanged()
        {
            if (SelectedViewModels is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)SelectedViewModels).CollectionChanged += new NotifyCollectionChangedEventHandler(list_CollectionChanged);
                try
                {
                    _selectedItemsChangedByViewModel = true;
                    ((IEnumerable)SelectedViewModels).ForEach<object>(i => this.SelectedItems.Add(i));
                }
                finally
                {
                    _selectedItemsChangedByViewModel = false;
                }
            }
        }

        private bool _selectedItemsChangedByViewModel = false;
        private bool _selectedItemsChangedByList = false;

        private void list_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_selectedItemsChangedByList) return;

            _selectedItemsChangedByViewModel = true;
            try
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    // do not touch SelectedItems in Single SelectionMode
                    if (this.SelectionMode == SelectionMode.Single)
                    {
                        this.SelectedItem = null;
                    }
                    else
                    {
                        this.SelectedItems.Clear();
                    }
                }
                else
                {
                    // do not touch SelectedItems in Single SelectionMode
                    if (this.SelectionMode == SelectionMode.Single)
                    {
                        this.SelectedItem = e.NewItems != null ? e.NewItems[0] : null;
                    }
                    else
                    {
                        if (e.OldItems != null) e.OldItems.ForEach<object>(i => this.SelectedItems.Remove(i));
                        if (e.NewItems != null) e.NewItems.ForEach<object>(i => this.SelectedItems.Add(i));
                    }
                }
            }
            finally
            {
                _selectedItemsChangedByViewModel = false;
            }
        }

        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (_selectedItemsChangedByViewModel) return;

            _selectedItemsChangedByList = true;
            try
            {
                if (e.OriginalSource == this)
                {
                    e.Handled = true;
                    if (SelectedViewModels is IList)
                    {
                        var lst = (IList)SelectedViewModels;
                        e.RemovedItems.OfType<object>().ForEach(i => lst.Remove(i));
                        e.AddedItems.OfType<object>().ForEach(i => lst.Add(i));
                    }
                    else if (SelectedViewModels is ICollection)
                    {
                        var lst = (ICollection)SelectedViewModels;
                        e.RemovedItems.OfType<object>().ForEach(i => lst.Remove(i));
                        e.AddedItems.OfType<object>().ForEach(i => lst.Add(i, true));
                    }
                }
            }
            finally
            {
                _selectedItemsChangedByList = false;
            }
        }
        #endregion
    }
}
