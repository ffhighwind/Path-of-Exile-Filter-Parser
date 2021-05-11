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
			//string filename = "NeverSink's filter - 0-SOFT.filter";
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

				foreach (IFilterRule rule in block.Rules) {
					string text = rule.ToString();
					switch (rule.Type) {
						case FilterType.DisabledBlock:
						case FilterType.WhiteSpace:
						case FilterType.Show:
						case FilterType.Hide:
							text = "  " + text;
							break;
						default:
							text = "\t  " + text;
							break;
					}
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

			foreach (RuleBlock block in poeFile.Blocks) {
				// Rules can be categorized into WhiteSpace, Blocks, Criteria, Actions, and Errors
				// See: https://www.pathofexile.com/item-filter/about
				List<IFilterRule> rules = block.Rules;
				if (block.IsDisabled) {
					rules = rules.Where(r => r.Type != FilterType.WhiteSpace).Select(r => ((DisabledBlock) r).Rule)
						.Where(r => r.Type != FilterType.WhiteSpace).OrderBy(r => r.Type).ToList();
				}
				else {
					rules = block.Rules.Where(r => r.Type != FilterType.WhiteSpace).OrderBy(r => r.Type).ToList();
				}

				foreach (string line in rules.Select(r => r.Type == FilterType.Show || r.Type == FilterType.Hide ? r.ToString() : "\t" + r.ToString())) {
					string text = line;
					if (text.Length > maxRuleLength)
						text = line.Substring(0, maxRuleLength - 3) + "...";
					if (block.IsDisabled)
						Console.Write("#");
					Console.WriteLine(text);
				}
				Console.WriteLine();
			}
		}

	}
}
