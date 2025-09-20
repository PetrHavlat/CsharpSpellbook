using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpSpellbook.Mvvm
{
	// <summary>
	/// Attribute used to mark properties that should raise PropertyChanged events when modified.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class NotifyChange : Attribute
	{
		/// <summary>
		/// Gets the names of all properties that should be notified when this property changes.
		/// </summary>
		public string[] NotifiedProperties { get; }

		/// <summary>
		/// Creates a new instance of NotifyPropertyAttribute.
		/// </summary>
		public NotifyChange()
		{
			NotifiedProperties = Array.Empty<string>();
		}

		/// <summary>
		/// Creates a new instance of NotifyPropertyAttribute with additional properties to notify.
		/// </summary>
		/// <param name="alsoNotifyProperties">Names of additional properties that should be notified when the decorated property changes.</param>
		public NotifyChange(params string[] notifiedProperties)
		{
			NotifiedProperties = notifiedProperties ?? Array.Empty<string>();
		}
	}
}
