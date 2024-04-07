using BBTimes.CustomComponents;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using System;
using System.Linq;
using System.Runtime.Remoting;

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
		private static void NaturalSpawnWindows(Window __instance,ref WindowObject ___windowObject)
		{
			var wComp = __instance.GetComponent<CustomWindowComponent>();
			if (PostRoomCreation.i == null || wComp != null)
				return;

			var data = BBTimesManager.CurrentFloorData;

			if (data == null) return;


			

			var objs = data.WindowObjects;
			objs.RemoveAll(x => !x.SelectionLimiters.Contains(__instance.aTile.room.category) && !x.SelectionLimiters.Contains(__instance.bTile.room.category));
			
			if (objs.Count == 0 || PostRoomCreation.i.controlledRNG.NextDouble() >= 0.45d) return;

			___windowObject = WeightedSelection<WindowObject>.ControlledRandomSelectionList(objs.ConvertAll(x => x.Selection), PostRoomCreation.i.controlledRNG);

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
