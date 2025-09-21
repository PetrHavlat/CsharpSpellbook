using Havlat.CsharpSpellbook.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToneGenerator.MvvmTest
{
	public class PersonViewModel : ObservableObject
	{
		public string FirstName { get => GetValue<string>(); set => SetValue(value, new string[] { "FirstName", "FullName" }); } 
		public string FamilyName { get => GetValue<string>(); set => SetValue(value, new string[] { "FamilyName", "FullName" }); } 
		public string FullName { get => $"{FirstName} {FamilyName}"; }

		public PersonViewModel() =>	InitializePropertiesUsingReflection();

		public PersonViewModel(string firstName, string lastName)
		{
			InitializePropertiesUsingReflection();
			
			FirstName = firstName;
			FamilyName = lastName;
		}		
	}
}
