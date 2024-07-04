using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(RandomEvent))]
	internal class RandomEventPatch
	{
		[HarmonyPatch("End")]
		[HarmonyPostfix]
		static void FixEndPhase(RandomEvent __instance, EnvironmentController ___ec)
		{
			if (__instance.GetType() == typeof(FloodEvent))
			{
				foreach (var r in ___ec.rooms)
				{
					foreach (var door in r.doors)
					{
						door.Open(true, false);
						door.OpenTimed(Random.Range(3f, 5f), false);
					}
				}
			}
		}
	}
}
