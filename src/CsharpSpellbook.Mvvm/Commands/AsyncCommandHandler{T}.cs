using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Havlat.CsharpSpellbook.Mvvm.Commands
{
	/// <summary>
	/// Provides a generic asynchronous command functionality.
	/// </summary>
	/// <typeparam name="T">The type of the command parameter.</typeparam>
	public class AsyncCommandHandler<T> : ICommand
	{
		#region Fields

		private readonly Func<T, bool> _canExecute;
		private readonly Func<T, Task> _execute;
		private bool _isExecuting;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandHandler{T}"/> class.
		/// </summary>
		/// <param name="execute">The asynchronous function to execute when the command is invoked.</param>
		/// <param name="canExecute">The function that determines whether the command can execute.</param>
		/// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
		public AsyncCommandHandler(Func<T, Task> execute, Func<T, bool> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandHandler{T}"/> class.
		/// </summary>
		/// <param name="execute">The asynchronous function to execute when the command is invoked.</param>
		/// <exception cref="ArgumentNullException">Thrown when execute is null.</exception>
		public AsyncCommandHandler(Func<T, Task> execute)
			: this(execute, null)
		{
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
		/// <param name="parameter">Data used by the command.</param>
		/// <returns>True if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			if (parameter == null && typeof(T).IsValueType)
				return false;

			return _canExecute((T)parameter);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameter">Data used by the command.</param>
		public async void Execute(object parameter)
		{
			var exec = _isExecuting ? 1 : 0;

			if (!CanExecute(parameter)) return;

			// Atomic check and set
			if (Interlocked.CompareExchange(ref exec, 1, 0) != 0)
				return;

			try
			{
				RaiseCanExecuteChanged();
				await _execute((T)parameter);
			}
			finally
			{
				Interlocked.Exchange(ref exec, 0);
				RaiseCanExecuteChanged();
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
