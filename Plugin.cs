using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BBTimes.CompatibilityModule;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.NPCs;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.Manager.InternalClasses;
using BBTimes.Manager.InternalClasses.LevelTypeWeights;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches.EnvironmentPatches;
using BBTimes.Plugin;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using UnityEngine;


namespace BBTimes
{
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)] // let's not forget this
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.levelloader", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.editorcustomrooms", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.newdecors", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.custommainmenusapi", BepInDependency.DependencyFlags.HardDependency)]

	// Soft dependencies / has exclusive compatibility with
	[BepInDependency("pixelguy.pixelmodding.baldiplus.newanimations", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("rost.moment.baldiplus.funsettings", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("io.github.luisrandomness.bbp_custom_posters", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.customvendingmachines", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.custommusics", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.grapplinghooktweaks", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("mrsasha5.baldi.basics.plus.advanced", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.leveleditor", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.infinitefloors", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.endlessfloors", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("Rad.cmr.baldiplus.arcaderenovations", BepInDependency.DependencyFlags.SoftDependency)]


	[BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class BasePlugin : BaseUnityPlugin
	{
		IEnumerator SetupFinal()
		{
			yield return 2;
			yield return "Calling custom data setup prefab post...";
			_cstData.ForEach(x => x.SetupPrefabPost());
			// Other stuff to setup
			yield return "Setup the rest of the assets...";
			SnowPile.SetupItemRandomization();
			Tresent.GatherShopItems();
		}
		IEnumerator SetupPost()
		{
			yield return 3;
			yield return "Creating post assets...";
			GameExtensions.TryRunMethod(SetupPostAssets);
			yield return "Forcing API to regenerate tags for Times...";
			IsModLoaded = true;
			Logger.LogDebug("Calling the api file manager to reload tags!");
			ModdedFileManager.Instance.RegenerateTags();
			yield return "Calling GC Collect...";

			// foreach (var floorData in BBTimesManager.floorDatas)
			// {
			// 	int idx = 0;
			// 	Debug.Log("Checking Floor: " + floorData.Key);
			// 	foreach (var classroom in floorData.Value.Classrooms)
			// 	{
			// 		Debug.Log($"{idx++}: Classroom {classroom.selection.name} contains activity: {classroom.selection.activity.prefab.name}");
			// 	}
			// }

			GC.Collect(); // Get any garbage I guess

			yield break;
		}

		void InitializeLevelTypeEnumsForSelection()
		{
			if (WeightedSelectionWithLevelType_AllStorage.All != null)
				return;

			var vals = EnumExtensions.GetValues<LevelType>();
			WeightedSelectionWithLevelType_AllStorage.All = [];
			for (int i = 0; i < vals.Length; i++)
				WeightedSelectionWithLevelType_AllStorage.All.Add((LevelType)vals[i]); // Get literally EVERY SINGLE LevelType value, even modded ones
		}

		void SetupPostAssets()
		{
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

			GameExtensions.TryRunMethod(BBTimesManager.SetupPostAssetsForSecretEnding);

			// ********************************************************** Holiday Baldi Setup ***************************************************************************

			if (Storage.IsChristmas)
			{
				GameExtensions.TryRunMethod(BBTimesManager.SetupChristmasHoliday);
			}
		}


		public static void PostSetup(AssetManager man) { } // This is gonna be used by other mods to patch after the BBTimesManager is done with the crap

		internal ConfigEntry<bool>
		disableOutside, disableHighCeilings, disableRedEndingCutscene,
		enableBigRooms, enableReplacementNPCsAsNormalOnes, enableYoutuberMode, forceChristmasMode, disableArcadeRennovationsSupport, disableSchoolhouseEscape, enableUnbalancedLegacyMode;
		internal List<string> disabledCharacters = [], disabledItems = [], disabledEvents = [], disabledBuilders = [];
		internal bool HasInfiniteFloors => Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors") ||
			Chainloader.PluginInfos.ContainsKey("Rad.cmr.baldiplus.arcaderenovations");

		private void Awake()
		{
			BBTimesManager.plug = this;

			const string
				ENV_SETTINGS = "Environment Settings",
				MISC_SETTINGS = "Miscellaneous Settings",
				SPECIAL_SETTINGS = "Holidays Settings";

			disableOutside = Config.Bind(ENV_SETTINGS, "Disable the outside", false, "Setting this \"true\" will completely disable the outside seen in-game. This should increase performance BUT will also change the seed layouts in the game.");
			disableHighCeilings = Config.Bind(ENV_SETTINGS, "Disable high ceilings", false, "Setting this \"true\" will completely disable the high ceilings from existing in pre-made levels (that includes the ones made with the Level Editor).");
			enableBigRooms = Config.Bind(ENV_SETTINGS, "Enable big rooms", false, "Setting this \"true\" will add the rest of the layouts Times also comes with. WARNING: These layouts completely unbalance the game, making it a lot harder than the usual.");
			disableSchoolhouseEscape = Config.Bind(ENV_SETTINGS, "Disable schoolhouse escape", false, "Setting this to \"true\" will disable entirely the schoolhouse escape sequence (the only exception is for the red sequence).");

			forceChristmasMode = Config.Bind(SPECIAL_SETTINGS, "Force enable christmas special", false, "Setting this to \"true\" will force the christmas special to be enabled.");

			enableYoutuberMode = Config.Bind(MISC_SETTINGS, "Enable youtuber mode", false, "Wanna get some exclusive content easily? Set this to \"true\" on and *everything* will have the weight of 9999.");
			enableReplacementNPCsAsNormalOnes = Config.Bind(MISC_SETTINGS, "Disable replacement feature", false, "Setting this \"true\" will allow replacement npcs to spawn as normal npcs instead, making the game considerably harder in some ways.");
			disableArcadeRennovationsSupport = Config.Bind(MISC_SETTINGS, "Disable arcade rennovations support", false, "Setting this to \"true\" disable any checks for arcade rennovations. This can be useful for RNG Floors, if you\'re having any issues.");
			disableRedEndingCutscene = Config.Bind(MISC_SETTINGS, "Disable the final cutscene", false, "If True, the cutscene played in the red sequence will be totally disabled.");
			enableUnbalancedLegacyMode = Config.Bind(MISC_SETTINGS, "Enable Unbalanced Legacy Mode", false, "If True, the old Times\' floor changes will be brought up back and make the game considerably unbalanced.");


			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAllConditionals();

			ModdedSaveGame.AddSaveHandler(new TimesHandler(Info, this));

			_modPath = AssetLoader.GetModPath(this);


			AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English);

#if KOFI
			MTM101BaldiDevAPI.AddWarningScreen(
				"<color=#c900d4>Ko-fi Exclusive Build!</color>\nKo-fi members helped make this possible. This Baldi\'s Basics Times build was made exclusively for supporters. Please, don't share it publicly. If you'd like to support future content, visit my Ko-fi page!",
				false
			);
#endif

			try
			{
				CompatibilityInitializer.InitializeOnAwake();
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to load compatibility module on Awake(). Printing error:");
				Debug.LogException(e);
			}


			LoadingEvents.RegisterOnAssetsLoaded(Info, BBTimesManager.InitializeContentCreation(), LoadingEventOrder.Pre);
			LoadingEvents.RegisterOnAssetsLoaded(Info, SetupPost(), LoadingEventOrder.Post); // Post
			LoadingEvents.RegisterOnAssetsLoaded(Info, SetupFinal(), LoadingEventOrder.Final); // Final


			PixelInternalAPI.ResourceManager.AddReloadLevelCallback((_, isNextlevel) => // Note: since it's always in the last level, there's no point to make a saving handler for this, since people cannot save in the middle of a level
			{
				MainGameManagerPatches.allowEndingToBePlayed = false;
			});

			// Literally just for this
			PixelInternalAPI.ResourceManager.AddGenStartCallback((_, _2, _3, _4) =>
			{
				ITM_BaldiYearbook.SetupYearbookPages();
			});

			GeneratorManagement.Register(this, GenerationModType.Base, (floorName, floorNum, sco) =>
			{
				var lds = sco.GetCustomLevelObjects();
				if (lds.Length == 0)
					return;

				foreach (var ld in lds)
				{
					bool shouldDisableOutside = ld.type == LevelType.Factory; // Factory has ceiling, so...
					ld.SetCustomModValue(Info, "Times_GenConfig_DisableOutside", shouldDisableOutside);
					ld.MarkAsNeverUnload(); // Maybe?

					// Legacy stuff
					var getGroup = new Func<string, RoomGroup>((name) =>
					{
						for (int i = 0; i < ld.roomGroup.Length; i++)
						{
							if (ld.roomGroup[i].name == name) return ld.roomGroup[i];
						}
						return null;
					});

					var classGroup = getGroup("Class");
					var officeGroup = getGroup("Office");
					var facultyGroup = getGroup("Faculty");

					bool legacy = enableUnbalancedLegacyMode.Value;

					if (floorName == BBTimesManager.F1)
					{
						// Custom datas
						ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(9, 9));

						// Legacy mode
						if (legacy)
						{
							try
							{
								ld.minSpecialRooms = 1;
								ld.maxSpecialRooms = 1;
								sco.additionalNPCs += 2;
								ld.additionTurnChance += 5;
								ld.bridgeTurnChance += 3;
								ld.extraDoorChance = 0.5f;
								classGroup.maxRooms = 5;
								facultyGroup.minRooms += 2;
								facultyGroup.maxRooms += 3;
								ld.maxHallsToRemove++;
								ld.maxPlots += 2;
								ld.minPlots += 1;
								ld.maxReplacementHalls += 2;
								ld.maxSize += new IntVector2(6, 5);
								ld.minSize += new IntVector2(5, 4);
								ld.maxSpecialBuilders += 2;
								ld.minHallsToRemove += 1;
								ld.minSpecialBuilders += 1;
								ld.outerEdgeBuffer += 5;
								ld.timeBonusLimit *= 1.2f;
							}
							catch
							{
								Debug.LogWarning($"BBTimes: Failed to correctly modify the floor. The cause is mostly due to some unfair change in the RoomGroups!");
							}
						}
						continue;
					}

					if (floorName == BBTimesManager.F2)
					{
						// Custom datas
						ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(9, 12));

						// Legacy mode
						if (legacy)
						{
							try
							{
								ld.maxSpecialRooms += 1;
								sco.additionalNPCs += 3;
								ld.additionTurnChance += 7;
								ld.bridgeTurnChance += 6;
								ld.extraDoorChance = 0.65f;
								classGroup.maxRooms += 1;
								facultyGroup.minRooms += 3;
								facultyGroup.maxRooms += 4;
								officeGroup.maxRooms += 1;
								ld.maxHallsToRemove++;
								ld.maxPlots += 2;
								ld.minPlots += 1;
								ld.maxReplacementHalls += 4;
								ld.maxSize += new IntVector2(7, 8);
								ld.minSize += new IntVector2(6, 5);
								ld.maxSpecialBuilders += 2;
								ld.minHallsToRemove += 1;
								ld.minSpecialBuilders += 1;
								ld.outerEdgeBuffer += 5;
								ld.timeBonusLimit *= 1.2f;
							}
							catch
							{
								Debug.LogWarning($"BBTimes: Failed to correctly modify the floor. The cause is mostly due to some unfair change in the RoomGroups!");
							}
						}
						continue;
					}

					if (floorName == BBTimesManager.F3)
					{
						// Custom datas
						ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(10, 13));

						// Legacy mode
						if (legacy)
						{
							try
							{
								ld.minSpecialRooms += 1;
								ld.maxSpecialRooms += 2;
								sco.additionalNPCs += 5;
								ld.additionTurnChance += 5;
								ld.bridgeTurnChance += 6;
								ld.extraDoorChance = 0.9f;
								classGroup.maxRooms += 3;
								facultyGroup.minRooms += 3;
								facultyGroup.maxRooms += 4;
								officeGroup.maxRooms += 2;
								ld.maxHallsToRemove++;
								ld.maxPlots += 3;
								ld.minPlots += 2;
								ld.maxReplacementHalls += 4;
								ld.maxSize += new IntVector2(7, 9);
								ld.minSize += new IntVector2(6, 5);
								ld.maxSpecialBuilders += 2;
								ld.minHallsToRemove += 1;
								ld.minSpecialBuilders += 1;
								ld.outerEdgeBuffer += 5;
								ld.timeBonusLimit *= 1.65f;
							}
							catch
							{
								Debug.LogWarning($"BBTimes: Failed to correctly modify the floor. The cause is mostly due to some unfair change in the RoomGroups!");
							}
						}
						continue;
					}

					if (floorName == BBTimesManager.F4)
					{
						// Custom datas
						ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(10, 15));

						// Legacy mode
						if (legacy)
						{
							try
							{
								ld.minSpecialRooms += 2;
								ld.maxSpecialRooms += 3;
								sco.additionalNPCs += 6;
								ld.additionTurnChance += 4;
								ld.bridgeTurnChance += 7;
								ld.extraDoorChance = 0.95f;
								classGroup.minRooms += 3;
								classGroup.maxRooms += 5;
								facultyGroup.minRooms += 6;
								facultyGroup.maxRooms += 8;
								officeGroup.minRooms += 2;
								officeGroup.maxRooms += 3;
								ld.maxHallsToRemove++;
								ld.maxPlots += 4;
								ld.minPlots += 3;
								ld.maxReplacementHalls += 8;
								ld.maxSize += new IntVector2(11, 13);
								ld.minSize += new IntVector2(8, 7);
								ld.maxSpecialBuilders += 5;
								ld.minHallsToRemove += 2;
								ld.minSpecialBuilders += 3;
								ld.outerEdgeBuffer += 7;
								ld.timeBonusLimit *= 2f;
							}
							catch
							{
								Debug.LogWarning($"BBTimes: Failed to correctly modify the floor. The cause is mostly due to some unfair change in the RoomGroups!");
							}
						}
						continue;
					}

					if (floorName == BBTimesManager.F5)
					{
						// Custom datas
						ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(12, BBTimesManager.MaximumNumballs));

						// Legacy mode
						if (legacy)
						{
							try
							{
								ld.minSpecialRooms += 2;
								ld.maxSpecialRooms += 3;
								sco.additionalNPCs += 6;
								ld.additionTurnChance += 4;
								ld.bridgeTurnChance += 7;
								ld.extraDoorChance = 0.95f;
								classGroup.minRooms += 4;
								classGroup.maxRooms += 7;
								facultyGroup.minRooms += 6;
								facultyGroup.maxRooms += 8;
								officeGroup.minRooms += 2;
								officeGroup.maxRooms += 3;
								ld.maxHallsToRemove++;
								ld.maxPlots += 4;
								ld.minPlots += 3;
								ld.maxReplacementHalls += 8;
								ld.maxSize += new IntVector2(14, 17);
								ld.minSize += new IntVector2(9, 10);
								ld.maxSpecialBuilders += 5;
								ld.minHallsToRemove += 2;
								ld.minSpecialBuilders += 3;
								ld.outerEdgeBuffer += 7;
								ld.timeBonusLimit *= 2f;
							}
							catch
							{
								Debug.LogWarning($"BBTimes: Failed to correctly modify the floor. The cause is mostly due to some unfair change in the RoomGroups!");
							}
						}
						continue;
					}

					if (floorName == BBTimesManager.END)
					{
						// Custom datas
						ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(9, 14));

						// Legacy mode
						if (legacy)
						{
							try
							{
								ld.maxSpecialRooms += 1;
								sco.additionalNPCs += 3;
								ld.additionTurnChance += 7;
								ld.bridgeTurnChance += 6;
								ld.extraDoorChance = 0.65f;
								classGroup.maxRooms += 1;
								facultyGroup.minRooms += 3;
								facultyGroup.maxRooms += 4;
								officeGroup.maxRooms += 1;
								ld.maxHallsToRemove++;
								ld.maxPlots += 2;
								ld.minPlots += 1;
								ld.maxReplacementHalls += 4;
								ld.maxSize += new IntVector2(7, 8);
								ld.minSize += new IntVector2(6, 5);
								ld.maxSpecialBuilders += 2;
								ld.minHallsToRemove += 1;
								ld.minSpecialBuilders += 1;
								ld.outerEdgeBuffer += 5;
								ld.timeBonusLimit *= 1.2f;
							}
							catch
							{
								Debug.LogWarning($"BBTimes: Failed to correctly modify the floor. The cause is mostly due to some unfair change in the RoomGroups!");
							}
						}
						continue;
					}
				}
			});

			GeneratorManagement.RegisterFieldTripLootChange(this, (fieldTripType, fieldTripLoot) =>
			{
				foreach (var floorPair in BBTimesManager.floorDatas)
				{
					var floor = floorPair.Value;
					List<WeightedItemObject> items = [.. floor.FieldTripItems];
					for (int i = 0; i < items.Count; i++)
					{
						// Remove disabled or duplicate items
						if (!Config.Bind("Item Settings", $"Enable {(items[i].selection.itemType == Items.Points ? items[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)items[i].selection.itemType))}",
							true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value || fieldTripLoot.potentialItems.Exists(x => x.selection == items[i].selection))
							items.RemoveAt(i--);
					}

					fieldTripLoot.potentialItems.AddRange(items);
				}
			});

			GeneratorManagement.Register(this, GenerationModType.Addend, (floorName, floorNum, sco) =>
			{
				InitializeLevelTypeEnumsForSelection();

				KeyValuePair<string, FloorData>? floordatapair = BBTimesManager.floorDatas.FirstOrDefault(x => x.Key == floorName);
				if (!floordatapair.HasValue || floordatapair.Value.Value == null) // Fail safe by adding another null check
				{
					Debug.LogWarning("Failed to get floor data for level: " + floorName);
					return;
				}

				var floordata = floordatapair.Value.Value;

				Debug.Log($"Adding floorData ({floordatapair.Value.Key}) to floor ({floorName})");

				// ******************* ELEMENTS THAT DEPENDS ON SCENEOBJECT SOLELY ***********************

				// ----- NPCs -----
				for (int i = 0; i < floordata.NPCs.Count; i++)
				{
					// --- Filter disabled NPCs and add to potential/forced lists ---
					if (!Config.Bind("NPC Settings", $"Enable {floordata.NPCs[i].selection.name}", !_disabledByDefault_NPCs.Contains(floordata.NPCs[i].GetType()), "If set to true, this character will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						disabledCharacters.Add(floordata.NPCs[i].selection.name);
						floordata.NPCs.RemoveAt(i--);
						continue;
					}

					var dat = floordata.NPCs[i].selection.GetComponent<INPCPrefab>();
					if (enableReplacementNPCsAsNormalOnes.Value || dat == null || dat.GetReplacementNPCs() == null || dat.GetReplacementNPCs().Length == 0)
						sco.potentialNPCs.Add(floordata.NPCs[i].GetWeightedSelection()); // Only non-replacement Npcs
					else
						sco.forcedNpcs = sco.forcedNpcs.AddToArray(floordata.NPCs[i].selection); // This field will be used for getting the replacement npcs, since they are outside the normal potential npcs, they can replace the existent ones at any time
				}

				// ----- Shop Items -----
				for (int i = 0; i < floordata.ShopItems.Count; i++)
				{
					// --- Filter disabled shop items and add to shopItems array ---
					string itemName = floordata.ShopItems[i].selection.itemType == Items.Points ? floordata.ShopItems[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)floordata.ShopItems[i].selection.itemType);
					if (!Config.Bind("Item Settings", $"Enable {itemName}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						if (!disabledItems.Contains(itemName))
							disabledItems.Add(itemName);
						floordata.ShopItems.RemoveAt(i--);
						continue;
					}

					sco.shopItems = sco.shopItems.AddToArray(floordata.ShopItems[i]);
				}

				// Old workaround for api 8.1.x
				// if (floorName == BBTimesManager.END)
				//	return;


				// *************** ELEMENTS THAT DEPENDS IN LEVEL OBJECTS ******************

				foreach (var ld in sco.GetCustomLevelObjects())
				{

					// ----- Items -----
					for (int i = 0; i < floordata.Items.Count; i++)
					{
						// --- Filter disabled items and add to potentialItems ---
						string itemName = floordata.Items[i].selection.itemType == Items.Points ? floordata.Items[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)floordata.Items[i].selection.itemType);
						if (!Config.Bind("Item Settings", $"Enable {itemName}",
							true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						{
							if (!disabledItems.Contains(itemName))
								disabledItems.Add(itemName);
							floordata.Items.RemoveAt(i--);
							continue;
						}

						if (floordata.Items[i].AcceptsLevelType(ld.type))
							ld.potentialItems = ld.potentialItems.AddToArray(floordata.Items[i].GetWeightedSelection());
					}

					// ----- Forced Items -----
					for (int i = 0; i < floordata.ForcedItems.Count; i++)
					{
						// --- Filter disabled forced items and add to forcedItems ---
						string itemName = floordata.ForcedItems[i].selection.itemType == Items.Points ? floordata.ForcedItems[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)floordata.ForcedItems[i].selection.itemType);
						if (!Config.Bind("Item Settings", $"Enable {itemName}",
							true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						{
							if (!disabledItems.Contains(itemName))
								disabledItems.Add(itemName);
							floordata.ForcedItems.RemoveAt(i--);
							continue;
						}

						if (floordata.ForcedItems[i].AcceptsLevelType(ld.type))
							ld.forcedItems.Add(floordata.ForcedItems[i].GetWeightedSelection());
					}

					// ----- Random Events -----
					for (int i = 0; i < floordata.Events.Count; i++)
					{
						// --- Filter disabled random events and add to randomEvents ---
						if (!Config.Bind("Random Event Settings", $"Enable {floordata.Events[i].selection.name}", true, "If set to true, this random event will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						{
							disabledEvents.Add(floordata.Events[i].selection.name);
							floordata.Events.RemoveAt(i--);
							continue;
						}
						if (floordata.Events[i].AcceptsLevelType(ld.type))
							ld.randomEvents.Add(floordata.Events[i].GetWeightedSelection());
					}

					// ----- Forced Object Builders -----
					for (int i = 0; i < floordata.ForcedObjectBuilders.Count; i++)
					{
						// --- Filter disabled forced object builders and add to forcedStructures ---
						if (!Config.Bind("Structure Settings", $"Enable {floordata.ForcedObjectBuilders[i].GetWeightedSelection().prefab.name}", true,
							"If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						{
							disabledBuilders.Add(floordata.ForcedObjectBuilders[i].GetWeightedSelection().prefab.name);
							floordata.ForcedObjectBuilders.RemoveAt(i--);
							continue;
						}
						if (floordata.ForcedObjectBuilders[i].AcceptsLevelType(ld.type))
							ld.forcedStructures = ld.forcedStructures.AddToArray(floordata.ForcedObjectBuilders[i].GetWeightedSelection());
					}

					// ----- Weighted Object Builders -----
					for (int i = 0; i < floordata.WeightedObjectBuilders.Count; i++)
					{
						// --- Filter disabled weighted object builders and add to potentialStructures ---
						if (!Config.Bind("Structure Settings", $"Enable {floordata.WeightedObjectBuilders[i].selection.prefab.name}", true,
							"If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
						{
							if (!disabledBuilders.Contains(floordata.WeightedObjectBuilders[i].selection.prefab.name))
								disabledBuilders.Add(floordata.WeightedObjectBuilders[i].selection.prefab.name);
							floordata.WeightedObjectBuilders.RemoveAt(i--);
							continue;
						}
						if (floordata.WeightedObjectBuilders[i].AcceptsLevelType(ld.type))
							ld.potentialStructures = ld.potentialStructures.AddToArray(floordata.WeightedObjectBuilders[i].GetWeightedSelection());
					}


					// ----- Room Groups and Special Rooms -----
					for (int i = 0; i < floordata.RoomAssets.Count; i++)
					{
						if (floordata.RoomAssets[i].AcceptsLevelType(ld.type))
							ld.roomGroup = ld.roomGroup.AddToArray(floordata.RoomAssets[i].GetWeightedSelection());
					}

					for (int i = 0; i < floordata.SpecialRooms.Count; i++)
					{
						// --- Filter disabled special rooms and add to special rooms (since removing their cblds are not a good idea) ---
						if (floordata.SpecialRooms[i].HasRoomName && !Config.Bind("Special Room Settings", $"Enable {floordata.SpecialRooms[i].RoomName}", true,
							"If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
							continue;

						if (floordata.SpecialRooms[i].AcceptsLevelType(ld.type))
							ld.potentialSpecialRooms = ld.potentialSpecialRooms.AddToArray(floordata.SpecialRooms[i].GetWeightedSelection());
					}

					// Only these below ignores level types, even halls.
					RoomGroup[] groups = [ld.roomGroup.First(x => x.name == "Class"), ld.roomGroup.First(x => x.name == "Faculty"), ld.roomGroup.First(x => x.name == "Office")];
					groups[0].potentialRooms = groups[0].potentialRooms.AddRangeToArray([.. floordata.Classrooms]);
					groups[1].potentialRooms = groups[1].potentialRooms.AddRangeToArray([.. floordata.Faculties]);
					groups[2].potentialRooms = groups[2].potentialRooms.AddRangeToArray([.. floordata.Offices]);

					// ----- Special Halls -----
					foreach (var fl in floordata.Halls)
					{
						if (fl.Value)
							ld.potentialPostPlotSpecialHalls = ld.potentialPostPlotSpecialHalls.AddToArray(fl.Key);
						else
							ld.potentialPrePlotSpecialHalls = ld.potentialPrePlotSpecialHalls.AddToArray(fl.Key);

					}

					// ----- *MODDED* School Textures -----
					foreach (var holder in floordata.SchoolTextures)
					{
						// Hall customization here
						if (holder.SelectionLimiters[0] == RoomCategory.Hall)
						{
							switch (holder.TextureType)
							{
								case Misc.SchoolTexture.Ceiling:
									ld.hallCeilingTexs = ld.hallCeilingTexs.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								case Misc.SchoolTexture.Floor:
									ld.hallFloorTexs = ld.hallFloorTexs.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								case Misc.SchoolTexture.Wall:
									ld.hallWallTexs = ld.hallWallTexs.AddToArray(holder.Selection.ToWeightedTexture());
									break;
								default:
									break;
							}
							continue;
						}

						// Modded rooms below
						string name = EnumExtensions.GetExtendedName<RoomCategory>((int)holder.SelectionLimiters[0]);
						var group = ld.roomGroup.FirstOrDefault(x => x.potentialRooms.Any(z => z.selection.category == holder.SelectionLimiters[0]));
						if (group == null)
						{
							// Debug.LogWarning("BBTimes: Failed to load texture for room category: " + name);
							continue;
						}
						// Debug.Log($"BBTimes: Adding texture {holder.Selection.selection.name} to room category: {name} in level: {ld.name}");
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
					}

				}

			});
		}

		readonly static Type[] _disabledByDefault_NPCs = [typeof(Glubotrony)]; // As requested by MSF

		static string _modPath = string.Empty;

		public static string ModPath => _modPath;

		internal static List<IObjectPrefab> _cstData = [];

		public bool IsModLoaded { get; private set; } = false;

	}

	public class TimesHandler(BepInEx.PluginInfo info, BasePlugin plug) : ModdedSaveGameIOBinary // Dummy class structure from the api
	{
		readonly private BepInEx.PluginInfo _info = info;
		public override BepInEx.PluginInfo pluginInfo => _info;

		readonly BasePlugin plug = plug;

		public override void Save(BinaryWriter writer)
		{
			try // Silently surpress any exceptions
			{
				writer.Write((byte)0);
			}
			catch { }
		}

		public override void Load(BinaryReader reader)
		{
			try
			{
				reader.ReadByte();
			}
			catch { }
		}

		public override void Reset() { }

		public override string[] GenerateTags()
		{
			List<string> tags = [];

			plug.disabledCharacters.ForEach(x => tags.Add($"Times_DisabledCharacterTag_{x}"));
			plug.disabledBuilders.ForEach(x => tags.Add($"Times_DisabledBuilderTag_{x}"));
			plug.disabledEvents.ForEach(x => tags.Add($"Times_DisabledEventTag_{x}"));
			plug.disabledItems.ForEach(x => tags.Add($"Times_DisabledItemTag_{x}"));


			if (plug.disableHighCeilings.Value)
				tags.Add("Times_Config_DisableHighCeilingsFunction");

			if (plug.enableBigRooms.Value)
				tags.Add("Times_Config_EnableBigRoomsMode");

			if (plug.enableReplacementNPCsAsNormalOnes.Value)
				tags.Add("Times_Config_ReplacementDisable");

			if (plug.enableYoutuberMode.Value)
				tags.Add("Times_Config_YoutuberMode");

			if (Storage.IsChristmas)
				tags.Add("Times_Specials_Christmas");

			if (plug.HasInfiniteFloors && plug.disableArcadeRennovationsSupport.Value)
				tags.Add("Times_Config_DisableArcadeRennovationsSupport");

			return [.. tags];
		}

		public override string DisplayTags(string[] tags)
		{
			for (int i = 0; i < tags.Length; i++)
			{
				if (tags[i].StartsWith("Times_DisabledCharacterTag_"))
				{
					tags[i] = "Disabled Character: " + tags[i].Split('_')[2]; // The third item from this array should be the Character's name
					continue;
				}
				if (tags[i].StartsWith("Times_DisabledBuilderTag_"))
				{
					tags[i] = "Disabled Builder: " + tags[i].Split('_')[2];
					continue;
				}
				if (tags[i].StartsWith("Times_DisabledEventTag_"))
				{
					tags[i] = "Disabled Random Event: " + tags[i].Split('_')[2];
					continue;
				}
				if (tags[i].StartsWith("Times_DisabledItemTag_"))
				{
					tags[i] = "Disabled Item: " + tags[i].Split('_')[2];
					continue;
				}

				tags[i] = tags[i] switch
				{
					"Times_Config_DisableHighCeilingsFunction" => "Disabled Highceilings for Special Rooms",
					"Times_Config_EnableBigRoomsMode" => "Enabled big room layouts",
					"Times_Config_ReplacementDisable" => "Character replacement feature disabled",
					"Times_Config_YoutuberMode" => "Youtube Mode enabled",
					"Times_Specials_Christmas" => "Christmas mode enabled",
					"Times_Config_DisableArcadeRennovationsSupport" => "No arcade renovations support",
					_ => tags[i]
				};
			}

			return base.DisplayTags(tags);
		}

		public override bool TagsReady() =>
			plug.IsModLoaded;

	}

	static class ModInfo
	{

		public const string PLUGIN_GUID = "pixelguy.pixelmodding.baldiplus.bbextracontent";

		public const string PLUGIN_NAME = "Baldi\'s Basics Times";
	}

	// Some cheats

}
