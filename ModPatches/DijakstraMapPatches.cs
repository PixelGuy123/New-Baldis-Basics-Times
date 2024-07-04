using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch]
	internal class DijakstraMapPatches
	{
		[HarmonyPatch(typeof(DijkstraMap), "UpdateIsNeeded")]
		[HarmonyPatch(typeof(DijkstraMap), "Calculate", [])]
		[HarmonyPrefix]
		private static void CheckForNullTargets(ref List<Transform> ___targets)
		{
			for (int i = 0; i < ___targets.Count; i++)
			{
				if (___targets[i] == null)
					___targets.RemoveAt(i--);
			}
		}
	}
}
