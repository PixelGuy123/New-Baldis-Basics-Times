using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(BaseGameManager))]
	internal static class PostGenPatch
	{
		[HarmonyPatch("Initialize")]
		[HarmonyPostfix]
		private static void PostGen(BaseGameManager __instance)
		{
		}
	}

	[HarmonyPatch]
	static class AlwaysLog
	{
		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> GetMethods() 
		{
			foreach (var methods in AccessTools.GetDeclaredMethods(typeof(MTM101BaldiDevAPI).Assembly.GetTypes().First(x => x.FullName == "MTM101BaldAPI.Patches.LevelGeneratorPatches")))
				yield return methods;
		}

		[HarmonyFinalizer]
		static Exception GetException(Exception __exception)
		{
			if (__exception != null)
			{
				Debug.LogWarning("An exception from the level generator patch has been suppressed!");
				Debug.LogException(__exception);
			}

			return null;
		}
	}
}
