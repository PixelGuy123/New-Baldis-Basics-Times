using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(Window))]
	internal class WindowPatch
	{
		[HarmonyPatch("Break")]
		[HarmonyPrefix]
		private static bool UnbreakableOrNot(Window __instance, AudioManager ___audMan) // Unbreakable window
		{
			var comp = __instance.GetComponent<CustomWindowComponent>();
			if (comp == null) return true;
			bool breakable = !comp.unbreakable;
			if (!breakable)
				___audMan.PlaySingle(windowHitAudio);
			return breakable;
		}

		[HarmonyPatch("Initialize")]
		[HarmonyPrefix]
		private static bool NaturalSpawnWindows(Window __instance, EnvironmentController ec, IntVector2 pos, Direction dir)
		{
			if (hasSpawnedWindow || ec.Active)
			{
				hasSpawnedWindow = false;
				return true;
			}

			var lg = LevelGeneratorInstanceGrabber.i;
			var wComp = __instance.GetComponent<CustomWindowComponent>();
			if (lg == null || wComp != null)
				return true;

			var dataLvl = Singleton<BaseGameManager>.Instance.levelObject;

			if (dataLvl is not CustomLevelGenerationParameters cld)
				return true;

			var listObj = cld.GetCustomModValue(BBTimesManager.plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn");

			if (listObj == null)
				return true;

			__instance.ec = ec;
			__instance.position = pos;
			__instance.direction = dir;

			var list = listObj as List<WindowObjectHolder>;

			_copiedWindows.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				if (
					list[i].SelectionLimiters.Contains(__instance.aTile.room.category) || // If the window has its correspondent category in front or back, it should be included
					list[i].SelectionLimiters.Contains(__instance.bTile.room.category)
					)
				{
					_copiedWindows.Add(list[i].Selection);
				}
			}

			if (_copiedWindows.Count == 0) return true;

			var selectedWindowObject = WeightedSelection<WindowObject>.ControlledRandomSelectionList(_copiedWindows, lg.controlledRNG);
			if (selectedWindowObject == null) // Null here means it is the default wood window
				return true;

			// ********* Replacement Phase Here ***********

			// Create already a new Window in the same place
			hasSpawnedWindow = true;
			__instance.ec.ForceBuildWindow(
				__instance.ec.CellFromPosition(__instance.position),
				__instance.direction,
				selectedWindowObject);

			// Destroy the Window
			__instance.StartCoroutine(OneFrameDestruction(__instance.gameObject));

			static IEnumerator OneFrameDestruction(GameObject obj)
			{
				yield return null;
				UnityEngine.Object.Destroy(obj);
			}

			return false;
		}

		[HarmonyPatch("OnDestroy")]
		[HarmonyPatch("Start")]
		[HarmonyFinalizer]
		private static Exception ShutUp() => null;

		public static SoundObject windowHitAudio;

		// Temporary lazy workaround (When making the custom windows mod, I'll implement a better way of adding custom windows to the map)
		static bool hasSpawnedWindow = false;
		readonly static List<WeightedSelection<WindowObject>> _copiedWindows = [];
	}
}
