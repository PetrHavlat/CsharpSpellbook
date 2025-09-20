using System;

namespace CsharpSpellbook.Mvvm
{
	/// <summary>
	/// Attribute used to mark properties that depend on other properties and should be notified when those properties change.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class DependsOn : Attribute
	{
		/// <summary>
		/// Gets the name of the property this property depends on.
		/// </summary>
		public string PropertyName { get; }

		/// <summary>
		/// Creates a new instance of DependsOnAttribute.
		/// </summary>
		/// <param name="propertyName">Name of the property this property depends on.</param>
		/// <exception cref="ArgumentNullException">Thrown when propertyName is null.</exception>
		public DependsOn(string propertyName)
		{
			PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		}
	}
}
