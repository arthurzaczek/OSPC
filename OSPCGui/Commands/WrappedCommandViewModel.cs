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
