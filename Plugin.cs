using BBTimes.CompatibilityModule;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.Events;
using BBTimes.CustomContent.Objects;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace BBTimes
{
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)] // let's not forget this
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.levelloader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.editorcustomrooms", BepInDependency.DependencyFlags.HardDependency)]

	// Soft dependencies / has exclusive compatibility with
	[BepInDependency("pixelguy.pixelmodding.baldiplus.newanimations", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("rost.moment.baldiplus.extramod", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("io.github.luisrandomness.bbp_custom_posters", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.customvendingmachines", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.custommusics", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.grapplinghooktweaks", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("baldi.basics.plus.advanced.mod", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.stackableitems", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.leveleditor", BepInDependency.DependencyFlags.SoftDependency)]

	[BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class BasePlugin : BaseUnityPlugin
	{
		IEnumerator SetupPost()
		{
			yield return 2;
			yield return "Calling custom data setup prefab post...";
			_cstData.ForEach(x => x.SetupPrefabPost());
			// Other stuff to setup
			yield return "Setup the rest of the assets...";
			BlackOut.sodaMachineLight = GenericExtensions.FindResourceObject<SodaMachine>().GetComponent<MeshRenderer>().materials[1].GetTexture("_LightGuide"); // Yeah, this one I'm looking for lol
			yield return "Creating post assets...";
			GameExtensions.TryRunMethod(SetupPostAssets);
			yield break;
		}

		void SetupPostAssets()
		{
			Sprite[] sprs = TextureExtensions.LoadSpriteSheet(2, 1, 55f, Path.Combine(ModPath, "objects", "LightSwitch", "lightSwitchSheet.png"));
			var lightSwitch = ObjectCreationExtensions.CreateSpriteBillboard(sprs[1], false)
				.AddSpriteHolder(0, LayerStorage.iClickableLayer);
			lightSwitch.gameObject.layer = 0;
			lightSwitch.name = "sprite";

			var sw = lightSwitch.transform.parent.gameObject.AddComponent<LightSwitch>();
			sw.name = "LightSwitch";
			sw.gameObject.ConvertToPrefab(true);

			sw.gameObject.AddBoxCollider(Vector3.forward * -1.05f, new(2f, 10f, 1f), true);
			sw.sprOff = sprs[0];
			sw.sprOn = sprs[1];
			sw.renderer = lightSwitch;
			sw.audMan = sw.gameObject.CreatePropagatedAudioManager();
			sw.audSwitch = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "objects", "LightSwitch", "LightSwitch_Toggle.wav"), string.Empty, SoundType.Effect, Color.white);
			sw.audSwitch.subtitle = false;



			var rs = BBTimesManager.AddFunctionToEverythingExcept<LightSwitchSpawner>((x) => x.standardLightCells.Count != 0, RoomCategory.Special, RoomCategory.Test, RoomCategory.Buffer, RoomCategory.Hall, RoomCategory.Mystery, RoomCategory.Store, RoomCategory.FieldTrip, RoomCategory.Null);
			rs.ForEach(x => x.lightPre = sw);

			try
			{
				CompatibilityInitializer.InitializePostOnLoadMods();
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to load compatibility modules due to an error:");
				Debug.LogException(e);
				MTM101BaldiDevAPI.CauseCrash(Info, e);
			}

			var fieldTrip = GenericExtensions.FindResourceObject<FieldTripBaseRoomFunction>();
			foreach (var floor in BBTimesManager.floorDatas)
				fieldTrip.potentialItems = fieldTrip.potentialItems.AddRangeToArray([.. floor.FieldTripItems.Except(fieldTrip.potentialItems)]);
			// No guaranteed items required to not mess with the good ones in
		}




		public static void PostSetup(AssetManager man) { } // This is gonna be used by other mods to patch after the BBTimesManager is done with the crap

		internal ConfigEntry<bool> disableOutside, disableHighCeilings;
		internal Dictionary<string, ConfigEntry<bool>> enabledCharacters = [], enabledItems = [], enabledStructures = [];

		private void Awake()
		{
			disableOutside = Config.Bind("Environment Settings", "Disable the outside", false, "Setting this \"true\" will completely disable the outside seen in-game. This should increase performance BUT will also change the seed layouts in the game.");
			disableHighCeilings = Config.Bind("Environment Settings", "Disable high ceilings", false, "Setting this \"true\" will completely disable the high ceilings from existing in pre-made levels (that includes the ones made with the Level Editor).");

			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAllConditionals();

			ModdedSaveGame.AddSaveHandler(Info);

			_modPath = AssetLoader.GetModPath(this);
			BBTimesManager.plug = this;

			CompatibilityInitializer.InitializeOnAwake();
			MainMenuPatch.newMidi = AssetLoader.MidiFromMod("timeNewJingle", this, "misc", "Audios", "newJingle.mid");


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

				RoomGroup[] groups = [ld.roomGroup.First(x => x.name == "Class"), ld.roomGroup.First(x => x.name == "Faculty"), ld.roomGroup.First(x => x.name == "Office")];
				ld.timeBonusVal *= 2;
				if (floorName == "F1")
				{
					ld.minSpecialRooms = 0; // Chance to have no special room
					ld.additionalNPCs += 2;
					ld.additionTurnChance += 10;
					ld.bridgeTurnChance += 4;
					ld.outerEdgeBuffer += 1;
					ld.extraDoorChance += 0.2f;
					ld.windowChance += 0.2f;
					groups[0].maxRooms = 5;
					ld.maxFacultyRooms += 2;
					ld.maxHallsToRemove += 1;
					ld.maxPlots += 1;
					ld.maxReplacementHalls += 1;
					ld.maxSize += new IntVector2(8, 5);
					ld.maxSpecialBuilders += 2;
					ld.minFacultyRooms += 1;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(3, 5);
					ld.timeBonusLimit *= 1.8f;
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
					groups[1].minRooms++;
					groups[1].maxRooms += 4;
					groups[0].maxRooms = 8;
					groups[0].minRooms = 6;
					groups[1].stickToHallChance = 0.85f;
					ld.maxHallsToRemove += 2;
					ld.maxPlots += 2;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 3;
					ld.maxSize += new IntVector2(9, 6);
					ld.maxSpecialBuilders += 2;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(5, 6);
					ld.minSpecialBuilders += 1;
					ld.maxOffices = 2;
					ld.specialRoomsStickToEdge = false;
					ld.maxLightDistance += 2;
					ld.timeBonusLimit *= 1.8f;
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
					groups[1].minRooms++;
					groups[1].maxRooms += 4;
					groups[0].maxRooms = 10;
					groups[2].maxRooms += 2;
					groups[1].stickToHallChance = 0.85f;
					ld.maxHallsToRemove++;
					ld.maxPlots += 2;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 2;
					ld.maxSize += new IntVector2(9, 6);
					ld.minSize += new IntVector2(5, 6);
					ld.maxSpecialBuilders += 2;
					ld.minHallsToRemove += 1;
					ld.minSpecialBuilders += 1;
					ld.outerEdgeBuffer += 5;
					ld.potentialSpecialRooms = ld.potentialSpecialRooms.AddRangeToArray([.. Resources.FindObjectsOfTypeAll<RoomAsset>() // Playground in F3
						.Where(x => x.name.StartsWith("Playground"))
						.ConvertAll(x => new WeightedRoomAsset() { selection = x, weight = 45 })]);
					ld.timeBonusLimit *= 1.8f;
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
					ld.maxHallsToRemove += 2;
					ld.maxPlots += 3;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 3;
					ld.maxSize += new IntVector2(10, 8);
					ld.maxSpecialBuilders += 2;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(7, 7);
					ld.minSpecialBuilders += 1;
					ld.maxLightDistance += 3;
					groups[1].stickToHallChance = 0.85f;
					groups[2].maxRooms = 3;
					groups[2].minRooms = 2;
					groups[1].minRooms++;
					groups[1].maxRooms += 5;
					groups[0].minRooms = 7;
					groups[0].maxRooms = 9;
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

				foreach (var npc in floordata.NPCs)
				{
					if (!Config.Bind("NPC Settings", $"Enable {npc.selection.name}", true, "If set to true, this character will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						continue;

					var dat = npc.selection.GetComponent<INPCPrefab>();
					if (dat == null || dat.GetReplacementNPCs() == null || dat.GetReplacementNPCs().Length == 0)
						ld.potentialNPCs.Add(npc); // Only non-replacement Npcs
					else
						ld.forcedNpcs = ld.forcedNpcs.AddToArray(npc.selection); // This field will be used for getting the replacement npcs, since they are outside the normal potential npcs, they can replace the existent ones at any time
				}

				List<WeightedItemObject> acceptableItems = new(floordata.Items);
				for (int i = 0; i < acceptableItems.Count; i++)
					if (!Config.Bind("Item Settings", $"Enable {(acceptableItems[i].selection.itemType == Items.Points ? acceptableItems[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)acceptableItems[i].selection.itemType))}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						acceptableItems.RemoveAt(i--);


				ld.potentialItems = ld.potentialItems.AddRangeToArray([.. acceptableItems]);

				List<ItemObject> items = new(floordata.ForcedItems);
				for (int i = 0; i < items.Count; i++)
					if (!Config.Bind("Item Settings", $"Enable {(items[i].itemType == Items.Points ? items[i].nameKey : EnumExtensions.GetExtendedName<Items>((int)items[i].itemType))}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						items.RemoveAt(i--);
				ld.forcedItems.AddRange(items);

				acceptableItems = new(floordata.ShopItems);
				for (int i = 0; i < acceptableItems.Count; i++)
					if (!Config.Bind("Item Settings", $"Enable {(acceptableItems[i].selection.itemType == Items.Points ? acceptableItems[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)acceptableItems[i].selection.itemType))}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						acceptableItems.RemoveAt(i--);

				ld.shopItems = ld.shopItems.AddRangeToArray([.. floordata.ShopItems]);

				List<WeightedRandomEvent> events = new(floordata.Events);
				for (int i = 0; i < events.Count; i++)
					if (!Config.Bind("Random Event Settings", $"Enable {events[i].selection.name}", true, "If set to true, this random event will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						events.RemoveAt(i--);

				ld.randomEvents.AddRange(events);

				List<ObjectBuilder> objBlds = new(floordata.ForcedObjectBuilders);
				for (int i = 0; i < objBlds.Count; i++)
					if (!Config.Bind("Structure Settings", $"Enable {(objBlds[i].obstacle != Obstacle.Null ?
						EnumExtensions.GetExtendedName<Obstacle>((int)objBlds[i].obstacle) : objBlds[i].name)}", true, "If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						objBlds.RemoveAt(i--);

				ld.forcedSpecialHallBuilders = ld.forcedSpecialHallBuilders.AddRangeToArray([.. objBlds]);

				List<WeightedObjectBuilder> rngObjBlds = new(floordata.WeightedObjectBuilders);
				for (int i = 0; i < rngObjBlds.Count; i++)
					if (!Config.Bind("Structure Settings", $"Enable {(rngObjBlds[i].selection.obstacle != Obstacle.Null ? 
						EnumExtensions.GetExtendedName<Obstacle>((int)rngObjBlds[i].selection.obstacle) : rngObjBlds[i].selection.name)}", true, "If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						rngObjBlds.RemoveAt(i--);

				ld.specialHallBuilders = ld.specialHallBuilders.AddRangeToArray([.. rngObjBlds]);

				List<RandomHallBuilder> hallObjBlds = new(floordata.HallBuilders);
				for (int i = 0; i < hallObjBlds.Count; i++)
					if (!Config.Bind("Structure Settings", $"Enable {hallObjBlds[i].selectable.name}", true, "If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						rngObjBlds.RemoveAt(i--);

				ld.standardHallBuilders = ld.standardHallBuilders.AddRangeToArray([.. hallObjBlds]);

				ld.roomGroup = ld.roomGroup.AddRangeToArray([.. floordata.RoomAssets]);
				ld.potentialSpecialRooms = ld.potentialSpecialRooms.AddRangeToArray([.. floordata.SpecialRooms]);

				RoomGroup[] groups = [ld.roomGroup.First(x => x.name == "Class"), ld.roomGroup.First(x => x.name == "Faculty"), ld.roomGroup.First(x => x.name == "Office")];
				groups[0].potentialRooms = groups[0].potentialRooms.AddRangeToArray([.. floordata.Classrooms]);
				groups[1].potentialRooms = groups[1].potentialRooms.AddRangeToArray([.. floordata.Faculties]);
				groups[2].potentialRooms = groups[2].potentialRooms.AddRangeToArray([.. floordata.Offices]);


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
								groups[0].ceilingTexture = groups[0].ceilingTexture.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Floor)
								groups[0].floorTexture = groups[0].floorTexture.AddToArray(holder.Selection.ToWeightedTexture());
							else if (holder.TextureType == Misc.SchoolTexture.Wall)
								groups[0].wallTexture = groups[0].wallTexture.AddToArray(holder.Selection.ToWeightedTexture());
							break;
						case RoomCategory.Faculty:
							if (holder.TextureType == Misc.SchoolTexture.Ceiling)
							{
								groups[1].ceilingTexture = groups[1].ceilingTexture.AddToArray(holder.Selection.ToWeightedTexture());
								groups[2].ceilingTexture = groups[2].ceilingTexture.AddToArray(holder.Selection.ToWeightedTexture());
							}
							else if (holder.TextureType == Misc.SchoolTexture.Floor)
							{
								groups[1].floorTexture = groups[1].floorTexture.AddToArray(holder.Selection.ToWeightedTexture());
								groups[2].floorTexture = groups[2].floorTexture.AddToArray(holder.Selection.ToWeightedTexture());
							}
							else if (holder.TextureType == Misc.SchoolTexture.Wall)
							{
								groups[1].wallTexture = groups[1].wallTexture.AddToArray(holder.Selection.ToWeightedTexture());
								groups[2].wallTexture = groups[2].wallTexture.AddToArray(holder.Selection.ToWeightedTexture());
							}
							break;
						default:
							string name = EnumExtensions.GetExtendedName<RoomCategory>((int)holder.SelectionLimiters[0]);
							var group = ld.roomGroup.FirstOrDefault(x => x.potentialRooms.Any(z => z.selection.category == holder.SelectionLimiters[0]));
							if (group == null)
							{
								Debug.LogWarning("BBTimes: Failed to load texture for room category: " + name);
								break;
							}
							switch (holder.TextureType)
							{
								case Misc.SchoolTexture.Ceiling:
									group.ceilingTexture = group.ceilingTexture.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								case Misc.SchoolTexture.Floor:
									group.floorTexture = group.floorTexture.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								case Misc.SchoolTexture.Wall:
									group.wallTexture = group.wallTexture.AddToArray(holder.Selection.ToWeightedTexture());
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

		internal static List<IObjectPrefab> _cstData = [];

	}

	static class ModInfo
	{

		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

		public const string PLUGIN_NAME = "Baldi\'s Basics Times";
	}

	// Some cheats

}
