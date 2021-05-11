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
	public enum FilterType
	{
		#region Blocks	
		WhiteSpace = 1,
		Hide,
		Show,
		DisabledBlock,
		ParseError,
		#endregion Blocks

		#region Criteria
		/// <summary>
		/// If the item is a Mirrored item or not. Does not drop normally, except when opening a Strongbox with the "Contains Mirrored Items", or via the Prophecy Kalandra's Craft.
		/// </summary>
		Mirrored,
		/// <summary>
		/// If an item is an Replica or not. Note: This is applicable to Replica Unique introduced in 3.12.
		/// </summary>
		Replica,
		/// <summary>
		/// If an item is identified or not.
		/// </summary>
		Identified,
		/// <summary>
		/// Currency stack size
		/// </summary>
		StackSize,
		/// <summary>
		/// The number of slots the item takes on the X-axis (horizontal axis), i.e. the width of the item.
		/// </summary>
		Width,
		/// <summary>
		/// The number of slots the item takes on the Y-axis (verical axis), i.e. the height of the item.
		/// </summary>
		Height,
		/// <summary>
		/// Filters for items dropped in a particular Monster level of the current area. This is probably the most relevant of the filters, as it allows enabling/disabling filters dynamically depending on leveling.
		/// </summary>
		AreaLevel,
		/// <summary>
		/// The item level the item was generated at.
		/// </summary>
		ItemLevel,
		/// <summary>
		/// The level that the item starts dropping at.
		/// </summary>
		DropLevel,
		/// <summary>
		/// Gem Level
		/// </summary>
		GemLevel,
		/// <summary>
		/// The amount of quality on the item.
		/// </summary>
		Quality,		
		/// <summary>
		/// Rarity of the item.
		/// </summary>
		Rarity,
		/// <summary>
		/// The item class. Specifying part of a class name is allowed and will match any classes with that text in the name. So for example "One Hand" will match both "One Hand Sword" and "One Hand Axe"
		/// </summary>
		Class,
		/// <summary>
		/// The base type of the item. Specifying a part of a base type name is allowed and will match any of the base types with that text in the name.
		/// </summary>
		BaseType,
		/// <summary>
		/// The prophecy name. Specifying a part of a prophecy name is allowed and will match any of the prophecies with that text in the name. Prophecies have the Class type "Stackable Currency".
		/// </summary>
		Prophecy,
		/// <summary>
		/// Filter by mods on an item by name. For example: [HasExplicitMod "Tyrannical" ] (Tyrannical=Local Physical Damage 155 to 169%)
		/// </summary>
		HasExplicitMod,
		/// <summary>
		/// If an item has any enchantment from the Labyrinth.
		/// </summary>
		AnyEnchantment,
		/// <summary>
		/// Filter Cluster Jewels by enchantment type.
		/// </summary>
		EnchantmentPassiveNode,
		/// <summary>
		/// Filter Cluster Jewels by number of passive skills. This condition checks only the "Adds X passive skills" modifiers.
		/// </summary>
		EnchantmentPassiveNum,
		/// <summary>
		/// Filter by enchantments
		/// </summary>
		HasEnchantment,
		/// <summary>
		/// If an item is fractured or not
		/// </summary>
		FracturedItem,
		/// <summary>
		/// If an item is synthesised or not
		/// </summary>
		SynthesisedItem,
		/// <summary>
		/// If an item is an Elder item or not.
		/// </summary>
		ElderItem,
		/// <summary>
		/// If an item is a Shaper item or not.
		ShaperItem,
		/// <summary>
		/// If an item has Influence of a certain type. Note that this also affects Maps that are influenced.
		/// If want item that has no Influence, choose value as None.
		/// </summary>
		HasInfluence,
		/// <summary>
		/// Does the exact same thing as SocketGroup but does not require the sockets to be linked. So the same example ">= 5GGG" will match 5 or more sockets not necessarily linked, with at least 3 green sockets anywhere.
		/// Unlike SocketGroup, this condition does allow for mixing and using Delve and Abyss sockets, for example, a Resonator with 3 sockets would be "DDD".
		/// </summary>
		Sockets,
		/// <summary>
		/// The size of the largest group of linked sockets that the item has.
		/// </summary>
		LinkedSockets,
		/// <summary>
		/// Supports a list of groups that each one represents linked sockets containing a specific set of colors, at least one group must be matched for the condition to pass.
		/// Each group is composed by an optional number and a sequence of letters. The number specifies the longest link which contains the following color sequence described by the letters. 
		/// Each letter is short-hand for the colour ([R]ed, [G]reen, [B]lue, [W]hite) or Special ones ([D]elve Socket, [A]byss Socket). 
		/// For example, 5RRG will match any group that contains two red sockets linked with a green socket in a 5-link group. Delve and Abyss cannot be in the same group as any other, as they cannot be linked.
		/// If a comparison operator is used, it will apply to the numeric portion, so a ">= 5GGG" will match a 5 or more linked group with 3 green sockets.
		/// SocketGroup with A and D socket has no effect. For example "SocketGroup RGBA" or "SocketGroup DD". As Abyss and Delve sockets are never linked.
		/// </summary>
		SocketGroup,
		ElderMap,
		BlightedMap,
		///<summary>
		/// If the map is shaped or not.
		/// </summary>
		ShapedMap,
		///<summary>
		/// The map tier of the map.
		/// </summary>
		MapTier,
		/// <summary>
		/// If an item has alternate quality or not. Note: This is applicable to Alternate Gems introduced in 3.12.
		/// </summary>
		AlternateQuality,
		/// <summary>
		/// Gem Quality Type
		/// </summary>
		GemQualityType,
		/// <summary>
		/// If an item is corrupted or not.
		/// </summary>
		Corrupted,
		/// <summary>
		/// How many corrupted mods are present.
		/// </summary>
		CorruptedMods,
		#endregion Criteria

		#region Actions
		SetFontSize,
		SetTextColor,
		SetBorderColor,
		SetBackgroundColor,		
		PlayAlertSound,
		PlayAlertSoundPositional,
		CustomAlertSound,
		DisableDropSound,
		EnableDropSound,
		MinimapIcon,
		PlayEffect,
		Continue,
		#endregion Actions
	}
}
