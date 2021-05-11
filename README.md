## Information

This is a parser for Path of Exile's loot filters that performs a simple syntax validation. It will not validate Classes, BaseTypes, Prophecies, Enchantments, mods, etc. These validations may be added in the future.

Neversink's filters are only used as test cases. I hold no copyright to them.

## Example

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
	Console.WriteLine("Rules printed.");
	Console.WriteLine("Press any key to continue...");			
	Console.ReadLine();
	Console.Clear();
	PrintBlocks(filename);
	Console.WriteLine();
	Console.WriteLine("Blocks printed.");
	Console.WriteLine("Press any key to continue...");
	Console.ReadLine();
	Console.Clear();
	PrintSortedBlocks(filename);
	Console.WriteLine("Sorted blocks printed.");
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
		IFilterRule rule = parser.Next();
		while (rule != null) {
			string ruleType;
			string ruleText = rule.ToString();
			if (rule.Type == FilterType.WhiteSpace)
				ruleType = "";
			else if (rule.Type == FilterType.DisabledBlock) {
				DisabledBlock disabledBlock = (DisabledBlock) rule;
				ruleType = "#" + disabledBlock.Rule.Type;
			}
			else if (rule.Type == FilterType.ParseError) {
				ParseError parseError = (ParseError) rule;
				ruleType = $"!!! ERROR !!!";
				if (parseError.Column > 0)
					ruleText = "..." + ruleText.Substring(parseError.Column);
			}
			else {
				ruleType = rule.Type.ToString();
				if (rule is IFilterCriteria _ || rule is IFilterAction _)
					ruleText = "\t" + ruleText;
				else if (rule.Type != FilterType.Show && rule.Type != FilterType.Hide)
					throw new InvalidOperationException();
			}
			ruleText = $"{ parser.LineNumber,-5} {ruleType,-22} {ruleText}";
			if (ruleText.Length > maxRuleLength)
				ruleText = ruleText.Substring(0, maxRuleLength - 3) + "...";
			Console.WriteLine(ruleText);

			rule = parser.Next();
		}
	}
}


private static void PrintBlocks(string filename, int maxRuleLength = 80)
{
	PoeFilterFile poeFile;
	using (TextReader reader = File.OpenText(filename)) {
		PoeFilterParser parser = new PoeFilterParser();
		poeFile = parser.Parse(reader);
	}

	for (int i = 0; i < poeFile.Blocks.Count; i++) {
		RuleBlock block = poeFile.Blocks[i];
		// A FilterBlock includes a group of rules after a Show/Hide.
		// It will also include some comments before and after the rule
		if (block.Rules.Count == 0)
			throw new InvalidOperationException(block.Name ?? "");

		foreach (IFilterRule rule in block.Rules) {
			string text = rule.ToString();
			if (rule.Type != FilterType.WhiteSpace && rule.Type != FilterType.Show && rule.Type != FilterType.Hide && rule.Type != FilterType.DisabledBlock) {
				text = "\t  " + text;
			}
			else
				text = "  " + text;
			if (text.Length > maxRuleLength)
				text = text.Substring(0, maxRuleLength - 3) + "...";
			Console.WriteLine(text);
		}
		Console.WriteLine("-");
	}
}

private static void PrintSortedBlocks(string filename, int maxRuleLength = 80)
{
	PoeFilterFile poeFile;
	using (TextReader reader = File.OpenText(filename)) {
		PoeFilterParser parser = new PoeFilterParser();
		poeFile = parser.Parse(reader);
	}

	for (int i = 0; i < poeFile.Blocks.Count; i++) {
		RuleBlock block = poeFile.Blocks[i];
		// A FilterBlock includes a group of rules after a Show/Hide.
		// It will also include some comments before and after the rule
		if (block.Rules.Count == 0)
			throw new InvalidOperationException(block.Name ?? "");

		// Rules can be categorized into WhiteSpace, Blocks, Criteria, Actions, and Errors
		// See: https://www.pathofexile.com/item-filter/about
		if (block.IsDisabled) {
			List<IFilterRule> rules = block.Rules.Where(r => r.Type != FilterType.WhiteSpace && ((DisabledBlock) r).Rule.Type != FilterType.WhiteSpace)
					.OrderBy(x => ((DisabledBlock) x).Rule.Type).ToList();
			foreach (IFilterRule rule in rules) {
				string text = rule.ToString();
				if (text.Length > maxRuleLength)
					text = text.Substring(0, maxRuleLength - 3) + "...";
				Console.WriteLine(text);
			}
		}
		else {
			List<IFilterRule> rules = block.Rules.Where(r => r.Type != FilterType.WhiteSpace).OrderBy(x => x.Type).ToList();
			string text = rules[0].ToString();
			if (text.Length > maxRuleLength)
				text = text.Substring(0, maxRuleLength - 3) + "...";
			Console.WriteLine(text);
			foreach (IFilterRule rule in rules.Skip(1)) {
				text = "\t" + rule.ToString();
				if (text.Length > maxRuleLength - 1)
					text = text.Substring(0, maxRuleLength - 4) + "...";
				Console.WriteLine(text);
			}
		}
		Console.WriteLine();
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