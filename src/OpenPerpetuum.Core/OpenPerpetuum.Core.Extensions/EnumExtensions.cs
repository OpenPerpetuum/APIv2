using System;

namespace OpenPerpetuum.Core.Extensions
{
	public static class EnumExtensions
	{
		public static bool IsNullableEnum(Type t)
		{
			Type u = Nullable.GetUnderlyingType(t);
			return (u != null) && u.IsEnum;
		}
	}
}
