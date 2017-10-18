using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PackageInstallerOrder
{
	[TestFixture]
	public class PackageInstallerTests
	{
		[Test]
		public void ValidateStringArrayTest()
		{
			Assert.IsFalse(PackageDependencyParser.ValidateStringArray(null));

			//Creating a test string array. This should not be a valid array.
			var testStringArray = new string[] { "This", "is", "only", "a", "test" };
			Assert.IsFalse(PackageDependencyParser.ValidateStringArray(testStringArray));

			//Test what happens if I am missing a  ": "
			testStringArray = new string[] { "This: ", "is: only", "a", "test: " };
			Assert.IsFalse(PackageDependencyParser.ValidateStringArray(testStringArray));

			//Validate each entry in the array to make sure that it follows the correct format.
			testStringArray = new[] { "Package: Dependancy", "Package2: ", "1: 2" };
			Assert.IsTrue(PackageDependencyParser.ValidateStringArray(testStringArray));

			//Validation should not pass if there is a duplicate package.
			testStringArray = new string[] { "Package: Dependancy", "Package: " };
			Assert.IsFalse(PackageDependencyParser.ValidateStringArray(testStringArray));

			//Validation what if you have empty package name and dependacy.
			testStringArray = new string[] { ": dependacy" };
			Assert.IsFalse(PackageDependencyParser.ValidateStringArray(testStringArray));
		}

		[Test]
		public void PackageOrderTest()
		{
			var stringArray = new string[] { "KittenService: CamelChaser", "CamelChaser: " };
			Assert.AreEqual(PackageDependencyParser.PackageOrder(stringArray), "CamelChaser, KittenService");

			stringArray = new string[]
			{
				"KittenService: ",
				"Leetmeme: CyberPortal",
				"CyberPortal: Ice",
				"CamelChaser: KittenService",
				"Fraudstream: Leetmeme",
				"Ice: "

			};
			Assert.AreEqual(PackageDependencyParser.PackageOrder(stringArray),
				"KittenService, Ice, CyberPortal, Leetmeme, CamelChaser, Fraudstream");

			//Test Circular dependancy
			stringArray = new string[]
			{
				"KittenService: ",
				"Leetmeme: CyberPortal",
				"CyberPortal: Ice",
				"CamelChaser: KittenService",
				"Fraudstream: ",
				"Ice: Leetmeme"

			};
			Assert.AreEqual(PackageDependencyParser.PackageOrder(stringArray), "We have found a Circular dependancy. Please correct the input a retry the function.\n");

			stringArray = new string[]
			{
				"KittenService: ",
				"Leetmeme: CyberPortal",
				"CyberPortal: Ice",
				"CamelChaser: KittenService",
				"Fraudstream: ",
				"Ice: NewString"

			};
			Assert.AreEqual(PackageDependencyParser.PackageOrder(stringArray), "This dependancy is not found amoungst the packages and cannot be\n installed. Please correct the input and retry the function.\n");

			//Test for packages that only have dependancies
			stringArray = new string[]
			{
				"1: 2",
				"2: 3",
				"3: 4",
				"4: 5",
				"5: 6",
				"6: 7",
				"7: 8",
				"8: 9",
				"9: 10",
				"10: 11",

			};
			Assert.AreEqual(PackageDependencyParser.PackageOrder(stringArray), "All our packages have dependancies and that means we cannot create an\n install order and possibly have a circular dependancy.\n Please correct and retry.\n");

			//Create large package dependacy array to see how well my code handles large arrays.
			int maxSize = 1000;
			stringArray = new string[maxSize];
			Stack outputStack = new Stack();
			for (int count = 0; count < maxSize; count++)
			{
				var largerNumber = count + 1;
				stringArray[count] = string.Format("{0}: {1}", count, largerNumber);
				outputStack.Push(count);
			}
			stringArray[maxSize - 1] = string.Format("{0}: ", maxSize - 1);

			var outputString = string.Empty;

			foreach (var item in outputStack)
			{
				outputString += string.Format("{0}, ", item);
			}

			outputString = outputString.TrimEnd(", ".ToCharArray());
			Assert.AreEqual(PackageDependencyParser.PackageOrder(stringArray), outputString);
		}
		[Test]
		public void FillDictionaryTest()
		{
			var stringArray = new string[] { "KittenService: CamelChaser", "CamelChaser: " };
			var testDictionary = PackageDependencyParser.FillDictionary(stringArray);

			Assert.AreEqual(testDictionary["KittenService"], "CamelChaser");
			Assert.AreEqual(testDictionary["CamelChaser"], string.Empty);

			//Test case where we have leading spaces from user input Package: , Package: directory
			stringArray = new string[] { "package: ", " package1: package" };
			testDictionary = PackageDependencyParser.FillDictionary(stringArray);

			Assert.AreEqual(testDictionary["package"], string.Empty);
			Assert.AreEqual(testDictionary["package1"], "package");

			//Test the case that the user includes other characters which are valid "Package: ", "Package1: Package"
			stringArray = new string[] { "\"package: \"", "\"package1: package\"" };
			testDictionary = PackageDependencyParser.FillDictionary(stringArray);

			Assert.AreEqual(testDictionary["package"], string.Empty);
			Assert.AreEqual(testDictionary["package1"], "package");

			stringArray = new string[] { "[\"package: \"", "\"package1: package\"]" };
			testDictionary = PackageDependencyParser.FillDictionary(stringArray);

			Assert.AreEqual(testDictionary["package"], string.Empty);
			Assert.AreEqual(testDictionary["package1"], "package");
		}
	}
}
