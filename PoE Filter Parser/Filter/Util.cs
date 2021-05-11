using System;
using System.Collections.Generic;

namespace PathOfExile.Filter
{
	internal class Util
	{
		public static string ComparisonString(ComparisonType comparison)
		{
			switch (comparison) {
				case ComparisonType.None:
					return "";
				case ComparisonType.NotEquals:
					return "!";
				case ComparisonType.Equals:
					return "=";
				case ComparisonType.ExactlyEquals:
					return "==";
				case ComparisonType.LessThan:
					return "<";
				case ComparisonType.LessThanOrEquals:
					return "<=";
				case ComparisonType.GreaterThan:
					return ">";
				case ComparisonType.GreaterThanOrEquals:
					return ">=";
			}
			throw new NotImplementedException(comparison.ToString());
		}
	}
}
