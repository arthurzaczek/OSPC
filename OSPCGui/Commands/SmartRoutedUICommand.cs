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
    /// This abstract class is a RoutedCommand which allows its
    /// subclasses to provide default logic for determining if 
    /// they can execute and how to execute.  To enable the default
    /// logic to be used, set the IsCommandSink attached property
    /// to true on the root element of the element tree which uses 
    /// one or more SmartRoutedCommand subclasses.
    /// </summary>
    /// <para>
    /// SmartRoutedCommand from http://www.codeproject.com/KB/WPF/SmartRoutedCommandsInWPF.aspx
    /// modified to match local usage:
    /// <list type="bullet">
    /// <item>inherit from RoutedUICommand instead of RoutedCommand</item>
    /// <item>formatting</item>
    /// </list>
    /// </para>
    public abstract class SmartRoutedUICommand : RoutedUICommand
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SmartRoutedUICommand class, specifying a label and an owner class.
        /// </summary>
        /// <param name="text">the label to use</param>
        /// <param name="ownerType">the owner class to use</param>
        public SmartRoutedUICommand(string text, Type ownerType)
            : base(text, text, ownerType)
        {
        }

        #endregion

        #region IsCommandSink

        /// <summary>
        /// Returns a value indicating whether or not the specified <see cref="DependencyObject"/> is a command sink or not.
        /// </summary>
        /// <param name="obj">the object to check</param>
        /// <returns>true if the passed object is a command sink, false in all other cases</returns>
        public static bool GetIsCommandSink(DependencyObject obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                return (bool)obj.GetValue(IsCommandSinkProperty);
            }
        }

        /// <summary>
        /// Sets the value indicating whether or not the specified <see cref="DependencyObject"/> is a command sink or not.
        /// </summary>
        /// <param name="obj">the object to modify</param>
        /// <param name="value">true if the passed object is a command sink, false otherwise</param>
        /// <exception cref="ArgumentNullException">if <paramref name="obj"/> is <code>null</code></exception>
        public static void SetIsCommandSink(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(IsCommandSinkProperty, value);
        }

        /// <summary>
        /// Represents the IsCommandSink attached property.  This is readonly.
        /// </summary>
        public static readonly DependencyProperty IsCommandSinkProperty =
            DependencyProperty.RegisterAttached(
            "IsCommandSink",
            typeof(bool),
            typeof(SmartRoutedUICommand),
            new UIPropertyMetadata(false, OnIsCommandSinkChanged));

        /// <summary>
        /// Invoked when the IsCommandSink attached property is set on an element.
        /// </summary>
        /// <param name="depObj">The element on which the property was set.</param>
        /// <param name="e">Information about the property setting.</param>
        private static void OnIsCommandSinkChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            bool isCommandSink = (bool)e.NewValue;

            UIElement sinkElem = depObj as UIElement;
            if (sinkElem == null)
            {
                throw new ArgumentException("Target object must be a UIElement.");
            }

            if (isCommandSink)
            {
                CommandManager.AddCanExecuteHandler(sinkElem, OnCanExecute);
                CommandManager.AddExecutedHandler(sinkElem, OnExecuted);
            }
            else
            {
                CommandManager.RemoveCanExecuteHandler(sinkElem, OnCanExecute);
                CommandManager.RemoveExecutedHandler(sinkElem, OnExecuted);
            }
        }

        #endregion // IsCommandSink

        #region Static Callbacks

        /// <summary>
        /// Event handler, called when CanExecute changes.
        /// </summary>
        /// <param name="sender">the changing object</param>
        /// <param name="e">the event's arguments</param>
        private static void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            SmartRoutedUICommand cmd = e.Command as SmartRoutedUICommand;
            if (cmd != null)
            {
                e.CanExecute = cmd.CanExecuteCore(e.Parameter);
            }
        }

        /// <summary>
        /// Event handler, called when Executed changes.
        /// </summary>
        /// <param name="sender">the changing object</param>
        /// <param name="e">the event's arguments</param>
        private static void OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SmartRoutedUICommand cmd = e.Command as SmartRoutedUICommand;
            if (cmd != null)
            {
                cmd.ExecuteCore(e.Parameter);
                e.Handled = true;
            }
        }

        #endregion // Static Callbacks

        #region Abstract Methods
        /// <summary>
        /// Child classes override this method to provide logic which
        /// determines if the command can execute.  This method will 
        /// only be invoked if no element in the tree indicated that
        /// it can execute the command.
        /// </summary>
        /// <param name="parameter">The command parameter (optional).</param>
        /// <returns>True if the command can be executed, else false.</returns>
        protected abstract bool CanExecuteCore(object parameter);

        /// <summary>
        /// Child classes override this method to provide default 
        /// execution logic.  This method will only be invoked if
        /// CanExecuteCore returns true.
        /// </summary>
        /// <param name="parameter">The command parameter (optional).</param>
        protected abstract void ExecuteCore(object parameter);
        #endregion // Abstract Methods
    }
}