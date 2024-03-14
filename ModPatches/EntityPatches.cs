using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{
	/*
	 * These are necessary
	 * mod patches to make stuff like
	 * custom npcs properly function
	 * in game
	 * */
	[HarmonyPatch(typeof(Entity), "Initialize")]
	internal class EntityWakeUpPatch
	{
		private static void Prefix(Entity __instance, ref Transform ___transform)
		{
			___transform = __instance.transform;
			__instance.SetActive(true);
		}
	}
}
