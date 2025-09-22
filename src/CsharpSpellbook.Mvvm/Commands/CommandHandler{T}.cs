using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Havlat.CsharpSpellbook.Mvvm.Commands
{
	/// <summary>
	/// A generic command implementation that relays its functionality to the provided delegates.
	/// </summary>
	/// <typeparam name="T">The type of the command parameter.</typeparam>
	public class CommandHandler<T> : ICommand
	{
		#region Fields

		private readonly Func<T, bool> _canExecute;
		private readonly Action<T> _execute;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandHandler{T}"/> class.
		/// </summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
		public CommandHandler(Action<T> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandHandler{T}"/> class.
		/// </summary>
		/// <param name="execute">The action to execute when the command is invoked.</param>
		/// <param name="canExecute">The function that determines whether the command can execute.</param>
		/// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
		public CommandHandler(Action<T> execute, Func<T, bool> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public Func<T, bool> CanExecute1 => _canExecute;

		public Action<T> Execute1 => _execute;

		#endregion

		#region ICommand Implementation

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		/// <returns>True if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			if (CanExecute1 == null)
				return true;

			if (parameter == null && typeof(T).IsValueType)
				return false;

			return CanExecute1((T)parameter);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		public void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				Execute1((T)parameter);
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
