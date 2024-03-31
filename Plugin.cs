using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using MTM101BaldAPI.Registers;
using BBTimes.Manager;
using System.Linq;
using UnityEngine;
using MTM101BaldAPI.AssetTools;
using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;


namespace BBTimes.Plugin
{
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)] // let's not forget this
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
    public class BasePlugin : BaseUnityPlugin
    {
		private void Awake()
		{
			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAll();
			
			_modPath = AssetLoader.GetModPath(this);

			LoadingEvents.RegisterOnAssetsLoaded(() => BBTimesManager.InitializeContentCreation(this), false);

			GeneratorManagement.Register(this, GenerationModType.Base, (floorName, floorNum, ld) =>
			{
#if CHEAT
				Debug.Log($"Level Object loaded as: {floorName} with num: {floorNum}. LevelObj name: {ld.name}");
#endif

				ld.MarkAsNeverUnload(); // Maybe?

				if (floorName == "F1")
				{
					ld.minSpecialRooms = 0; // Chance to have no special room
					ld.additionalNPCs += 2;
					ld.additionTurnChance += 10;
					ld.bridgeTurnChance += 4;
					ld.outerEdgeBuffer += 1;
					ld.extraDoorChance += 0.2f;
					ld.windowChance += 0.2f;
					ld.maxClassRooms = 5;
					ld.maxFacultyRooms += 2;
					ld.maxHallsToRemove += 1;
					ld.maxPlots += 1;
					ld.maxReplacementHalls += 1;
					ld.maxSize += new IntVector2(8, 5);
					ld.maxSpecialBuilders += 2;
					ld.minFacultyRooms += 1;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(3, 5);
					ld.classStickToHallChance = 0.9f;
					return;
				}

				if (floorName == "F2")
				{
					ld.deadEndBuffer = 4;
					ld.minSpecialRooms = 1; // Chance to have no special room
					ld.maxSpecialRooms = 2;
					ld.additionalNPCs += 4;
					ld.additionTurnChance += 5;
					ld.bridgeTurnChance += 3;
					ld.outerEdgeBuffer += 3;
					ld.extraDoorChance += 0.3f;
					ld.windowChance += 0.35f;
					ld.maxClassRooms = 8;
					ld.minClassRooms = 6;
					ld.maxFacultyRooms += 4;
					ld.maxHallsToRemove += 2;
					ld.maxPlots += 2;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 3;
					ld.maxSize += new IntVector2(9, 6);
					ld.maxSpecialBuilders += 2;
					ld.minFacultyRooms += 1;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(5, 6);
					ld.minSpecialBuilders += 1;
					ld.classStickToHallChance = 0.75f;
					ld.facultyStickToHallChance = 0.85f;
					ld.maxOffices = 2;
					ld.officeStickToHallChance = 0.9f;
					ld.specialRoomsStickToEdge = false;
					ld.maxLightDistance += 2;

					// TEMPORARY CHANGE TO REMOVE DR REFLEX AS FORCED NPC
					var drReflex = ld.forcedNpcs.First(x => x.GetType() == typeof(DrReflex));
					ld.forcedNpcs = [.. ld.forcedNpcs.Where(x => x.Character != Character.DrReflex)];
					ld.potentialNPCs.Add(new() { selection = drReflex, weight = 55});
					return;
				}

				if (floorName == "F3")
				{
					ld.deadEndBuffer = 2;
					ld.minSpecialRooms = 2; // Chance to have no special room
					ld.maxSpecialRooms = 3;
					ld.additionalNPCs += 4;
					ld.additionTurnChance += 15;
					ld.bridgeTurnChance += 6;
					ld.outerEdgeBuffer += 3;
					ld.extraDoorChance += 0.3f;
					ld.windowChance += 0.35f;
					ld.maxClassRooms = 12;
					ld.maxFacultyRooms += 4;
					ld.maxHallsToRemove += 2;
					ld.maxPlots += 2;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 3;
					ld.maxSize += new IntVector2(9, 6);
					ld.maxSpecialBuilders += 2;
					ld.minFacultyRooms += 1;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(5, 6);
					ld.minSpecialBuilders += 1;
					ld.classStickToHallChance = 0.75f;
					ld.facultyStickToHallChance = 0.85f;
					ld.maxOffices = 2;
					ld.officeStickToHallChance = 0.9f;
					ld.specialRoomsStickToEdge = false;
					ld.maxLightDistance += 5;
					ld.standardLightStrength -= 2;
					ld.potentialSpecialRooms = ld.potentialSpecialRooms.AddRangeToArray([.. Resources.FindObjectsOfTypeAll<RoomAsset>() // Playground in F3
						.Where(x => x.name.StartsWith("Playground"))
						.ConvertAll(x => new WeightedRoomAsset() { selection = x, weight = 45 })]);
					return;
				}

				if (floorName == "END")
				{
					ld.minSpecialRooms = 1; // Chance to have no special room
					ld.maxSpecialRooms = 2;
					ld.deadEndBuffer = 3;
					ld.additionalNPCs += 4;
					ld.additionTurnChance += 25;
					ld.bridgeTurnChance += 6;
					ld.outerEdgeBuffer += 3;
					ld.extraDoorChance += 0.3f;
					ld.windowChance += 0.35f;
					ld.maxClassRooms = 9;
					ld.minClassRooms = 7;
					ld.maxFacultyRooms += 5;
					ld.maxHallsToRemove += 2;
					ld.maxPlots += 3;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 3;
					ld.maxSize += new IntVector2(10, 8);
					ld.maxSpecialBuilders += 2;
					ld.minFacultyRooms += 1;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(7, 7);
					ld.minSpecialBuilders += 1;
					ld.classStickToHallChance = 0.75f;
					ld.facultyStickToHallChance = 0.85f;
					ld.maxOffices = 3;
					ld.minOffices = 2;
					ld.officeStickToHallChance = 0.9f;
					ld.maxLightDistance += 3;
					return;
				}
				
			});

			GeneratorManagement.Register(this, GenerationModType.Addend, (floorName, floorNum, ld) =>
			{
				var floordata = BBTimesManager.floorDatas.FirstOrDefault(x => x.Floor == floorName);
				if (floordata == null)
				{
					Debug.LogWarning("Failed to get floor data for level: " + ld.name);
					return;
				}

				foreach(var npc in floordata.NPCs)
				{
					if (npc.selection.GetComponent<CustomNPCData>().npcsBeingReplaced.Length == 0)
						ld.potentialNPCs.Add(npc); // Only non-replacement Npcs
					else
						ld.forcedNpcs = ld.forcedNpcs.AddToArray(npc.selection); // This field will be used for getting the replacement npcs, since they are outside the normal potential npcs, they can replace the existent ones at any time
				}
				
				ld.items = ld.items.AddRangeToArray([.. floordata.Items]);
				ld.randomEvents.AddRange(floordata.Events);
				ld.forcedSpecialHallBuilders = ld.forcedSpecialHallBuilders.AddRangeToArray([.. floordata.ForcedObjectBuilders]);
				ld.specialHallBuilders = ld.specialHallBuilders.AddRangeToArray([.. floordata.WeightedObjectBuilders]);
				ld.standardHallBuilders = ld.standardHallBuilders.AddRangeToArray([.. floordata.HallBuilders]);
				ld.shopItems = ld.shopItems.AddRangeToArray([.. floordata.ShopItems]);
				ld.fieldTripItems.AddRange(floordata.FieldTripItems);

				foreach (var holder in floordata.SchoolTextures) // Add the school textures
				{
					switch (holder.SelectionLimiters[0])
					{
						case RoomCategory.Hall:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
								ld.hallCeilingTexs = ld.hallCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
							if (holder.TextureType == Misc.SchoolTexture.Floor)
								ld.hallFloorTexs = ld.hallFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
							if (holder.TextureType == Misc.SchoolTexture.Wall)
								ld.hallWallTexs = ld.hallWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						case RoomCategory.Class:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
								ld.classCeilingTexs = ld.classCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
							if (holder.TextureType == Misc.SchoolTexture.Floor)
								ld.classFloorTexs = ld.classFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
							if (holder.TextureType == Misc.SchoolTexture.Wall)
								ld.classWallTexs = ld.classWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						case RoomCategory.Faculty:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
								ld.facultyCeilingTexs = ld.facultyCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
							if (holder.TextureType == Misc.SchoolTexture.Floor)
								ld.facultyFloorTexs = ld.facultyFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
							if (holder.TextureType == Misc.SchoolTexture.Wall)
								ld.facultyWallTexs = ld.facultyWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						default:
							// Operation here for custom rooms, soon.
							break;
					}
				}

			});
		}

		static string _modPath = string.Empty;

		public static string ModPath => _modPath;

	}

	static class ModInfo
	{

		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

		public const string PLUGIN_NAME = "Baldi\'s Basics Times";

		public const string PLUGIN_VERSION = "1.1.0";
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
