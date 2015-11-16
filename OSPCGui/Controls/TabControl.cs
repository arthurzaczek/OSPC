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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    
    public class TabControl : System.Windows.Controls.TabControl
    {
        public string SelectedTabName
        {
            get { return (string)GetValue(SelectedTabNameProperty); }
            set { SetValue(SelectedTabNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTabName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTabNameProperty =
            DependencyProperty.Register("SelectedTabName", typeof(string), typeof(TabControl), new PropertyMetadata(OnSelectedTabNameChanged));

        private static void OnSelectedTabNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newVal = e.NewValue as string;
            if (string.IsNullOrEmpty(newVal)) return;
            var self = (TabControl)d;
            var tab = self.Items.OfType<System.Windows.Controls.TabItem>().SingleOrDefault(i => i.Name == newVal);
            if (tab != null && !tab.IsSelected)
            {
                self.SelectedItem = tab;
            }
        }

        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems.Count > 0)
            {
                var obj = e.AddedItems.OfType<System.Windows.Controls.TabItem>().FirstOrDefault();
                SelectedTabName = obj != null ? obj.Name : string.Empty;
            }
        }
    }
}
