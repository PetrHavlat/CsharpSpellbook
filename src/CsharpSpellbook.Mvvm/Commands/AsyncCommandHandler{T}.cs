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
	public class AsyncRelayCommand<T> : ICommand
	{
		private readonly Func<T, Task> _execute;
		private readonly Func<T, bool>? _canExecute;
		private readonly SemaphoreSlim _semaphore = new(1, 1);
		private CancellationTokenSource? _cts;

		public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler? CanExecuteChanged;

		public bool CanExecute(object? parameter)
		{
			if (parameter is not T typedParam)
				return false;

			return !_semaphore.CurrentCount.Equals(0) &&
				   (_canExecute?.Invoke(typedParam) ?? true);
		}

		public async void Execute(object? parameter)
		{
			if (!CanExecute(parameter) || parameter is not T typedParam)
				return;

			_cts?.Cancel();
			_cts = new CancellationTokenSource();

			try
			{
				await _semaphore.WaitAsync(_cts.Token);
				RaiseCanExecuteChanged();
				await _execute(typedParam);
			}
			catch (OperationCanceledException) { }
			finally
			{
				_semaphore.Release();
				RaiseCanExecuteChanged();
				_cts = null;
			}
		}

		/// <summary>
		/// Raises the <see cref="CanExecuteChanged"/> event.
		/// </summary>
		public void RaiseCanExecuteChanged() =>
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);

		public void Dispose()
		{
			_cts?.Cancel();
			_semaphore.Dispose();
		}
	}

}
