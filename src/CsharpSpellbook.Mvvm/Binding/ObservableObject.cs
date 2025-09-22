using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Havlat.CsharpSpellbook.Mvvm.Binding
{
	/// <summary>
	/// Base class for objects that implement property change notification.
	/// Provides a dictionary-based implementation of <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public abstract class ObservableObject : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged Implementation

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">Name of the property that changed.</param>
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event for multiple properties.
		/// </summary>
		/// <param name="propertyNames">Array of property names that changed.</param>
		protected virtual void OnPropertyChanged(params string[] propertyNames)
		{
			if (PropertyChanged != null && propertyNames != null)
			{
				foreach (var name in propertyNames)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(name));
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event for all properties.
		/// </summary>
		protected virtual void OnPropertyChangedAll()
		{
			lock (_valuesLock)
			{
				foreach (var key in _values.Keys)
				{
					OnPropertyChanged(key);
				}
			}
		}

		#endregion

		#region Property Access Methods

		// Ideas based on concept of Steve Cadwallader:
		// http://www.codecadwallader.com/2013/04/05/inotifypropertychanged-1-of-3-without-the-strings/
		// http://www.codecadwallader.com/2013/04/06/inotifypropertychanged-2-of-3-without-the-backing-fields/
		// http://www.codecadwallader.com/2013/04/08/inotifypropertychanged-3-of-3-without-the-reversed-notifications/

		/// <summary>
		/// Gets the value of a property.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="propertyName">Name of the property (automatically provided by compiler).</param>
		/// <returns>The property value, or default value if not found.</returns>
		protected T GetValue<T>([CallerMemberName] string propertyName = null)
		{
			if (propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			lock (_valuesLock)
			{
				if (_values.TryGetValue(propertyName, out object value))
				{
					return (T)value;
				}
			}

			return default;
		}

		/// <summary>
		/// Sets a new value for a property with optional notification.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="newValue">New value to set.</param>
		/// <param name="notify">Whether to raise the PropertyChanged event.</param>
		/// <param name="propertyName">Name of the property (automatically provided by compiler).</param>
		/// <returns>True if the value was changed, false if the new value equals the old value.</returns>
		protected bool SetValue<T>(T newValue, bool notify, [CallerMemberName] string propertyName = null)
		{
			if (propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			bool changed = false;

			lock (_valuesLock)
			{
				if (_values.TryGetValue(propertyName, out object oldValue))
				{
					if (!EqualityComparer<T>.Default.Equals((T)oldValue, newValue))
					{
						_values[propertyName] = newValue;
						changed = true;
					}
				}
				else
				{
					_values[propertyName] = newValue;
					changed = true;
				}
			}

			if (changed && notify)
			{
				OnPropertyChanged(propertyName);
			}

			return changed;
		}

		/// <summary>
		/// Sets a new value for a property and raises the PropertyChanged event.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="newValue">New value to set.</param>
		/// <param name="propertyName">Name of the property (automatically provided by compiler).</param>
		/// <returns>True if the value was changed, false if the new value equals the old value.</returns>
		protected bool SetValue<T>(T newValue, [CallerMemberName] string propertyName = null)
		{
			return SetValue(newValue, true, propertyName);
		}

		/// <summary>
		/// Sets a new value for a property and raises the PropertyChanged event for multiple properties.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="newValue">New value to set.</param>
		/// <param name="notifiedProperties">Array of property names to notify besides the changed property.</param>
		/// <param name="propertyName">Name of the property (automatically provided by compiler).</param>
		/// <returns>True if the value was changed, false if the new value equals the old value.</returns>
		protected bool SetValue<T>(T newValue, string[] notifiedProperties, [CallerMemberName] string propertyName = null)
		{
			bool changed = SetValue(newValue, false, propertyName);

			if (changed)
			{
				if (notifiedProperties != null && notifiedProperties.Length > 0)
				{
					var propertiesToNotify = new string[notifiedProperties.Length + 1];
					propertiesToNotify[0] = propertyName;
					Array.Copy(notifiedProperties, 0, propertiesToNotify, 1, notifiedProperties.Length);

					OnPropertyChanged(propertiesToNotify);
				}
				else
				{
					OnPropertyChanged(propertyName);
				}
			}

			return changed;
		}

		/// <summary>
		/// Begins a batch update. Property change notifications will be deferred until EndUpdate is called.
		/// </summary>
		public void BeginUpdate()
		{
			Interlocked.Increment(ref _batchUpdateCount);
		}

		/// <summary>
		/// Ends a batch update. Property change notifications that were deferred will be raised.
		/// </summary>
		/// <param name="notifyChanges">Whether to raise property change events for properties that changed during the batch update.</param>
		public void EndUpdate(bool notifyChanges = true)
		{
			if (Interlocked.Decrement(ref _batchUpdateCount) == 0 && notifyChanges)
			{
				lock (_batchChangesLock)
				{
					if (_batchChangedProperties.Count > 0)
					{
						OnPropertyChanged(_batchChangedProperties.ToArray());
						_batchChangedProperties.Clear();
					}
				}
			}
		}

		#endregion

		#region Fields and Constructor

		/// <summary>
		/// Backing dictionary for property values.
		/// </summary>
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

		/// <summary>
		/// Lock object for thread safety.
		/// </summary>
		private readonly object _valuesLock = new object();

		/// <summary>
		/// Counter for batch updates.
		/// </summary>
		private int _batchUpdateCount;

		/// <summary>
		/// Lock object for batch changes.
		/// </summary>
		private readonly object _batchChangesLock = new object();

		/// <summary>
		/// Set of properties that changed during a batch update.
		/// </summary>
		private readonly HashSet<string> _batchChangedProperties = new HashSet<string>();

		/// <summary>
		/// Creates a new instance of the ObservableObject class.
		/// The default constructor DOES NOT automatically initialize properties.
		/// </summary>
		protected ObservableObject()
		{
			// Default constructor does not use reflection to initialize values
		}

		/// <summary>
		/// Creates a new instance of the ObservableObject class and initializes properties using reflection.
		/// CAUTION: This constructor has performance implications.
		/// </summary>
		/// <param name="initializeProperties">Set to true to initialize properties using reflection.</param>
		protected ObservableObject(bool initializeProperties)
		{
			if (initializeProperties)
			{
				InitializePropertiesUsingReflection();
			}
		}

		/// <summary>
		/// Initializes all properties using reflection. This method should be used with caution due to performance implications.
		/// </summary>
		protected void InitializePropertiesUsingReflection()
		{
			lock (_valuesLock)
			{
				foreach (var property in GetType().GetProperties())
				{
					if (property.CanRead && property.GetGetMethod(false) != null)
					{
						try
						{
							_values[property.Name] = property.GetValue(this);
						}
						catch (Exception)
						{
							// Ignore exceptions during initialization
						}
					}
				}
			}
		}

		#endregion
	}
}
