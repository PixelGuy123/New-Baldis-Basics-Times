using BBTimes.Misc.Modifiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.Extensions
{
    public static class BaseModifierExtensions
	{
		public static IEnumerator ReverseSlideFOVAnimation<T>(this IList<T> collection, T instance, float offset, float smoothness = 2f) where T : BaseModifier
		{
			if (smoothness <= 1f)
				yield break;

			instance.Mod += offset;
			collection.Add(instance);
			while (!instance.Mod.CompareFloats(1f))
			{
				instance.Mod += (1f - instance.Mod) / smoothness * Time.timeScale;
				yield return null;
			}

			collection.Remove(instance);

			yield break;
		}

		public static IEnumerator SlideFOVAnimation<T>(this IList<T> collection, T instance, float offset, float smoothness = 2f) where T : BaseModifier
		{
			if (smoothness <= 1f)
				yield break;

			float off = Mathf.Clamp(instance.Mod + offset, 0f, 125f);

			collection.Add(instance);
			while (!instance.Mod.CompareFloats(off))
			{
				instance.Mod += (off - instance.Mod) / smoothness * Time.timeScale;
				yield return null;
			}

			yield break;
		}
	}
}
