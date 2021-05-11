#region License
// Released under Apache 2.0 License 
// License: https://opensource.org/licenses/Apache2.0
// Home page: https://github.com/ffhighwind/Path-of-Exile-Utilities

// Copyright(c) 2021 Wesley Hamilton

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace PathOfExile.Filter
{
	public class PoeFilterFile
	{
		public PoeFilterFile() { }

		public List<RuleBlock> Blocks { get; } = new List<RuleBlock>();
		public List<ParseError> Errors { get; } = new List<ParseError>();

		public PoeFilterFile(List<IFilterRule> rules)
		{
			if (rules.Count == 0)
				return;
			int i = FixStart(rules);
			int start = 0;
			for (; ; ) {
				// add disabled blocks between start and i and change start to the new value
				start = AddDisabledBlocks(rules, start, i);
				if (i >= rules.Count)
					break;
				string name = rules[i].Comment;
				i++;
				int end = i; // the index of the last rule for the next block
				for (; i < rules.Count; i++) {
					// find next Show/Hide (start of next block)
					// start/end will store the indexes for this range
					IFilterRule rule = rules[i];
					FilterType type = rule.Type;
					if (type == FilterType.Show || type == FilterType.Hide) {
						// move the end index forward to include commented code (DisabledBlocks) before next disabled Show/Hide
						for (int j = end + 1; j < i; j++) {
							rule = rules[j];
							type = rule.Type;
							if (type == FilterType.DisabledBlock) {
								DisabledBlock disabledBlock = (DisabledBlock) rule;
								type = disabledBlock.Rule.Type;
								if (type == FilterType.Show || type == FilterType.Hide) {
									break;
								}
								end = j;
							}
						}
						break;
					}
					if (type == FilterType.WhiteSpace || type == FilterType.DisabledBlock)
						continue;
					end = i;
				}
				end++;
				// move end forward to capture empty whitespace
				for (; end < i; end++) {
					IFilterRule rule = rules[end];
					FilterType type = rule.Type;
					if (type != FilterType.WhiteSpace || rule.Comment != null)
						break;
				}
				start = AddFilterBlock(rules, name, start, end);
			}
		}

		private int AddFilterBlock(List<IFilterRule> rules, string name, int start, int end)
		{
			List<IFilterRule> list = new List<IFilterRule>(end - start + 1);
			for (; start < end; start++) {
				IFilterRule rule = rules[start];
				list.Add(rule);
			}
			if (list.Count == 0)
				throw new InvalidOperationException();
			RuleBlock block = new RuleBlock();
			block.Rules = list;
			block.Name = name;
			Blocks.Add(block);
			return end;
		}

		private int AddDisabledBlocks(List<IFilterRule> rules, int start, int end)
		{
			string name = null;
			int ruleEnd = start;
			for (int i = start; i < end; i++) {
				IFilterRule rule = rules[i];
				FilterType type = rule.Type;
				if (type == FilterType.DisabledBlock) {
					DisabledBlock disabledBlock = (DisabledBlock) rule;
					type = disabledBlock.Rule.Type;
					if (type == FilterType.Show || type == FilterType.Hide) {
						if (name != null) {
							start = AddFilterBlock(rules, name, start, ruleEnd + 1);
							Blocks.Last().IsDisabled = true;
						}
						name = rule.Text;
					}
					ruleEnd = i;
				}
				else {
					if (type != FilterType.WhiteSpace)
						throw new InvalidOperationException(); // sanity check
					if (rule.Comment == null)
						ruleEnd = i;
				}
			}
			if (name != null) {
				start = AddFilterBlock(rules, name, start, ruleEnd + 1);
				Blocks.Last().IsDisabled = true;
			}
			return start;
		}

		private static int FindEnd(List<IFilterRule> rules, int i)
		{
			for (; i > 0; i--) {
				IFilterRule rule = rules[i - 1];
				FilterType type = rule.Type;
				if (type != FilterType.WhiteSpace)
					break;
			}
			return i;
		}

		/// <summary>
		/// Iterates all rules into the first Show/Hide and changes the type for invalid rules. 
		/// DisabledBlocks are changed to comments and other rules are changed to ParseErrors.
		/// </summary>
		/// <returns>The index of the first Show/Hide rule or the index after the rules list.</returns>
		private int FixStart(List<IFilterRule> rules)
		{
			bool hasDisabledBlock = false;
			int i;
			for (i = 0; i < rules.Count; i++) {
				IFilterRule rule = rules[i];
				FilterType type = rule.Type;
				if (type == FilterType.WhiteSpace)
					continue;
				if (type == FilterType.Show || type == FilterType.Hide)
					break;
				if (type == FilterType.DisabledBlock) {
					if (hasDisabledBlock)
						continue;
					DisabledBlock disabledBlock = (DisabledBlock) rule;
					type = disabledBlock.Rule.Type;
					if (type == FilterType.Show || type == FilterType.Hide)
						hasDisabledBlock = true;
					else
						rules[i] = new WhiteSpace(); // replace disabled block with a comment since it doesn't have a Show/Hide
				}
				else {
					ParseError parseError;
					if (type != FilterType.ParseError) {
						parseError = new ParseError(i + 1, 0, rule.Text, null, type);
						rules[i] = parseError;
					}
					else
						parseError = (ParseError) rule;
					Errors.Add(parseError);
				}
			}
			return i;
		}
	}
}
