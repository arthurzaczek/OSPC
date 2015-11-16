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

using OSPCGui.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OSPCGui.Commands
{/// <summary>
 /// Converts a Zetbox ICommandViewModel to a WPF ICommand
 /// </summary>
    [ValueConversion(typeof(ICommandViewModel), typeof(System.Windows.Input.ICommand))]
    public class CommandViewModelConverter
        : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a ICommandViewModel into a System.Windows.Input.ICommand by using the WrappedCommandViewModel/>.
        /// </summary>
        /// <param name="value">the command to wrap</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>A new <see cref="System.Windows.Input.ICommand"/> acting like the specified <paramref name="value"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var command = value as ICommandViewModel;
            if (command != null)
            {
                return new WrappedCommandViewModel(command);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">This parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>This method doesn't return anything.</returns>
        /// <exception cref="NotImplementedException">Always.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
