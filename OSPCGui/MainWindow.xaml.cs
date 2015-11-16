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

using OSPCGui.Controls;
using OSPCGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSPCGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IHasViewModel<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();
            ViewModel.Messages.CollectionChanged += (s, e) =>
            {
                ListBoxAutomationPeer svAutomation = (ListBoxAutomationPeer)ScrollViewerAutomationPeer.CreatePeerForElement(lstMessages);

                IScrollProvider scrollInterface = (IScrollProvider)svAutomation.GetPattern(PatternInterface.Scroll);
                System.Windows.Automation.ScrollAmount scrollVertical = System.Windows.Automation.ScrollAmount.LargeIncrement;
                System.Windows.Automation.ScrollAmount scrollHorizontal = System.Windows.Automation.ScrollAmount.NoAmount;
                //If the vertical scroller is not available, the operation cannot be performed, which will raise an exception. 
                if (scrollInterface.VerticallyScrollable)
                    scrollInterface.Scroll(scrollHorizontal, scrollVertical);
            };
        }

        public MainWindowViewModel ViewModel
        {
            get
            {
                return (MainWindowViewModel)DataContext;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
