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

namespace PathOfExile.Filter
{
	/// <summary>
	/// Class implementation of a StringSegment. Includes caching of the text value.
	/// </summary>
	public sealed class FilterToken : ICloneable
	{
		internal FilterToken() { }

		public TokenType TokenType { get; internal set; }
		public int Start { get; internal set; }
		public int End { get; internal set; }
		public int Length => End - Start;
		public string Line { get; internal set; }
		public string Text => Line.Substring(Start, Length);

		public static bool operator ==(FilterToken token, string str)
		{
			if (str != null && str.Length == token.Length) {
				for (int i = 0; i < str.Length; i++) {
					char c = str[i];
					char c2 = token.Line[token.Start + i];
					if (c != c2)
						return false;
				}
				return true;
			}
			return false;
		}
		public static bool operator !=(FilterToken token, string str) => !(token == str);
		public static bool operator ==(string str, FilterToken token) => token == str;
		public static bool operator !=(string str, FilterToken token) => !(token == str);

		public void Copy(FilterToken token)
		{
			TokenType = token.TokenType;
			Start = token.Start;
			End = token.End;
			Line = token.Line;
		}

		public FilterToken Clone()
		{
			return (FilterToken) MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
