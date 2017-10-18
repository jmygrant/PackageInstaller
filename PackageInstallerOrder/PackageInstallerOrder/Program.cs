using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PackageInstallerOrder
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//Create string Array.
			//Read in input array.
			Console.WriteLine(
				"Please insert your package dependancy array make sure all entered \ndata looks like is Package: Dependancy. Package:  is valid input as well.\n");
			var userInput = Console.ReadLine();
			var parseUserInput = userInput.Split(',');

			//Validate the string Array.
			if (PackageDependencyParser.ValidateStringArray(parseUserInput))
			{
				//Create package install order.
				string output = PackageDependencyParser.PackageOrder(parseUserInput);

				Console.WriteLine(string.IsNullOrEmpty(output) ? "There is no valid package dependancy path.\n" : output);
				Console.ReadLine();
			}
			else
			{
				Console.WriteLine("The input information was invalid. Please restart program to try again.");
				Console.ReadLine();
			}
		}
	}


}
