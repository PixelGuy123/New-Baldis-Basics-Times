using BBTimes.Helpers;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using MTM101BaldAPI.Registers;
using BBTimes.Manager;
using System.Linq;
using UnityEngine;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;


namespace BBTimes.Plugin
{
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)] // let's not forget this
    [BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
    public class BasePlugin : BaseUnityPlugin
    {
		private void Awake()
		{
			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAll();

			MTM101BaldiDevAPI.SaveGamesHandler = SavedGameDataHandler.Modded;
			_modPath = AssetLoader.GetModPath(this);

			LoadingEvents.RegisterOnAssetsLoaded(() => BBTimesManager.InitializeContentCreation(this), false);

			GeneratorManagement.Register(this, GenerationModType.Addend, (floorName, floorNum, ld) =>
			{
				var floordata = BBTimesManager.floorDatas.FirstOrDefault(x => x.Floor == floorName);
				if (floordata == null)
				{
					Debug.LogWarning("Failed to get floor data for level: " + ld.name);
					return;
				}


				ld.potentialNPCs.AddRange(floordata.NPCs);
				ld.items = ld.items.AddRangeToArray([.. floordata.Items]);

			});
		}

		static string _modPath = string.Empty;

		public static string ModPath => _modPath;

	}

	static class ModInfo
	{

		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

		public const string PLUGIN_NAME = "Baldi\'s Basics Times";

		public const string PLUGIN_VERSION = "0.1.0";
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
