using System;
using System.Collections.Generic;
using UnityEngine;

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

		public static List<Transform> AllChilds(this Transform transform)
		{
			List<Transform> cs = [];
			int count = transform.childCount;
			for (int i = 0; i < count; i++)
				cs.Add(transform.GetChild(i));
			return cs;
		}
	}
}
