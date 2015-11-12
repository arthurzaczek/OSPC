
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
