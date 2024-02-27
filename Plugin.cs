using BBTimes.Helpers;
using BBTimes.NPCs;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;


namespace BBTimes.Plugin
{
    [BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAll();
        }
    }

	static class ModInfo
	{

		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

		public const string PLUGIN_NAME = "Baldi\'s Basics Times";

		public const string PLUGIN_VERSION = "0.0.1";
	}

	[HarmonyPatch(typeof(NameManager), "Awake")]
	internal static class AddOfficeChair
	{
		[HarmonyPrefix]
		static void AddIt()
		{
			var ld = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.name == "Main1");
			ld.potentialNPCs.Add(new() { selection = NPCCreator.CreateNPC<OfficeChair>("OfficeChair", true), weight = 1000 });
			ld.minExtraRooms = 1;
			ld.maxExtraRooms = 1;
		}
	}

	// Some cheats
#if CHEAT

	[HarmonyPatch(typeof(HappyBaldi), "SpawnWait", MethodType.Enumerator)]
	internal static class QuickCheatBox
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Zero(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, name:"9"))
			.Set(OpCodes.Ldc_I4_0, null)
			.InstructionEnumeration();
	}
	[HarmonyPatch(typeof(Baldi_Chase), "OnStateTriggerStay")]
	internal static class QuickBaldiNoDeath
	{
		[HarmonyPrefix]
		internal static bool NoDeath() => false;
	}
	[HarmonyPatch(typeof(BaseGameManager), "Initialize")]
	internal static class AlwaysFullMap
	{
		private static void Prefix(BaseGameManager __instance)
		{
			__instance.CompleteMapOnReady();
		}
	}
#endif

}
