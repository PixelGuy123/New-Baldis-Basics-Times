using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.Extensions
{
	public static class ReflectionExtensions
	// yoinked this from my Infinite Floors mod
	{
		static bool IsInheritFromType<T, T2>() =>
			typeof(T).IsSubclassOf(typeof(T2)) || typeof(T) == typeof(T2);
		internal static T Mono_GetACopyFromFields<T, C>(this T original, C toCopyFrom) where T : MonoBehaviour where C : MonoBehaviour
		{
			if (!IsInheritFromType<T, C>())
			{
				throw new ArgumentException($"Type T ({typeof(T).FullName}) does not inherit from type C ({typeof(C).FullName})");
			}

			List<Type> typesToFollow = [typeof(C)];
			Type t = typeof(C);

			while (true)
			{
				t = t.BaseType;

				if (t == null || t == typeof(MonoBehaviour))
				{
					break;
				}

				typesToFollow.Add(t);
			}

			foreach (var ty in typesToFollow)
			{
				foreach (FieldInfo fieldInfo in AccessTools.GetDeclaredFields(ty))
				{
					fieldInfo.SetValue(original, fieldInfo.GetValue(toCopyFrom));
				}
			}

			return original;
		}
	}

}
