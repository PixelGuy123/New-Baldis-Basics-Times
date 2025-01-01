using BBTimes.CustomComponents;
using BBTimes.Manager;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;

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
		private static void EnableMe(Window __instance) => __instance.gameObject.SetActive(true); // Make sure to enable it

		[HarmonyPatch("Initialize")]
		[HarmonyPostfix]
		private static void NaturalSpawnWindows(Window __instance, ref WindowObject ___windowObject)
		{
			var lg = LevelGeneratorInstanceGrabber.i;
			var wComp = __instance.GetComponent<CustomWindowComponent>();
			if (lg == null || wComp != null)
				return;

			var dataLvl = Singleton<CoreGameManager>.Instance.sceneObject.levelObject;

			if (dataLvl == null || dataLvl is not CustomLevelObject) return;




			var listObj = ((CustomLevelObject)dataLvl).GetCustomModValue(BBTimesManager.plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn");

			if (listObj == null)
				return;

			var objs = listObj as List<WindowObjectHolder>;

			objs.RemoveAll(x => !x.SelectionLimiters.Contains(__instance.aTile.room.category) && !x.SelectionLimiters.Contains(__instance.bTile.room.category));

			if (objs.Count == 0 || lg.controlledRNG.NextDouble() >= 0.45d) return;

			___windowObject = WeightedSelection<WindowObject>.ControlledRandomSelectionList(objs.ConvertAll(x => x.Selection), lg.controlledRNG);

			if (wComp == null)
			{
				wComp = __instance.gameObject.AddComponent<CustomWindowComponent>();
				var compI = ___windowObject.windowPre.GetComponent<CustomWindowComponent>();
				wComp.unbreakable = compI.unbreakable;
			}


			__instance.UpdateTextures();
		}

		[HarmonyPatch("OnDestroy")]
		[HarmonyPatch("Start")]
		[HarmonyFinalizer]
		private static Exception ShutUp() => null;

		public static SoundObject windowHitAudio;
	}
}
