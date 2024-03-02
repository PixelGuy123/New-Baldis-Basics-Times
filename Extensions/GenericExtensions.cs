using System;
using System.Collections.Generic;

namespace BBTimes.Extensions
{
	public static class GenericExtensions
	{
		public static void ReplaceAt<T>(this IList<T> list, int index, T replacement)
		{
			list.RemoveAt(index);
			list.Insert(index, replacement);
		}

		public static bool Replace<T>(this IList<T> list, Predicate<T> predicate, T replacement)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
				{
					ReplaceAt(list, i, replacement);
					return true;
				}
			}
			return false;
		}
	}
}
