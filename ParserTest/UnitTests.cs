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
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PathOfExile.Filter;

namespace PathOfExile
{
	[TestClass]
	public class UnitTests
	{
		private static void TestFile(string filename)
		{
			PoeFilterFile poeFile;
			using (TextReader reader = File.OpenText(filename)) {
				PoeFilterParser parser = new PoeFilterParser();
				poeFile = parser.Parse(reader);
			}
			foreach (RuleBlock filter in poeFile.Blocks) {
				for(int i = 0; i < filter.Rules.Count; i++) {
					IFilterRule rule = filter.Rules[i];
					if (rule.Type == FilterType.ParseError)
						throw new InvalidOperationException(rule.Type + ": " + rule.Text);
					if (rule.Type == FilterType.WhiteSpace)
						continue;
					if (rule is DisabledBlock disabledBlock) {
						rule = disabledBlock.Rule;
						// Disabled blocks are commented rules
					}
					if (rule.Type == FilterType.Show || rule.Type == FilterType.Hide)
						continue;
					if (rule is IFilterCriteria _)
						continue;
					if (rule is IFilterAction _)
						continue;
					throw new InvalidOperationException(); // must be show/hide, action, or criteria
				}
			}
		}

		[TestMethod]
		public void S1_Regular_Highwind()
		{
			TestFile("S1_Regular_Highwind.filter");
		}

		[TestMethod]
		public void Neversink_Soft()
		{
			TestFile("NeverSink's filter - 0-SOFT.filter");
		}

		[TestMethod]
		public void Neversink_UberPlusStrict()
		{
			TestFile("NeverSink's filter - 6-UBER-PLUS-STRICT.filter");
		}
	}
}
