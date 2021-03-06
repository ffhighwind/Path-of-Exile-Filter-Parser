## Information

This is a parser for Path of Exile's loot filters that performs a simple syntax validation. It will not validate Classes, BaseTypes, Prophecies, Enchantments, mods, etc. These validations may be added in the future.

## Example

The following code was taken from the ParserExample project.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PathOfExile.Filter;

namespace PathOfExile
{
public class Program
{
public static void Main(string[] args)
{
	string filename = "myfilter.filter";
	PrintFile(filename);
	Console.WriteLine();
	Console.WriteLine("Press any key to continue...");			
	Console.ReadLine();
}

private static void PrintFile(string filename, int maxRuleLength = 100)
{
	using (TextReader reader = File.OpenText(filename)) {
		PoeFilterParser parser = new PoeFilterParser();
		parser.TextReader = reader;

		// Note: parser.Next() is naive and will not find all errors.
		// Use parser.Parse(filename) to ensure that all errors are picked up.
		IFilterRule rule;
		while ((rule = parser.Next()) != null) {
			string ruleType = rule.Type.ToString();
			string ruleText = rule.ToString();
			switch (rule.Type) {
				case FilterType.WhiteSpace:
					ruleType = "";
					break;
				case FilterType.DisabledBlock:
					DisabledBlock disabledBlock = (DisabledBlock) rule;
					ruleType = "#" + disabledBlock.Rule.Type;
					break;
				case FilterType.ParseError:
					ParseError parseError = (ParseError) rule;
					if (parseError.Column > 0)
						ruleText = "..." + ruleText.Substring(parseError.Column);
					break;
				case FilterType.Show:
				case FilterType.Hide:
					break;
				default:
					ruleText = "\t" + ruleText;
					break;
			}
			ruleText = $"{ parser.LineNumber,-5} {ruleType,-22} {ruleText}";
			if (ruleText.Length > maxRuleLength)
				ruleText = ruleText.Substring(0, maxRuleLength - 3) + "...";
			Console.WriteLine(ruleText);
		}
	}
}

}
}


```

## License

*The Apache 2.0 License*

Copyright (c) 2021 Wesley Hamilton
 
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
