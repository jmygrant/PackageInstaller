using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PackageInstallerOrder
{
	public static class PackageDependencyParser
	{

		public static bool ValidateStringArray(string[] stringArray)
		{
			if (stringArray == null)
			{
				return false;
			}

			List<string> packagesAlreadyUsed = new List<string>();
			foreach (var validStr in stringArray)
			{
				if (!validStr.Contains(": "))
				{
					return false;
				}

				var index = validStr.IndexOf(":");
				if (index <= 0)
				{
					return false;
				}

				var subStr = validStr.Substring(0, index);
				if (packagesAlreadyUsed.Contains(subStr))
				{
					return false;
				}
				packagesAlreadyUsed.Add(subStr);
			}
			return true;
		}

		/// <summary>
		/// Used to recurse through our dictionary and determine the order which we should use to make sure we can install all the packages.
		/// </summary>
		/// <returns>A string that is comma delimited which will give an order of how we should install the packages.
		///  If the array passed in contains a circular dependancy we pass an empty string back.</returns>
		public static string PackageOrder(string[] stringArray)
		{
			//Read each string into a Dictionary since our keys should be unique.
			var packageOrderDictionary = FillDictionary(stringArray);

			//Recurse through Dictionary and see if any entries resolve to a package with no dependancies. Add those entries to a Queue.
			//If no entry has a no dependancies return false.
			Queue dependancyOrderQueue = new Queue();
			foreach (var keyValuePair in packageOrderDictionary)
			{
				if (string.IsNullOrEmpty(keyValuePair.Value))
				{
					dependancyOrderQueue.Enqueue(keyValuePair.Key);
				}
			}

			//If we end up not having any packages with out dependancies we will not have a install order so return empty string.
			if (dependancyOrderQueue.Count == 0)
			{
				return "All our packages have dependancies and that means we cannot create an\n install order and possibly have a circular dependancy.\n Please correct and retry.\n";
			}

			//Removing from the package dictionary packages that have no dependacies since we alreay added those to the queue.
			foreach (string queue in dependancyOrderQueue)
			{
				if (packageOrderDictionary.ContainsKey(queue))
				{
					packageOrderDictionary.Remove(queue);
				}
			}

			//Recurse through the Dictionary and walk down the needed depedancies to make sure that there is not circular depedancy.
			Stack tempOrderStack = new Stack();
			foreach (var keyValuePair in packageOrderDictionary)
			{
				//Created a copy since I do not want to change the original keyValuePair
				var CopyKeyValuePair = new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value);
				do
				{
					//I check to see if the value is in the queue since the dictionary is set up to have 
					//a package as the key and depedancy is the value.
					if (dependancyOrderQueue.Contains(CopyKeyValuePair.Value))
					{
						//Used to make sure that we do not try and add a key that is already in the queue.
						if (!dependancyOrderQueue.Contains(CopyKeyValuePair.Key))
						{
							//Add key to the queue.
							dependancyOrderQueue.Enqueue(CopyKeyValuePair.Key);
							//Recurse through the stack as we now have added the package needed and 
							//we can now add all the values that were depedant on that package.
							while (tempOrderStack.Count > 0)
							{
								dependancyOrderQueue.Enqueue(tempOrderStack.Pop());
							}
						}
					}
					else
					{
						//Created a temporary stack so that as we traverse the dependancies we can keep track of what packages had 
						// depedancies. I used a stack because it is last in last out meaning that I will get the package needed
						// first and through popping the stack I can work through the stack of packages dependant on the package
						// at the top of the stack being added first.
						if (!tempOrderStack.Contains(CopyKeyValuePair.Key))
						{
							tempOrderStack.Push(CopyKeyValuePair.Key);
							if (packageOrderDictionary.ContainsKey(CopyKeyValuePair.Value))
							{
								CopyKeyValuePair = new KeyValuePair<string, string>(CopyKeyValuePair.Value,
									packageOrderDictionary[CopyKeyValuePair.Value]);
							}
							else
							{
								return "This dependancy is not found amoungst the packages and cannot be\n installed. Please correct the input and retry the function.\n";
							}
						}
						else
						{
							//If we already have the package in the stack then we have a circular depandancy and we should stop.
							return "We have found a Circular dependancy. Please correct the input a retry the function.\n";
						}
					}
				} while (!dependancyOrderQueue.Contains(CopyKeyValuePair.Key));
			}

			//Creating the string that holds the package installation order.
			string dependancyOrderString = string.Empty;

			foreach (var queueOrder in dependancyOrderQueue)
			{
				dependancyOrderString += queueOrder + ", ";
			}

			return dependancyOrderString.TrimEnd(", ".ToCharArray());
		}

		public static Dictionary<string, string> FillDictionary(string[] stringArray)
		{
			var packageOrderDictionary = new Dictionary<string, string>();
			foreach (var str in stringArray)
			{
				var removeLeadingSpaces = str.TrimStart(' ');
				var removeNonAlphaNumericCharaters = new Regex("[^A-Za-z0-9: ]*").Replace(removeLeadingSpaces, "");
				var splitStr = removeNonAlphaNumericCharaters.Split(": ".ToCharArray());
				packageOrderDictionary.Add(splitStr[0], splitStr[2]);
			}
			return packageOrderDictionary;
		}
	}
}
