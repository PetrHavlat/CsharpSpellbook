using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Havlat.CsharpSpellbook.Mvvm.Commands
{
	// <summary>
	/// A command implementation that relays its functionality to the provided delegates.
	/// </summary>
	public class CommandHandler : ICommand
	{
		#region Fields

		private readonly Func<bool> _canExecute;
		private readonly Action _execute;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandHandler"/> class.
		/// </summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
		public CommandHandler(Action execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandHandler"/> class.
		/// </summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <param name="canExecute">The function that determines whether the command can execute.</param>
		/// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
		public CommandHandler(Action execute, Func<bool> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand Implementation

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command. Not used in this implementation.</param>
		/// <returns>True if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute();
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameter">Data used by the command. Not used in this implementation.</param>
		public void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				_execute();
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged"/> event.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			OnCanExecuteChanged(EventArgs.Empty);
		}

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged"/> event with the provided arguments.
		/// </summary>
		/// <param name="e">Arguments of the event being raised.</param>
		protected virtual void OnCanExecuteChanged(EventArgs e)
		{
			CanExecuteChanged?.Invoke(this, e);
		}

		#endregion
	}

	
}

