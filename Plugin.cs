using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using BBTimes.Manager;
using System.Linq;
using UnityEngine;
using MTM101BaldAPI.AssetTools;
using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.Events;
using System.Collections;
using System.Collections.Generic;


namespace BBTimes.Plugin
{
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)] // let's not forget this
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.levelloader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.editorcustomrooms", BepInDependency.DependencyFlags.HardDependency)]

	[BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
    public class BasePlugin : BaseUnityPlugin
    {
		IEnumerator SetupPost()
		{
			yield return 2;
			yield return "Calling custom data setup prefab post...";
			_cstData.ForEach(x => x.PostPrefabSetup());
			// Other stuff to setup
			yield return "Setup the rest of the assets...";
			ITM_GoldenQuarter.quarter = ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value;
			BlackOut.sodaMachineLight = GenericExtensions.FindResourceObject<SodaMachine>().GetComponent<MeshRenderer>().materials[1].GetTexture("_LightGuide"); // Yeah, this one I'm looking for lol
			yield break;
		}

		public static void PostSetup(AssetManager man) { } // This is gonna be used by other mods to patch after the BBTimesManager is done with the crap

		private void Awake()
		{
			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAll();
			
			_modPath = AssetLoader.GetModPath(this);
			BBTimesManager.plug = this;

			LoadingEvents.RegisterOnAssetsLoaded(Info, BBTimesManager.InitializeContentCreation(), false);

			LoadingEvents.RegisterOnAssetsLoaded(Info, SetupPost(), true); // Post

			GeneratorManagement.Register(this, GenerationModType.Base, (floorName, floorNum, ld) =>
			{
#if CHEAT
				Debug.Log($"Level Object loaded as: {floorName} with num: {floorNum}. LevelObj name: {ld.name}");
				Debug.Log("------- ITEM DATA -------");
				Debug.Log("Floor " + floorName);
				ld.shopItems.Do(x => Debug.Log($"{x.selection.itemType} >> {x.selection.price} || weight: {x.weight} || cost: {x.selection.value}"));
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
					ld.minSpecialRooms = 1;
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
					return;
				}

				if (floorName == "F3")
				{
					ld.minSpecialRooms += 1;
					ld.maxSpecialRooms += 2;
					ld.additionalNPCs += 4;
					ld.additionTurnChance += 15;
					ld.bridgeTurnChance += 6;
					ld.extraDoorChance = 0.5f;
					ld.windowChance += 0.35f;
					ld.maxClassRooms = 12;
					ld.maxFacultyRooms += 4;
					ld.maxHallsToRemove++;
					ld.maxPlots += 2;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 2;
					ld.maxSize += new IntVector2(9, 6);
					ld.minSize += new IntVector2(5, 6);
					ld.maxSpecialBuilders += 2;
					ld.minFacultyRooms += 1;
					ld.minHallsToRemove += 1;
					ld.minSpecialBuilders += 1;
					ld.classStickToHallChance = 0.75f;
					ld.facultyStickToHallChance = 0.85f;
					ld.outerEdgeBuffer += 5;
					ld.maxOffices += 2;
					ld.officeStickToHallChance = 0.9f;
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
				
				ld.potentialItems = ld.potentialItems.AddRangeToArray([.. floordata.Items]);
				ld.randomEvents.AddRange(floordata.Events);
				ld.forcedSpecialHallBuilders = ld.forcedSpecialHallBuilders.AddRangeToArray([.. floordata.ForcedObjectBuilders]);
				ld.specialHallBuilders = ld.specialHallBuilders.AddRangeToArray([.. floordata.WeightedObjectBuilders]);
				ld.standardHallBuilders = ld.standardHallBuilders.AddRangeToArray([.. floordata.HallBuilders]);
				ld.shopItems = ld.shopItems.AddRangeToArray([.. floordata.ShopItems]);
				ld.fieldTripItems.AddRange(floordata.FieldTripItems);
				ld.additionalRoomTypes.AddRange(floordata.RoomAssets.Values);
				ld.additionalTextureGroups.AddRange(floordata.RoomAssets.Keys);
				ld.potentialSpecialRooms = ld.potentialSpecialRooms.AddRangeToArray([.. floordata.SpecialRooms]);
				ld.potentialClassRooms = ld.potentialClassRooms.AddRangeToArray([.. floordata.Classrooms]);
				ld.potentialFacultyRooms = ld.potentialFacultyRooms.AddRangeToArray([.. floordata.Faculties]);
				ld.potentialOffices = ld.potentialOffices.AddRangeToArray([.. floordata.Offices]);
				foreach (var fl in floordata.Halls)
				{
					if (fl.Value)
						ld.potentialPostPlotSpecialHalls = ld.potentialPostPlotSpecialHalls.AddToArray(fl.Key);
					else
						ld.potentialPrePlotSpecialHalls = ld.potentialPrePlotSpecialHalls.AddToArray(fl.Key);

				}

				foreach (var holder in floordata.SchoolTextures) // Add the school textures
				{
					switch (holder.SelectionLimiters[0])
					{
						case RoomCategory.Hall:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
								ld.hallCeilingTexs = ld.hallCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Floor)
								ld.hallFloorTexs = ld.hallFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Wall)
								ld.hallWallTexs = ld.hallWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						case RoomCategory.Class:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
								ld.classCeilingTexs = ld.classCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Floor)
								ld.classFloorTexs = ld.classFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Wall)
								ld.classWallTexs = ld.classWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						case RoomCategory.Faculty:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
								ld.facultyCeilingTexs = ld.facultyCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Floor)
								ld.facultyFloorTexs = ld.facultyFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Wall)
								ld.facultyWallTexs = ld.facultyWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						default:
							string name = EnumExtensions.GetExtendedName<RoomCategory>((int)holder.SelectionLimiters[0]);
							var group = ld.additionalTextureGroups.FirstOrDefault(x => x.name == name);
							if (group == null)
							{
								Debug.LogWarning("BBTimes: Failed to load texture for room category: " + name);
								break;
							}
							switch (holder.TextureType)
							{
								case Misc.SchoolTexture.Ceiling:
									group.potentialCeilTextures = group.potentialCeilTextures.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								case Misc.SchoolTexture.Floor:
									group.potentialFloorTextures = group.potentialFloorTextures.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								case Misc.SchoolTexture.Wall:
									group.potentialWallTextures = group.potentialWallTextures.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								default:
									break;
							}
								
							break;
					}
				}

			});
		}

		static string _modPath = string.Empty;

		public static string ModPath => _modPath;

		internal const string CharacterRadarGUID = "org.aestheticalz.baldi.characterradar";

		internal static List<CustomBaseData> _cstData = [];

	}

	static class ModInfo
	{

		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

		public const string PLUGIN_NAME = "Baldi\'s Basics Times";

		public const string PLUGIN_VERSION = "1.1.0";
	}

	// Some cheats

}
