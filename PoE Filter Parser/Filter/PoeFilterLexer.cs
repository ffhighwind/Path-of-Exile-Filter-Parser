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

namespace PathOfExile.Filter
{
	public class PoeFilterLexer
	{
		public PoeFilterLexer()
		{
			Token.TokenType = TokenType.EOF;
		}

		public PoeFilterLexer(TextReader reader) : this()
		{
			Reader = reader;
		}

		private TextReader reader;
		public TextReader Reader
		{
			get => reader;
			set {
				reader = value ?? throw new ArgumentNullException(nameof(Reader));
				LineNumber = 0;
				Token.Line = "";
				Token.Start = 0;
				Token.End = 0;
				Token.TokenType = TokenType.Error;
			}
		}

		public readonly FilterToken Token = new FilterToken();
		public int LineNumber { get; private set; }

		public bool NextLine()
		{
			Token.Line = reader.ReadLine();
			if (Token.Line == null) {
				Token.TokenType = TokenType.EOF;
				return false;
			}
			LineNumber++;
			Token.Start = 0;
			Token.End = 0;
			Token.TokenType = TokenType.Error;
			return true;
		}

		public FilterToken Next()
		{
			if (Token.Line == null)
				return Token;
			if (Token.TokenType == TokenType.EOL) {
				Token.Line = reader.ReadLine();
				if (Token.Line == null) {
					Token.TokenType = TokenType.EOF;
					return Token; // end of file
				}
				LineNumber++;
				Token.End = 0;
				Token.TokenType = TokenType.Error;
			}
			Token.Start = Token.End;
			if (Token.End >= Token.Line.Length) {
				Token.TokenType = TokenType.EOL;
				return Token;
			}

			// Skip whitespace
			char ch = Token.Line[Token.End];
			Token.End++;
			switch (ch) {
				case '"':
					while (Token.End < Token.Line.Length) {
						ch = Token.Line[Token.End];
						Token.End++;
						if (ch == '"') {
							Token.TokenType = TokenType.QuotedString;
							return Token;
						}
					}
					Token.TokenType = TokenType.Error;
					return Token;
				case '#':
					Token.TokenType = TokenType.Pound;
					return Token;
				case ' ':
				case '\t':
					Token.TokenType = TokenType.WhiteSpace;
					while (Token.End < Token.Line.Length) {
						ch = Token.Line[Token.End];
						if (ch != ' ' && ch != '\t')
							return Token;
						Token.End++;
					}
					return Token;
				case '=':
					if (Token.End < Token.Line.Length && Token.Line[Token.End] == '=') {
						Token.End++;
						Token.TokenType = TokenType.EqualEqual;
						return Token;
					}
					Token.TokenType = TokenType.Equal;
					return Token;
				case '<':
					if (Token.End < Token.Line.Length && Token.Line[Token.End] == '=') {
						Token.End++;
						Token.TokenType = TokenType.LessThanOrEqual;
						return Token;
					}
					Token.TokenType = TokenType.LessThan;
					return Token;
				case '>':
					if (Token.End < Token.Line.Length && Token.Line[Token.End] == '=') {
						Token.End++;
						Token.TokenType = TokenType.GreaterThanOrEqual;
						return Token;
					}
					Token.TokenType = TokenType.GreaterThan;
					return Token;
				case '!':
					Token.TokenType = TokenType.Exclamation;
					return Token;
				case '\r':
					if (Token.End < Token.Line.Length && Token.Line[Token.End] == '\n') {
						Token.End++;
					}
					Token.TokenType = TokenType.EOL;
					return Token;
				case '\n':
					Token.TokenType = TokenType.EOL;
					return Token;
				case '-':
					Token.TokenType = TokenType.Dash;
					return Token;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					Token.TokenType = TokenType.Number;
					while (Token.End < Token.Line.Length) {
						ch = Token.Line[Token.End];
						if (!char.IsDigit(ch)) {
							if (char.IsLetter(ch) || ch == '_') {
								Token.End++;
								goto stringLabel;
							}
							break; // break before invalid character
						}
						Token.End++;
					}
					return Token;
				default:
					// NOTE: Only UTF8 is technically supported
					if (!char.IsLetter(ch)) {
						Token.TokenType = TokenType.Error;
						return Token;
					}
				stringLabel:
					Token.TokenType = TokenType.String;
					while (Token.End < Token.Line.Length) {
						ch = Token.Line[Token.End];
						if (char.IsLetterOrDigit(ch) || ch == '_') {
							Token.End++;
							continue;
						}
						break;
					}
					break;
			}
			return Token;
		}
	}
}