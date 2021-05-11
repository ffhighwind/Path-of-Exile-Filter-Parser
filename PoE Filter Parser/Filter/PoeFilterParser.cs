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
using System.IO;
using System.Linq;
using System.Text;

namespace PathOfExile.Filter
{
	/// <summary>
	/// </summary>
	/// <see cref="https://www.pathofexile.com/item-filter/about"/>
	public class PoeFilterParser
	{
		public PoeFilterParser()
		{
			Token = Lexer.Token;
		}

		public int LineNumber => Lexer.LineNumber;
		public string Line => Token.Line;
		private readonly PoeFilterLexer Lexer = new PoeFilterLexer();
		private FilterToken Token;

		private static string[] Colors { get; } = new string[] { "Red", "Green", "Blue", "Brown", "White", "Yellow", "Cyan", "Grey", "Orange", "Pink", "Purple" };
		private static string[] Shapes { get; } = new string[] { "Circle", "Diamond", "Hexagon", "Square", "Star", "Triangle", "Cross", "Moon", "Raindrop", "Kite", "Pentagon", "UpsideDownHouse" };
		private static string[] Rarities { get; } = new string[] { "Normal", "Magic", "Rare", "Unique" };
		private static string[] GemQualityTypes { get; } = new string[] { "Superior", "Divergent", "Anomalous", "Phantasmal" };
		private static char[] SocketColors { get; } = new char[] { 'R', 'G', 'B', 'W', 'A' };

		public TextReader TextReader
		{
			get => Lexer.Reader;
			set => Lexer.Reader = value;
		}

		/// <summary>
		/// Context sensitive parsing.
		/// </summary>
		public PoeFilterFile Parse(TextReader reader)
		{
			Lexer.Reader = reader;
			List<IFilterRule> rules = new List<IFilterRule>(200);
			for (; ; ) {
				IFilterRule rule = Next();
				if (rule == null)
					break;
				rules.Add(rule);
			}
			return new PoeFilterFile(rules);
		}

		/// <summary>
		/// Context insensitive parsing.
		/// </summary>
		public IFilterRule Next()
		{
			IFilterRule r = null;
			bool isComment = false;
			Lexer.NextLine();
			for (; ; ) {
				NextToken();
				switch (Token.TokenType) {
					case TokenType.Pound:
						if (isComment) {
							r = new WhiteSpace(Line); // two # in a row
							return r;
						}
						isComment = true;
						continue;
					case TokenType.EOF:
						return null;
					case TokenType.EOL:
						r = new WhiteSpace(Line);
						return r;
					case TokenType.WhiteSpace:
						continue; // skip initial whitespace
					case TokenType.Error:
					case TokenType.Number:
					case TokenType.QuotedString:
					case TokenType.Exclamation:
					case TokenType.Equal:
					case TokenType.EqualEqual:
					case TokenType.GreaterThan:
					case TokenType.GreaterThanOrEqual:
					case TokenType.LessThan:
					case TokenType.LessThanOrEqual:
					case TokenType.Dash:
						break;
					case TokenType.String:
						switch (Token.Line[Token.Start]) {
							//case '#':
							case 'A':
								//AlternateQuality
								//AnyEnchantment
								//AreaLevel
								r = MatchAlternateQuality() ?? MatchAnyEnchantment() ?? MatchAreaLevel();
								break;
							case 'B':
								//BaseType
								//BlightedMap
								r = MatchBaseType() ?? MatchBlightedMap();
								break;
							case 'C':
								//Class
								//Continue
								//Corrupted
								//CorruptedMods
								//CustomAlertSound
								r = MatchClass() ?? MatchContinue() ?? MatchCorruptedMods() ?? MatchCorrupted() ?? MatchCustomAlertSound();
								break;
							case 'D':
								//DisableDropSound
								//DropLevel
								r = MatchDropLevel() ?? MatchDisableDropSound();
								break;
							case 'E':
								//ElderItem
								//ElderMap
								//EnableDropSound
								//EnchantmentPassiveNode
								//MatchEnchantmentPassiveNum
								r = MatchEnableDropSound() ?? MatchElderItem() ?? MatchEnchantmentPassiveNode() ?? MatchEnchantmentPassiveNum() ?? MatchElderMap();
								break;
							case 'F':
								//FracturedItem
								r = MatchFracturedItem();
								break;
							case 'G':
								//GemLevel
								//GemQualityType
								r = MatchGemLevel() ?? MatchGemQualityType();
								break;
							case 'H':
								//HasEnchantment
								//HasExplicitMod
								//HasInfluence
								//Height
								r = MatchHide() ?? MatchHasInfluence() ?? MatchHeight() ?? MatchHasExplicitMod() ?? MatchHasEnchantment();
								break;
							case 'I':
								//Identified
								//ItemLevel
								r = MatchItemLevel() ?? MatchIdentified();
								break;
							case 'L':
								//LinkedSockets
								r = MatchLinkedSockets();
								break;
							case 'M':
								//MapTier
								//MinimapIcon
								//Mirrored
								r = MatchMinimapIcon() ?? MatchMapTier() ?? MatchMirrored();
								break;
							case 'P':
								//PlayAlertSound
								//PlayAlertSoundPositional
								//PlayEffect
								//Prophecy
								r = MatchPlayAlertSound() ?? MatchPlayEffect() ?? MatchProphecy() ?? MatchPlayAlertSoundPositional();
								break;
							case 'Q':
								//Quality
								r = MatchQuality();
								break;
							case 'R':
								//Rarity
								//Replica
								r = MatchRarity() ?? MatchReplica();
								break;
							case 'S':
								//SetBackgroundColor
								//SetBorderColor
								//SetFontSize
								//SetTextColor
								//ShapedMap
								//ShaperItem
								//Show
								//SocketGroup
								//Sockets
								//StackSize
								//SynthesisedItem
								r = MatchShow() ?? MatchSetBackgroundColor() ?? MatchSetBorderColor() ?? MatchSetFontSize() ?? MatchSetTextColor()
									?? MatchSockets() ?? MatchSocketGroup() ?? MatchShaperItem() ?? MatchShapedMap() ?? MatchStackSize() ?? MatchSynthesisedItem();
								break;
							case 'W':
								//Width
								r = MatchWidth();
								break;
							default:
								break;
						}
						break; // handled later
					default:
						throw new NotImplementedException(Token.TokenType.ToString());
				}
				if (r == null) {
					if (isComment)
						return new WhiteSpace(Line);
					else
						r = CreateParseError();
				}
				else if (isComment) {
					if (r.Type == FilterType.ParseError)
						return new WhiteSpace(Line);
					else {
						r = new DisabledBlock(r);
					}
				}
				int commentIndex = Line.IndexOf('#', Token.Start);
				if (commentIndex > 0) {
					r.Comment = Line.Substring(commentIndex + 1).Trim();
				}
				return r;
			}
		}

		#region Utilities
		private ParseError CreateParseError(FilterType filterType) => CreateParseError(null, filterType);

		private ParseError CreateParseError(string message = null, FilterType filterType = FilterType.ParseError)
		{
			return new ParseError(LineNumber, Token.Start, Token.Line, message, filterType);
		}

		private bool Accept(string str)
		{
			if (Token == str) {
				NextToken();
				WS();
				return true;
			}
			return false;
		}

		private bool Accept(char c)
		{
			if (Token.Length == 1 && Token.Text[0] == c) {
				NextToken();
				WS();
				return true;
			}
			return false;
		}

		private bool Accept(TokenType type)
		{
			if (Token.TokenType == type) {
				NextToken();
				WS();
				return true;
			}
			return false;
		}

		private bool Accept()
		{
			NextToken();
			WS();
			return true;
		}

		private void WS()
		{
			if (Token.TokenType == TokenType.WhiteSpace)
				Token = Lexer.Next();
		}

		private void NextToken()
		{
			Token = Lexer.Next();
		}

		private bool IsEndOfRule()
		{
			WS();
			return Token.TokenType == TokenType.EOL || Token.TokenType == TokenType.Pound || Token.TokenType == TokenType.EOF;
		}
		#endregion Utilities

		#region Blocks
		private IFilterRule MatchShow()
		{
			if (!Accept("Show"))
				return null;
			return new Show();
		}

		private IFilterRule MatchHide()
		{
			if (!Accept("Hide"))
				return null;
			return new Hide();
		}
		#endregion Blocks

		#region Criteria

		#region Booleans
		private IFilterRule MatchAlternateQuality()
		{
			if (!Accept("AlternateQuality"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new AlternateQuality(b);
			}
			return CreateParseError(FilterType.AlternateQuality);
		}

		private IFilterRule MatchAnyEnchantment()
		{
			if (!Accept("AnyEnchantment"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new AnyEnchantment(b);
			}
			return CreateParseError(FilterType.AnyEnchantment);
		}

		private IFilterRule MatchBlightedMap()
		{
			if (!Accept("BlightedMap"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new BlightedMap(b);
			}
			return CreateParseError(FilterType.BlightedMap);
		}

		private IFilterRule MatchCorrupted()
		{
			if (!Accept("Corrupted"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new Corrupted(b);
			}
			return CreateParseError(FilterType.Corrupted);
		}

		private IFilterRule MatchElderItem()
		{
			if (!Accept("ElderItem"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new ElderItem(b);
			}
			return CreateParseError(FilterType.ElderItem);
		}

		private IFilterRule MatchElderMap()
		{
			if (!Accept("ElderMap"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new ElderMap(b);
			}
			return CreateParseError(FilterType.ElderMap);
		}

		private IFilterRule MatchIdentified()
		{
			if (!Accept("Identified"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new Identified(b);
			}
			return CreateParseError(FilterType.Identified);
		}

		private IFilterRule MatchMirrored()
		{
			if (!Accept("Mirrored"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new Mirrored(b);
			}
			return CreateParseError(FilterType.Mirrored);
		}

		private IFilterRule MatchFracturedItem()
		{
			if (!Accept("FracturedItem"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new FracturedItem(b);
			}
			return CreateParseError(FilterType.FracturedItem);
		}

		private IFilterRule MatchReplica()
		{
			if (!Accept("Replica"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new Replica(b);
			}
			return CreateParseError(FilterType.Replica);
		}

		private IFilterRule MatchShapedMap()
		{
			if (!Accept("ShapedMap"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new ShapedMap(b);
			}
			return CreateParseError(FilterType.ShapedMap);
		}

		private IFilterRule MatchShaperItem()
		{
			if (!Accept("ShaperItem"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new ShaperItem(b);
			}
			return CreateParseError(FilterType.ShaperItem);
		}

		private IFilterRule MatchSynthesisedItem()
		{
			if (!Accept("SynthesisedItem"))
				return null;
			string str = Token.Text;
			bool b = str.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (b || str.Equals("false", StringComparison.OrdinalIgnoreCase)) {
				return new SynthesisedItem(b);
			}
			return CreateParseError(FilterType.SynthesisedItem);
		}
		#endregion Booleans

		#region Comparison Int
		private bool GetComparison(out ComparisonType comparison)
		{
			switch (Token.TokenType) {
				case TokenType.Exclamation:
					comparison = ComparisonType.NotEquals;
					break;
				case TokenType.Equal:
					comparison = ComparisonType.Equals;
					break;
				case TokenType.EqualEqual:
					comparison = ComparisonType.ExactlyEquals;
					break;
				case TokenType.GreaterThan:
					comparison = ComparisonType.GreaterThan;
					break;
				case TokenType.GreaterThanOrEqual:
					comparison = ComparisonType.GreaterThanOrEquals;
					break;
				case TokenType.LessThan:
					comparison = ComparisonType.LessThan;
					break;
				case TokenType.LessThanOrEqual:
					comparison = ComparisonType.LessThanOrEquals;
					break;
				default:
					comparison = ComparisonType.Equals;
					return true;
			}
			NextToken();
			if (Token.TokenType == TokenType.WhiteSpace) {
				NextToken();
				return true;
			}
			return false;
		}

		private bool GetComparison(out ComparisonType comparison, out int i)
		{
			if (GetComparison(out comparison) && Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out i))
				return true;
			i = 0;
			return false;
		}

		private List<string> GetStrings(out ComparisonType comparison)
		{
			switch (Token.TokenType) {
				case TokenType.Exclamation:
				case TokenType.GreaterThan:
				case TokenType.GreaterThanOrEqual:
				case TokenType.LessThan:
				case TokenType.LessThanOrEqual:
					comparison = ComparisonType.Equals;
					return null;
				case TokenType.Equal:
					comparison = ComparisonType.Equals;
					NextToken();
					if (Token.TokenType != TokenType.WhiteSpace)
						return null;
					NextToken();
					break;
				case TokenType.EqualEqual:
					comparison = ComparisonType.ExactlyEquals;
					NextToken();
					if (Token.TokenType != TokenType.WhiteSpace)
						return null;
					NextToken();
					break;
				default:
					comparison = ComparisonType.Equals;
					break;
			}
			return GetStrings();
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0-100</para>
		/// </summary>
		private IFilterRule MatchAreaLevel()
		{
			if (!Accept("AreaLevel"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new AreaLevel(comparison, i);
			}
			return CreateParseError(FilterType.AreaLevel);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0-100</para>
		/// </summary>
		private IFilterRule MatchCorruptedMods()
		{
			if (!Accept("CorruptedMods"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new CorruptedMods(comparison, i);
			}
			return CreateParseError(FilterType.CorruptedMods);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0-100</para>
		/// </summary>
		private IFilterRule MatchDropLevel()
		{
			if (!Accept("DropLevel"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new DropLevel(comparison, i);
			}
			return CreateParseError(FilterType.DropLevel);
		}

		/// <summary>
		/// Only the "Adds X passive skills" modifiers.
		/// [Operator] Value
		/// <para><b>Value:</b> 0-7</para>
		/// </summary>
		private IFilterRule MatchEnchantmentPassiveNum()
		{
			if (!Accept("EnchantmentPassiveNum"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new EnchantmentPassiveNum(comparison, i);
			}
			return CreateParseError(FilterType.EnchantmentPassiveNum);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0+</para>
		/// </summary>
		private IFilterRule MatchGemLevel()
		{
			if (!Accept("GemLevel"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new GemLevel(comparison, i);
			}
			return CreateParseError(FilterType.GemLevel);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 1-4</para>
		/// </summary>
		private IFilterRule MatchHeight()
		{
			if (!Accept("Height"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new Height(comparison, i);
			}
			return CreateParseError(FilterType.Height);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0-100</para>
		/// </summary>
		private IFilterRule MatchItemLevel()
		{
			if (!Accept("ItemLevel"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new ItemLevel(comparison, i);
			}
			return CreateParseError(FilterType.ItemLevel);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0-6</para>
		/// </summary>
		private IFilterRule MatchLinkedSockets()
		{
			if (!Accept("LinkedSockets"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new LinkedSockets(comparison, i);
			}
			return CreateParseError(FilterType.LinkedSockets);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 1-17</para>
		/// </summary>
		private IFilterRule MatchMapTier()
		{
			if (!Accept("MapTier"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new MapTier(comparison, i);
			}
			return CreateParseError(FilterType.MapTier);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 0-30</para>
		/// </summary>
		private IFilterRule MatchQuality()
		{
			if (!Accept("Quality"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new Quality(comparison, i);
			}
			return CreateParseError(FilterType.Quality);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 1+</para>
		/// </summary>
		private IFilterRule MatchStackSize()
		{
			if (!Accept("StackSize"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new StackSize(comparison, i);
			}
			return CreateParseError(FilterType.StackSize);
		}

		/// <summary>
		/// [Operator] Value
		/// <para><b>Value:</b> 1-2</para>
		/// </summary>
		private IFilterRule MatchWidth()
		{
			if (!Accept("Width"))
				return null;
			if (GetComparison(out ComparisonType comparison, out int i) && i >= 0) {
				//Accept();
				return new Width(comparison, i);
			}
			return CreateParseError(FilterType.Width);
		}
		#endregion Comparison Int

		#region Strings
		private List<string> GetStrings()
		{
			List<string> list = new List<string>();
		loopStart:
			switch (Token.TokenType) {
				case TokenType.WhiteSpace:
					NextToken();
					goto loopStart;
				case TokenType.String:
					list.Add(Token.Text);
					Accept();
					goto loopStart;
				case TokenType.QuotedString:
					if (Token.Length == 0)
						goto failure;
					list.Add(Token.Text);
					Accept();
					goto loopStart;
				case TokenType.Pound:
				case TokenType.EOF:
				case TokenType.EOL:
					break;
				default:
					goto failure;
			}
			if (list.Count != 0)
				return list;
			failure:
			return null;
		}

		private IFilterRule MatchBaseType()
		{
			if (!Accept("BaseType"))
				return null;
			List<string> list = GetStrings(out ComparisonType comparison);
			if (list != null) {
				return new BaseType(comparison, list);
			}
			return CreateParseError(FilterType.BaseType);
		}

		private IFilterRule MatchClass()
		{
			if (!Accept("Class"))
				return null;
			List<string> list = GetStrings(out ComparisonType comparison);
			if (list != null) {
				return new Class(comparison, list);
			}
			return CreateParseError(FilterType.Class);
		}

		private IFilterRule MatchEnchantmentPassiveNode()
		{
			if (!Accept("EnchantmentPassiveNode"))
				return null;
			List<string> list = GetStrings(out ComparisonType comparison);
			if (list != null) {
				return new EnchantmentPassiveNode(comparison, list);
			}
			return CreateParseError(FilterType.EnchantmentPassiveNode);
		}

		private IFilterRule MatchHasEnchantment()
		{
			if (!Accept("HasEnchantment"))
				return null;
			List<string> list = GetStrings(out ComparisonType comparison);
			if (list != null) {
				return new HasEnchantment(comparison, list);
			}
			return CreateParseError(FilterType.HasEnchantment);
		}

		private IFilterRule MatchHasExplicitMod()
		{
			if (!Accept("HasExplicitMod"))
				return null;
			_ = GetComparison(out ComparisonType comparison); // ignore the whitespace piece
			int count = 0;
			List<string> list = null;
			if (Token.TokenType == TokenType.Number) {
				if (int.TryParse(Token.Text, out int i) && i >= 0) {
					Accept();
					list = GetStrings();
				}
			}
			else
				list = GetStrings();
			if (list != null) {
				return new HasExplicitMod(comparison, count, list);
			}
			return CreateParseError(FilterType.HasExplicitMod);
		}

		private IFilterRule MatchHasInfluence()
		{
			if (!Accept("HasInfluence"))
				return null;
			List<string> list = GetStrings(out ComparisonType comparison);
			if (list != null) {
				return new HasInfluence(comparison, list);
			}
			return CreateParseError(FilterType.HasInfluence);
		}

		private IFilterRule MatchProphecy()
		{
			if (!Accept("Prophecy"))
				return null;
			List<string> list = GetStrings(out ComparisonType comparison);
			if (list != null) {
				return new Prophecy(comparison, list);
			}
			return CreateParseError(FilterType.Prophecy);
		}
		#endregion Strings

		#region Sockets
		private bool GetSockets(out ComparisonType comparison, out int count, out string colors)
		{
			colors = null;
			count = 0;
			if (!GetComparison(out comparison))
				return false;
			string line = Line;
			int start;
			int end;
			switch (Token.TokenType) {
				case TokenType.Number:
				case TokenType.String:
					start = Token.Start;
					end = Token.End;
					break;
				case TokenType.QuotedString:
					start = Token.Start + 1;
					end = Token.End - 1;
					break;
				default:
					return false;
			}
			StringBuilder sb = new StringBuilder();
			for (int i = start; i < end; i++) {
				char ch = line[i];
				if (char.IsDigit(line[i])) {
					count = count * 10 + (ch - '0');
					continue;
				}
				for (; i < end; i++) {
					if (!SocketColors.Contains(ch))
						return false;
					ch = line[i];
				}
			}
			Accept();
			for (; ; ) {
				if (Token.TokenType == TokenType.QuotedString) {
					start = Token.Start + 1;
					end = Token.End - 1;
				}
				else if (Token.TokenType == TokenType.String) {
					start = Token.Start;
					end = Token.End;
				}
				else
					break;
				for (int i = start; i < end; i++) {
					char ch = line[i];
					if (!SocketColors.Contains(ch))
						return false;
				}
			}
			if (!IsEndOfRule())
				return false;
			if (sb.Length > 0) {
				char[] chars = new char[sb.Length];
				sb.CopyTo(0, chars, 0, sb.Length);
				Array.Sort(chars);
				colors = new string(chars);
			}
			return true;
		}

		/// <summary>
		/// [Operator] GroupSyntax
		/// <para><b>GroupSyntax:</b> Linked Sockets (0-6) + Socket Colors (RGBADW) (e.g. 5RGGB)</para>
		/// </summary>
		private IFilterRule MatchSocketGroup()
		{
			if (!Accept("SocketGroup"))
				return null;
			if (GetSockets(out ComparisonType comparison, out int count, out string colors)) {
				return new SocketGroup(comparison, count, colors);
			}
			return CreateParseError(FilterType.SocketGroup);
		}

		/// <summary>
		/// [Operator] GroupSyntax
		/// <para><b>GroupSyntax:</b> Sockets (0-6) + Socket Colors (RGBADW) (e.g. 5RGGB)</para>
		/// </summary>
		private IFilterRule MatchSockets()
		{
			if (!Accept("Sockets"))
				return null;
			if (GetSockets(out ComparisonType comparison, out int count, out string colors)) {
				return new Sockets(comparison, count, colors);
			}
			return CreateParseError(FilterType.Sockets);
		}
		#endregion Sockets

		/// <summary>
		/// [Operator] QualityType+
		/// <para><b>QualityType:</b> Superior Divergent Anomalous Phantasmal</para>
		/// </summary>
		private IFilterRule MatchGemQualityType()
		{
			if (!Accept("GemQualityType"))
				return null;
			if (Token.TokenType == TokenType.String) {
				string value = Token.Text;
				if (GemQualityTypes.Contains(value)) {
					return new GemQualityType(value);
				}
			}
			return CreateParseError(FilterType.GemQualityType);
		}

		/// <summary>
		/// [Operator] Rarity
		/// <para><b>Rarity:</b> Normal Magic Rare Unique</para>
		/// </summary>
		private IFilterRule MatchRarity()
		{
			if (!Accept("Rarity"))
				return null;
			if (GetComparison(out ComparisonType comparison) && Token.TokenType == TokenType.String) {
				string value = Token.Text;
				if (Rarities.Contains(value)) {
					return new Rarity(comparison, value);
				}
			}
			return CreateParseError(FilterType.Rarity);
		}
		#endregion Criteria

		#region Actions
		#region RGBA
		private bool GetColors(out byte red, out byte green, out byte blue, out byte? alpha)
		{
			alpha = null;
			red = 0;
			green = 0;
			blue = 0;
			if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int r) && r >= 0 && r <= 255) {
				red = (byte) r;
				Accept();
				if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int g) && g >= 0 && g <= 255) {
					green = (byte) g;
					Accept();
					if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int b) && b >= 0 && b <= 255) {
						blue = (byte) b;
						Accept();
						if (Token.TokenType == TokenType.Number) {
							if (int.TryParse(Token.Text, out int a) && a >= 0 && a <= 255) {
								alpha = (byte) a;
								return true;
							}
						}
						else if (IsEndOfRule()) {
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Red Green Blue [Alpha]
		/// <para><b>RGBA:</b>0-255</para>
		/// </summary>
		private IFilterRule MatchSetBackgroundColor()
		{
			if (!Accept("SetBackgroundColor"))
				return null;
			if (GetColors(out byte r, out byte g, out byte b, out byte? a)) {
				return new SetBackgroundColor(r, g, b, a ?? SetBackgroundColor.DefaultAlpha);
			}
			return CreateParseError(FilterType.SetBackgroundColor);
		}

		/// <summary>
		/// Red Green Blue [Alpha]
		/// <para><b>RGBA:</b>0-255</para>
		/// </summary>
		private IFilterRule MatchSetBorderColor()
		{
			if (!Accept("SetBorderColor"))
				return null;
			if (GetColors(out byte r, out byte g, out byte b, out byte? a)) {
				return new SetBorderColor(r, g, b, a ?? SetBorderColor.DefaultAlpha);
			}
			return CreateParseError(FilterType.SetBorderColor);
		}

		/// <summary>
		/// Red Green Blue [Alpha]
		/// <para><b>RGBA:</b>0-255</para>
		/// </summary>
		private IFilterRule MatchSetTextColor()
		{
			if (!Accept("SetTextColor"))
				return null;
			if (GetColors(out byte r, out byte g, out byte b, out byte? a)) {
				return new SetTextColor(r, g, b, a ?? SetTextColor.DefaultAlpha);
			}
			return CreateParseError(FilterType.SetTextColor);
		}
		#endregion RGBA

		private IFilterRule MatchContinue()
		{
			if (!Accept("Continue"))
				return null;
			return new Continue();
		}

		/// <summary>
		/// ("Filename" | "None") [Volume]
		/// <para><b>Volume:</b> 0-300 (default 100)</para>
		/// </summary>
		private IFilterRule MatchCustomAlertSound()
		{
			if (!Accept("CustomAlertSound"))
				return null;
			if (Token.TokenType == TokenType.QuotedString) {
				string path = Token.Text;
				if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int volume) && volume >= 0 && volume <= 300) {
					return new CustomAlertSound(path, volume);
				}
				else if (IsEndOfRule()) {
					return new CustomAlertSound(path);
				}
			}
			return CreateParseError(FilterType.CustomAlertSound);
		}

		private IFilterRule MatchDisableDropSound()
		{
			if (!Accept("DisableDropSound"))
				return null;
			return new DisableDropSound();
		}

		private IFilterRule MatchEnableDropSound()
		{
			if (!Accept("EnableDropSound"))
				return null;
			return new EnableDropSound();
		}

		/// <summary>
		/// Size Color Shape
		/// <para><b>Size:</b> -1 (disable), 0 (largest), 1, 2</para>
		/// <para><b>Color:</b> Red Green Blue Brown White Yellow Cyan Grey Orange Pink Purple</para>
		/// <para><b>Shape:</b> Circle Diamond Hexagon Square Star Triangle Cross Moon Raindrop Kite Pentagon UpsideDownHouse</para>
		/// </summary>
		private IFilterRule MatchMinimapIcon()
		{
			if (!Accept("MinimapIcon"))
				return null;
			bool negative = false;
			if (Token.TokenType == TokenType.Dash) {
				negative = true;
				NextToken();
			}
			if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int size) && size >= -1 && size <= 2) {
				Accept();
				if (negative)
					size = -size;
				if (Token.TokenType == TokenType.String) {
					string color = Token.Text;
					if (Colors.Contains(color) && Token.TokenType == TokenType.String) {
						Accept();
						string shape = Token.Text;
						if (Shapes.Contains(shape)) {
							return new MinimapIcon(size, color, shape);
						}
					}
				}
				else if (size == -1 && IsEndOfRule()) {
					return new MinimapIcon(size, null, null);
				}
			}
			return CreateParseError(FilterType.MinimapIcon);
		}

		/// <summary>
		/// Id [Volume]
		/// <para><b>Id:</b> None, 1-16</para>
		/// <para><b>Volume:</b> 0-300</para>
		/// </summary>
		private IFilterRule MatchPlayAlertSound()
		{
			if (!Accept("PlayAlertSound"))
				return null;
			if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int soundId) && soundId >= 1 && soundId <= 16) {
				if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int volume) && volume >= 0 && volume <= 300) {
					return new PlayAlertSound(soundId, volume);
				}
				else if (IsEndOfRule()) {
					return new PlayAlertSound(soundId, 50);
				}
			}
			else if (Token.TokenType == TokenType.String && Token == "None") {
				return new PlayAlertSound(0, 0);
			}
			return CreateParseError(FilterType.PlayAlertSound);
		}

		/// <summary>
		/// Id [Volume]
		/// <para><b>Id:</b> None, 1-16</para>
		/// <para><b>Volume:</b> 0-300</para>
		/// </summary>
		private IFilterRule MatchPlayAlertSoundPositional()
		{
			if (!Accept("PlayAlertSoundPositional"))
				return null;
			if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int soundId) && soundId >= 1 && soundId <= 16) {
				NextToken();
				if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int volume) && volume >= 0 && volume <= 300) {
					return new PlayAlertSoundPositional(soundId, volume);
				}
				else if (IsEndOfRule()) {
					return new PlayAlertSoundPositional(soundId, 50);
				}
			}
			else if (Token.TokenType == TokenType.String && Token == "None") {
				return new PlayAlertSound(0, 0);
			}
			return CreateParseError(FilterType.PlayAlertSoundPositional);
		}

		/// <summary>
		/// Color [Temp]
		/// <para><b>Color:</b> Red Green Blue Brown White Yellow Cyan Grey Orange Pink Purple</para>
		/// </summary>
		private IFilterRule MatchPlayEffect()
		{
			if (!Accept("PlayEffect"))
				return null;
			if (Token.TokenType == TokenType.String) {
				string text = Token.Text;
				if (Token == "None") {
					return new PlayEffect(text, false);
				}
				else if (Colors.Contains(text)) {
					Accept();
					bool temp = Token.TokenType == TokenType.String && Token == "Temp";
					return new PlayEffect(text, temp);
				}
			}
			return CreateParseError(FilterType.PlayEffect);
		}

		/// <summary>
		/// FontSize
		/// <para><b>FontSize:</b>18-45 (32 default, any number is accepted)</para>
		/// </summary>
		private IFilterRule MatchSetFontSize()
		{
			if (!Accept("SetFontSize"))
				return null;
			if (Token.TokenType == TokenType.Number && int.TryParse(Token.Text, out int size) && size >= 0) {
				return new SetFontSize(size);
			}
			return CreateParseError(FilterType.SetFontSize);
		}
		#endregion Actions
	}
}
