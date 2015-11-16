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

namespace OSPCGui.Commands
{
    using ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Wrap a CommandViewModel  into a SmartRoutedUICommand.
    /// </summary>
    public class WrappedCommandViewModel
        : SmartRoutedUICommand
    {
        /// <summary>
        /// Initializes a new instance of the WrappedZetboxCommand class.
        /// </summary>
        /// <param name="cmd">the command to wrap</param>
        public WrappedCommandViewModel(ICommandViewModel cmd)
            : base(cmd == null ? String.Empty : cmd.Label, typeof(WrappedCommandViewModel))
        {
            if (cmd == null) { throw new ArgumentNullException("cmd", "No command to wrap"); }

            _command = cmd;
            _command.CanExecuteChanged += (sender, args) => CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// The wrapped command.
        /// </summary>
        private ICommandViewModel _command;

        /// <inheritdoc/>
        protected override bool CanExecuteCore(object parameter)
        {
            return _command.CanExecute(parameter);
        }

        /// <inheritdoc/>
        protected override void ExecuteCore(object parameter)
        {
            _command.Execute(parameter);
        }
    }
}
