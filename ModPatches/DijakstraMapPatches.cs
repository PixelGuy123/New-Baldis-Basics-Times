using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch]
	internal class DijakstraMapPatches
	{
		[HarmonyTargetMethods]
		private static MethodInfo[] GetMethods() => [AccessTools.Method(typeof(DijkstraMap), "UpdateIsNeeded"), AccessTools.Method(typeof(DijkstraMap), "Calculate", [])]; // Specifically point out the one without parameters
			
		[HarmonyPrefix]
		private static void CheckForNullTargets(ref List<Transform> ___targets)
		{
			for (int i = 0; i < ___targets.Count; i++)
				if (___targets[i] == null)
				{
					___targets.RemoveAt(i);
					i--;
				}
		}
	}
}
