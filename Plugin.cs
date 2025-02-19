using BBTimes.CompatibilityModule;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.SecretEndingComponents;
using BBTimes.CustomContent.Events;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches;
using BBTimes.Plugin;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using PlusLevelFormat;
using PlusLevelLoader;
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
	[BepInDependency("pixelguy.pixelmodding.baldiplus.newdecors", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.custommainmenusapi", BepInDependency.DependencyFlags.HardDependency)]

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
	[BepInDependency("pixelguy.pixelmodding.baldiplus.infinitefloors", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("mtm101.rulerp.baldiplus.endlessfloors", BepInDependency.DependencyFlags.SoftDependency)]
	[BepInDependency("Rad.cmr.baldiplus.arcaderenovations", BepInDependency.DependencyFlags.SoftDependency)]


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
			SnowPile.SetupItemRandomization();
			Tresent.GatherShopItems();
			yield return "Creating post assets...";
			GameExtensions.TryRunMethod(SetupPostAssets);
			yield return "Forcing API to regenerate tags for Times...";
			IsModLoaded = true;
			Logger.LogDebug("Calling the api file manager to reload tags!");
			ModdedFileManager.Instance.RegenerateTags();

			yield break;
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

			var fieldTrip = GenericExtensions.FindResourceObject<FieldTripBaseRoomFunction>();
			foreach (var floor in BBTimesManager.floorDatas)
			{
				List<WeightedItemObject> items = new(floor.FieldTripItems);
				for (int i = 0; i < items.Count; i++)
				{
					if (!Config.Bind("Item Settings", $"Enable {(items[i].selection.itemType == Items.Points ? items[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)items[i].selection.itemType))}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value || fieldTrip.potentialItems.Any(x => x.selection == items[i].selection))
						items.RemoveAt(i--);
				}

				fieldTrip.potentialItems = fieldTrip.potentialItems.AddRangeToArray([.. items]);
			}
			// No guaranteed items required to not mess with the good ones in


			// Times Ending Manager Setup
			var sceneObjectClone = Instantiate(GenericExtensions.FindResourceObjectByName<SceneObject>("EndlessPremadeMedium"));
			sceneObjectClone.name = "TimesSecretEnding";

			sceneObjectClone.extraAsset = Instantiate(sceneObjectClone.extraAsset);
			sceneObjectClone.extraAsset.name = "TimesSecretExtraAsset";
			sceneObjectClone.extraAsset.npcSpawnPoints.Clear();
			sceneObjectClone.extraAsset.npcsToSpawn.Clear();
			sceneObjectClone.extraAsset.lightMode = LightMode.Additive;
			sceneObjectClone.extraAsset.minLightColor = Color.white;

			sceneObjectClone.levelContainer = null;
			sceneObjectClone.levelNo = 99;
			sceneObjectClone.nextLevel = null;
			sceneObjectClone.levelTitle = "???";
			sceneObjectClone.nameKey = "???";
			sceneObjectClone.shopItems = [];
			sceneObjectClone.skyboxColor = Color.black;
			sceneObjectClone.usesMap = false;
			sceneObjectClone.levelObject = null;


			using (BinaryReader reader = new(File.OpenRead(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLevel.cbld"))))
			{
				sceneObjectClone.levelAsset = CustomLevelLoader.LoadLevelAsset(LevelExtensions.ReadLevel(reader));
				sceneObjectClone.levelAsset.name = "TimesSecretEndingAsset";
				sceneObjectClone.levelAsset.rooms[0].ceilTex = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLabCeiling.png"));
				sceneObjectClone.levelAsset.rooms[0].wallTex = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLabWall.png"));
				sceneObjectClone.levelAsset.rooms[0].florTex = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLabFloor.png"));

				sceneObjectClone.levelAsset.rooms[0].doorMats = ObjectCreators.CreateDoorDataObject("TimesSecretLabMetalDoor",
					AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "smallMetalDoorOpen.png")),
					AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "smallMetalDoorClosed.png")));

				var doorTextureMask = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "metalDoorMask.png"));
				sceneObjectClone.levelAsset.rooms[0].doorMats.open.SetTexture("_Mask", doorTextureMask);
				sceneObjectClone.levelAsset.rooms[0].doorMats.shut.SetTexture("_Mask", doorTextureMask);

				sceneObjectClone.levelAsset.posters.Add(new()
				{
					poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "liveTubeMakeUp.png"))]),
					position = new(16, 12),
					direction = Direction.North
				});
				sceneObjectClone.levelAsset.posters.Add(new()
				{
					poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "levelGenMakeUp.png"))]),
					position = new(15, 10),
					direction = Direction.South
				});
				sceneObjectClone.levelAsset.posters.Add(new()
				{
					poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "chk_funFormula.png"))]),
					position = new(15, 7),
					direction = Direction.South
				});
				sceneObjectClone.levelAsset.posters.Add(new()
				{
					poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "chk_theNoWinFormula.png"))]),
					position = new(15, 9),
					direction = Direction.North
				});
				sceneObjectClone.levelAsset.posters.Add(new()
				{
					poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "chk_noRealWin.png"))]),
					position = new(16, 8),
					direction = Direction.East
				});
				sceneObjectClone.levelAsset.rooms[0].hasActivity = false;
				sceneObjectClone.levelAsset.rooms[0].activity = null;
				// Setup door clone
				var newDoor = (StandardDoor)sceneObjectClone.levelAsset.doors[0].doorPre.SafeDuplicatePrefab(true);
				newDoor.audDoorShut = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "metalDoorShut.wav")), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
				newDoor.audDoorOpen = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "metalDoorOpen.wav")), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
				newDoor.name = "SmallMetalDoor";

				sceneObjectClone.levelAsset.doors.ForEach(d => d.doorPre = newDoor);
			}

			var newManager = new GameObject("TimesSecretEndingManager").SetAsPrefab(true).AddComponent<TimesSecretEndingManager>();


			newManager.Mono_GetACopyFromFields(sceneObjectClone.manager);

			newManager.canvas = ObjectCreationExtensions.CreateCanvas();
			newManager.canvas.transform.SetParent(newManager.transform);
			newManager.canvas.gameObject.SetActive(false);
			newManager.canvas.name = newManager.name + "Canvas";

			newManager.activeImage = ObjectCreationExtensions.CreateImage(newManager.canvas, TextureExtensions.CreateSolidTexture(480, 360, Color.black));
			newManager.activeImage.name = "TimesEndScreen";
			var baldiReference = GenericExtensions.FindResourceObject<Baldi>();

			newManager.audSlap = baldiReference.slap;
			newManager.audLoseSounds = baldiReference.loseSounds;

			newManager.timesScreen = AssetLoader.SpriteFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretTimesEnd.jpg"), Vector2.one * 0.5f);
			newManager.audSeeYaSoon = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence_End.wav")), "Vfx_SecBAL_EndSequence_SeeYa", SoundType.Voice, Color.green);

			newManager.levelNo = 99;
			newManager.spawnNpcsOnInit = false;
			newManager.managerNameKey = "???";

			newManager.audMan = newManager.gameObject.CreateAudioManager(15f, 25f).MakeAudioManagerNonPositional();
			newManager.audHummmmm = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "spookyNoisesForEnding.mp3")), string.Empty, SoundType.Music, Color.white);
			newManager.audHummmmm.subtitle = false;

			sceneObjectClone.manager = newManager;
			MainGameManagerPatches.secretEndingObj = sceneObjectClone;

			//var scene = GenericExtensions.FindResourceObjects<SceneObject>().First(x => x.levelTitle == "F1");
			//scene.nextLevel = sceneObjectClone;
			//scene.levelObject.finalLevel = true;

			// ********************************************************** Christmas Baldi Setup ***************************************************************************

			if (BooleanStorage.IsChristmas)
			{
				var baldiSPrites = TextureExtensions.LoadSpriteSheet(6, 1, 30f, BBTimesManager.MiscPath, BBTimesManager.TextureFolder, BBTimesManager.GetAssetName("christmasBaldi.png"));
				var chBaldi = ObjectCreationExtensions.CreateSpriteBillboard(baldiSPrites[0])
					.AddSpriteHolder(out var chBaldiRenderer, 4f, LayerStorage.iClickableLayer); // Baldo offset should be exactly 5f + hisDefaultoffset
				chBaldi.gameObject.AddBoxCollider(Vector3.up * 5f, new(2.5f, 10f, 2.5f), true);
				chBaldi.name = "Times_ChristmasBaldi";
				chBaldiRenderer.name = "Times_ChristmasBaldi_Renderer";

				chBaldi.gameObject.ConvertToPrefab(true); // He won't be in the editor, he's made specifically for christmas mode

				var christmasBaldi = chBaldi.gameObject.AddComponent<ChristmasBaldi>();

				christmasBaldi.audMan = christmasBaldi.gameObject.CreatePropagatedAudioManager(95f, 175f);
				christmasBaldi.present = BBTimesManager.man.Get<ItemObject>("times_itemObject_Present");
				christmasBaldi.audBell = BBTimesManager.man.Get<SoundObject>("audRing");
				christmasBaldi.audIntro = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BAL_PresentIntro.wav")), "Vfx_BAL_Pitstop_PresentIntro_1", SoundType.Voice, Color.green);
				christmasBaldi.audIntro.additionalKeys = [
					new() { key = "Vfx_BAL_Pitstop_PresentIntro_2", time = 2.417f },
					new() { key = "Vfx_BAL_Pitstop_PresentIntro_3", time = 5.492f },
					new() { key = "Vfx_BAL_Wow", time = 9.544f },
					new() { key = "Vfx_BAL_Pitstop_PresentIntro_4", time = 11.029f },
					new() { key = "Vfx_BAL_Pitstop_PresentIntro_5", time = 14.059f }
					];

				christmasBaldi.audBuyItem = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BAL_buypresent.wav")), "Vfx_BAL_Pitstop_MerryChristmas", SoundType.Voice, Color.green);

				christmasBaldi.audNoYtps = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BAL_needYtpsForPresent.wav")), "Vfx_BAL_Pitstop_Nopresent_1", SoundType.Voice, Color.green);
				christmasBaldi.audNoYtps.additionalKeys = [
					new() { key = "Vfx_BAL_Pitstop_Nopresent_2", time = 0.818f }
					];

				christmasBaldi.audGenerous = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BAL_Pitstop_Generous.wav")), "Vfx_BAL_Pitstop_Generous_1", SoundType.Voice, Color.green);
				christmasBaldi.audGenerous.additionalKeys = [
					new() { key = "Vfx_BAL_Pitstop_Generous_2", time = 2.597f }
					];

				christmasBaldi.audCollectingPresent = [
					ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BAL_Pitstop_Thanks1.wav")), "Vfx_BAL_Pitstop_Thanks1", SoundType.Voice, Color.green),
					ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BAL_Pitstop_Thanks2.wav")), "Vfx_BAL_Pitstop_Thanks2", SoundType.Voice, Color.green)
					];

				var volumeAnimator = christmasBaldi.gameObject.AddComponent<SpriteVolumeAnimator>();
				volumeAnimator.audMan = christmasBaldi.audMan;
				volumeAnimator.renderer = chBaldiRenderer;
				volumeAnimator.volumeMultipler = 1.2f;
				volumeAnimator.sprites = baldiSPrites;

				var pitstopAsset = GenericExtensions.FindResourceObjectByName<LevelAsset>("Pitstop"); // Find shop pitstop here
				pitstopAsset.tbos.Add(new() { direction = Direction.North, position = new(30, 11), prefab = christmasBaldi });
			}
		}




		public static void PostSetup(AssetManager man) { } // This is gonna be used by other mods to patch after the BBTimesManager is done with the crap

		internal ConfigEntry<bool> disableOutside, disableHighCeilings, enableBigRooms, enableReplacementNPCsAsNormalOnes, enableYoutuberMode, forceChristmasMode, disableArcadeRennovationsSupport, disableSchoolhouseEscape;
		internal List<string> disabledCharacters = [], disabledItems = [], disabledEvents = [], disabledBuilders = [];
		internal bool HasInfiniteFloors => Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors") ||
			Chainloader.PluginInfos.ContainsKey("Rad.cmr.baldiplus.arcaderenovations");

		private void Awake()
		{
			BBTimesManager.plug = this;

			disableOutside = Config.Bind("Environment Settings", "Disable the outside", false, "Setting this \"true\" will completely disable the outside seen in-game. This should increase performance BUT will also change the seed layouts in the game.");
			disableHighCeilings = Config.Bind("Environment Settings", "Disable high ceilings", false, "Setting this \"true\" will completely disable the high ceilings from existing in pre-made levels (that includes the ones made with the Level Editor).");
			enableBigRooms = Config.Bind("Environment Settings", "Enable big rooms", false, "Setting this \"true\" will add the rest of the layouts Times also comes with. WARNING: These layouts completely unbalance the game, making it a lot harder than the usual.");
			enableReplacementNPCsAsNormalOnes = Config.Bind("NPC Settings", "Disable replacement feature", false, "Setting this \"true\" will allow replacement npcs to spawn as normal npcs instead, making the game considerably harder in some ways.");
			enableYoutuberMode = Config.Bind("Misc Settings", "Enable youtuber mode", false, "Wanna get some exclusive content easily? Set this to \"true\" on and *everything* will have the weight of 9999.");
			forceChristmasMode = Config.Bind("Specials Settings", "Force enable christmas special", false, "Setting this to \"true\" will force the christmas special to be enabled.");
			disableArcadeRennovationsSupport = Config.Bind("Misc Settings", "Disable arcade rennovations support", false, "Setting this to \"true\" disable any checks for arcade rennovations. This can be useful for RNG Floors, if you\'re having any issues.");
			disableSchoolhouseEscape = Config.Bind("Environment Settings", "Disable schoolhouse escape", false, "Setting this to \"true\" will disable entirely the schoolhouse escape sequence (the only exception is for the red sequence).");


			Harmony harmony = new(ModInfo.PLUGIN_GUID);
			harmony.PatchAllConditionals();

			ModdedSaveGame.AddSaveHandler(new TimesHandler(Info, this));

			_modPath = AssetLoader.GetModPath(this);


			AssetLoader.LoadLocalizationFolder(Path.Combine(ModPath, "Language", "English"), Language.English);
			
			try
			{
				CompatibilityInitializer.InitializeOnAwake();
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to load compatibility module on Awake(). Printing error:");
				Debug.LogException(e);
			}


			LoadingEvents.RegisterOnAssetsLoaded(Info, BBTimesManager.InitializeContentCreation(), false);

			LoadingEvents.RegisterOnAssetsLoaded(Info, SetupPost(), true); // Post


			PixelInternalAPI.ResourceManager.AddReloadLevelCallback((_, isNextlevel) => // Note: since it's always in the last level, there's no point to make a saving handler for this, since people cannot save in the middle of a level
			{
				if (!isNextlevel)
					MainGameManagerPatches.allowEndingToBePlayed = false;
			});

			GeneratorManagement.Register(this, GenerationModType.Finalizer, (_, _2, sco) =>
			{
				var ld = sco.levelObject;
				if (ld == null) return;

				ld.minSpecialBuilders = Mathf.Min(ld.minSpecialBuilders, ld.potentialStructures.Length);
				ld.maxSpecialBuilders = Mathf.Min(ld.maxSpecialBuilders, ld.potentialStructures.Length); // Workaround to avoid a bug (that results in a crash) caused by an oversight from Mystman12.
			});

			GeneratorManagement.Register(this, GenerationModType.Base, (floorName, floorNum, sco) =>
			{
				var nld = sco.levelObject;
				if (nld == null)
					return;
				if (nld is not CustomLevelObject ld)
					return;

#if CHEAT
				Debug.Log($"Level Object loaded as: {floorName} with num: {floorNum}. LevelObj name: {ld.name}");
				Debug.Log("------- ITEM DATA -------");
				Debug.Log("Floor " + floorName);
				ld.shopItems.Do(x => Debug.Log($"{x.selection.itemType} >> {x.selection.price} || weight: {x.weight} || cost: {x.selection.value}"));
#endif
				ld.SetCustomModValue(Info, "Times_GenConfig_DisableOutside", false);
				ld.MarkAsNeverUnload(); // Maybe?

				RoomGroup[] groups = [ld.roomGroup.First(x => x.name == "Class"), ld.roomGroup.First(x => x.name == "Faculty"), ld.roomGroup.First(x => x.name == "Office")];
				ld.timeBonusVal *= 2;


				if (floorName == "F1")
				{
					//var builder = Resources.FindObjectsOfTypeAll<RotoHallBuilder>()[0];
					//for (int i = 0; i < 5; i++)
					//	ld.forcedSpecialHallBuilders = ld.forcedSpecialHallBuilders.AddToArray(builder);

					//sco.additionalNPCs += 2;
					ld.additionTurnChance += 10;
					ld.bridgeTurnChance += 4;
					ld.outerEdgeBuffer += 1;
					ld.extraDoorChance += 0.2f;
					groups[0].maxRooms = 5;
					ld.maxHallsToRemove += 1;
					ld.maxPlots += 1;
					ld.maxReplacementHalls += 1;
					ld.maxSize += new IntVector2(5, 3);
					ld.maxSpecialBuilders += 2;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(3, 1);
					ld.timeBonusLimit *= 1.8f;

					// Custom datas
					ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(9, 9));
					return;
				}

				if (floorName == "F2")
				{
					ld.deadEndBuffer = 4;
					ld.minSpecialRooms = 1;
					ld.maxSpecialRooms = 2;
					//sco.additionalNPCs += 4;
					ld.additionTurnChance += 5;
					ld.bridgeTurnChance += 3;
					ld.outerEdgeBuffer += 3;
					ld.extraDoorChance += 0.3f;
					groups[1].minRooms++;
					groups[1].maxRooms += 4;
					groups[0].maxRooms = 8;
					groups[0].minRooms = 6;
					groups[1].stickToHallChance = 0.85f;
					ld.maxHallsToRemove += 2;
					ld.maxPlots += 2;
					ld.minPlots += 1;
					ld.maxReplacementHalls += 3;
					ld.maxSize += new IntVector2(6, 4);
					ld.maxSpecialBuilders += 2;
					ld.minHallsToRemove += 1;
					ld.minSize += new IntVector2(4, 2);
					ld.minSpecialBuilders += 1;
					ld.specialRoomsStickToEdge = false;
					ld.maxLightDistance += 2;
					ld.timeBonusLimit *= 1.8f;

					// Custom datas
					ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(9, 12));
					return;
				}

				if (floorName == "F3")
				{
					ld.minSpecialRooms += 1;
					ld.maxSpecialRooms += 2;
					//sco.additionalNPCs += 4;
					ld.additionTurnChance += 15;
					ld.bridgeTurnChance += 6;
					ld.extraDoorChance = 0.5f;
					groups[1].minRooms++;
					groups[1].maxRooms += 4;
					groups[0].maxRooms = 10;
					groups[2].maxRooms += 2;
					groups[1].stickToHallChance = 0.85f;
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
					ld.potentialSpecialRooms = ld.potentialSpecialRooms.AddRangeToArray([.. Resources.FindObjectsOfTypeAll<RoomAsset>() // Playground in F3
						.Where(x => x.name.StartsWith("Playground"))
						.ConvertAll(x => new WeightedRoomAsset() { selection = x, weight = 45 })]);
					ld.timeBonusLimit *= 1.8f;

					// Custom datas
					ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(12, BBTimesManager.MaximumNumballs));
					return;
				}

				if (floorName == "END")
				{
					ld.minSpecialRooms = 1;
					ld.maxSpecialRooms = 2;
					ld.deadEndBuffer = 3;
					//sco.additionalNPCs += 4;
					ld.additionTurnChance += 25;
					ld.bridgeTurnChance += 6;
					ld.outerEdgeBuffer += 3;
					ld.extraDoorChance += 0.3f;
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

					// Custom datas
					ld.SetCustomModValue(Info, "Times_EnvConfig_MathMachineNumballsMinMax", new IntVector2(9, 14));
					return;
				}

			});

			GeneratorManagement.Register(this, GenerationModType.Addend, (floorName, floorNum, sco) =>
			{
				var ld = sco.levelObject;
				if (ld == null)
					return;

				var floordata = BBTimesManager.floorDatas.FirstOrDefault(x => x.Floor == floorName);
				if (floordata == null)
				{
					//Debug.LogWarning("Failed to get floor data for level: " + ld.name);
					return;
				}

				bool isChristmas = BooleanStorage.IsChristmas;

				for (int i = 0; i < floordata.NPCs.Count; i++)
				{
					if (!Config.Bind("NPC Settings", $"Enable {floordata.NPCs[i].selection.name}", true, "If set to true, this character will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						disabledCharacters.Add(floordata.NPCs[i].selection.name);
						floordata.NPCs.RemoveAt(i--);
						continue;
					}

					var dat = floordata.NPCs[i].selection.GetComponent<INPCPrefab>();
					if (enableReplacementNPCsAsNormalOnes.Value || dat == null || dat.GetReplacementNPCs() == null || dat.GetReplacementNPCs().Length == 0)
					{
						if (isChristmas && floordata.NPCs[i].selection.GetMeta().tags.Contains(ConstantStorage.ChristmasSpecial_TimesTag))
							floordata.NPCs[i].weight = Mathf.FloorToInt(floordata.NPCs[i].weight * 1.45f);

						sco.potentialNPCs.Add(floordata.NPCs[i]); // Only non-replacement Npcs
					}
					else
					{
						if (isChristmas && floordata.NPCs[i].selection.GetMeta().tags.Contains(ConstantStorage.ChristmasSpecial_TimesTag))
							dat.ReplacementWeight = Mathf.FloorToInt(dat.ReplacementWeight * 1.45f);

						sco.forcedNpcs = sco.forcedNpcs.AddToArray(floordata.NPCs[i].selection); // This field will be used for getting the replacement npcs, since they are outside the normal potential npcs, they can replace the existent ones at any time
					}
				}

				//List<WeightedItemObject> acceptableItems = floordata.Items;
				for (int i = 0; i < floordata.Items.Count; i++)
				{
					string itemName = floordata.Items[i].selection.itemType == Items.Points ? floordata.Items[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)floordata.Items[i].selection.itemType);
					if (!Config.Bind("Item Settings", $"Enable {itemName}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						disabledItems.Add(itemName);
						floordata.Items.RemoveAt(i--);
						continue;
					}

					if (isChristmas && floordata.Items[i].selection.GetMeta().tags.Contains(ConstantStorage.ChristmasSpecial_TimesTag))
						floordata.Items[i].weight = Mathf.FloorToInt(floordata.Items[i].weight * 1.95f);
				}


				ld.potentialItems = ld.potentialItems.AddRangeToArray([.. floordata.Items]);

				//List<ItemObject> items = floordata.ForcedItems;
				for (int i = 0; i < floordata.ForcedItems.Count; i++)
				{
					string itemName = floordata.ForcedItems[i].itemType == Items.Points ? floordata.ForcedItems[i].nameKey : EnumExtensions.GetExtendedName<Items>((int)floordata.ForcedItems[i].itemType);
					if (!Config.Bind("Item Settings", $"Enable {itemName}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						if (!disabledItems.Contains(itemName))
							disabledItems.Add(itemName);
						floordata.ForcedItems.RemoveAt(i--);
					}
				}
				ld.forcedItems.AddRange(floordata.ForcedItems);

				//acceptableItems = new(floordata.ShopItems);
				for (int i = 0; i < floordata.ShopItems.Count; i++)
				{
					string itemName = floordata.ShopItems[i].selection.itemType == Items.Points ? floordata.ShopItems[i].selection.nameKey : EnumExtensions.GetExtendedName<Items>((int)floordata.ShopItems[i].selection.itemType);
					if (!Config.Bind("Item Settings", $"Enable {itemName}",
						true, "If set to true, this item will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						if (!disabledItems.Contains(itemName))
							disabledItems.Add(itemName);
						floordata.ShopItems.RemoveAt(i--);
					}
				}

				sco.shopItems = sco.shopItems.AddRangeToArray([.. floordata.ShopItems]);

				//List<WeightedRandomEvent> events = new(floordata.Events);
				for (int i = 0; i < floordata.Events.Count; i++)
				{
					if (!Config.Bind("Random Event Settings", $"Enable {floordata.Events[i].selection.name}", true, "If set to true, this random event will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						disabledEvents.Add(floordata.Events[i].selection.name);
						floordata.Events.RemoveAt(i--);
						continue;
					}

					if (isChristmas && floordata.Events[i].selection.GetMeta().tags.Contains(ConstantStorage.ChristmasSpecial_TimesTag))
						floordata.Events[i].weight = Mathf.FloorToInt(floordata.Events[i].weight * 1.5f);
				}

				ld.randomEvents.AddRange(floordata.Events);

				//List<ObjectBuilder> objBlds = new(floordata.ForcedObjectBuilders);
				for (int i = 0; i < floordata.ForcedObjectBuilders.Count; i++)
				{
					if (!Config.Bind("Structure Settings", $"Enable {floordata.ForcedObjectBuilders[i].prefab.name}", true,
						"If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						disabledBuilders.Add(floordata.ForcedObjectBuilders[i].prefab.name);
						floordata.ForcedObjectBuilders.RemoveAt(i--);
					}
				}

				ld.forcedStructures = ld.forcedStructures.AddRangeToArray([.. floordata.ForcedObjectBuilders]);

				//List<WeightedObjectBuilder> rngObjBlds = new(floordata.WeightedObjectBuilders);
				for (int i = 0; i < floordata.WeightedObjectBuilders.Count; i++)
				{
					if (!Config.Bind("Structure Settings", $"Enable {floordata.WeightedObjectBuilders[i].selection.prefab.name}", true,
						"If set to true, this structure will be included in the maps made by the Level Generator (eg. Hide and Seek).").Value)
					{
						if (!disabledBuilders.Contains(floordata.WeightedObjectBuilders[i].selection.prefab.name))
							disabledBuilders.Add(floordata.WeightedObjectBuilders[i].selection.prefab.name);
						floordata.WeightedObjectBuilders.RemoveAt(i--);
					}
				}

				ld.potentialStructures = ld.potentialStructures.AddRangeToArray([.. floordata.WeightedObjectBuilders]);

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
								//Debug.LogWarning("BBTimes: Failed to load texture for room category: " + name);
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

		public bool IsModLoaded { get; private set; } = false;

	}

	public class TimesHandler(BepInEx.PluginInfo info, BasePlugin plug) : ModdedSaveGameIOBinary // Dummy class structure from the api
	{
		readonly private BepInEx.PluginInfo _info = info;
		public override BepInEx.PluginInfo pluginInfo => _info;

		readonly BasePlugin plug = plug;

		public override void Save(BinaryWriter writer)
		{
			writer.Write((byte)0);
		}

		public override void Load(BinaryReader reader)
		{
			reader.ReadByte();
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

			if (BooleanStorage.IsChristmas)
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
