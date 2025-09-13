using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BBTimes.CompatibilityModule;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomComponents.SecretEndingComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager.InternalClasses;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using BBTimes.ModPatches.NpcPatches;
using BBTimes.Plugin;
using BepInEx.Bootstrap;
using CustomMainMenusAPI;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using TMPro;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager // basically holds the logic to create everything to the game
	{
		public static bool EditorExists => Chainloader.PluginInfos.ContainsKey(Storage.guid_LevelStudio);
		internal static BasePlugin plug;
		internal static IEnumerator InitializeContentCreation()
		{
			yield return 14 + (EditorExists ? 1 : 0);

			yield return "Loading assets...";
			GameExtensions.TryRunMethod(SetAssets);
			yield return "Loading Secret Ending Pre-Assets...";
			GameExtensions.TryRunMethod(SetupPreAssetsForSecretEnding);
			yield return "Adding extra component for some objects...";
			GameExtensions.TryRunMethod(AddExtraComponentsForSomeObjects);
			yield return "Creating sprite billboards...";
			GameExtensions.TryRunMethod(CreateSpriteBillboards);
			yield return "Creating cube maps...";
			GameExtensions.TryRunMethod(CreateCubeMaps);
			yield return "Creating musics...";
			GameExtensions.TryRunMethod(GetMusics);
			yield return "Creating npcs...";
			GameExtensions.TryRunMethod(CreateNPCs);
			yield return "Creating items...";
			GameExtensions.TryRunMethod(CreateItems);
			yield return "Creating events...";
			GameExtensions.TryRunMethod(CreateEvents);
			yield return "Creating object builders...";
			GameExtensions.TryRunMethod(CreateObjBuilders);
			yield return "Creating windows...";
			GameExtensions.TryRunMethod(CreateWindows);
			yield return "Creating custom rooms...";
			GameExtensions.TryRunMethod(CreateCustomRooms);
			yield return "Creating room functions...";
			GameExtensions.TryRunMethod(CreateRoomFunctions);
			yield return "Creating school textures...";
			GameExtensions.TryRunMethod(CreateSchoolTextures);
			yield return "Creating map icons...";
			GameExtensions.TryRunMethod(GetIcons);

			if (plug.enableYoutuberMode.Value)
			{
				foreach (var flDatPair in floorDatas)
				{
					var flDat = flDatPair.Value;
					flDat.Classrooms.ForEach(x => x.weight = 9999);
					flDat.Events.ForEach(x => x.weight = 9999);
					flDat.Faculties.ForEach(x => x.weight = 9999);
					flDat.FieldTripItems.ForEach(x => x.weight = 9999);
					flDat.Halls.Do(x => x.Key.weight = 9999);
					flDat.Items.Do(x => x.weight = 9999);
					flDat.NPCs.Do(x => x.weight = 9999);
					flDat.Offices.ForEach(x => x.weight = 9999);
					flDat.ShopItems.ForEach(x => x.weight = 9999);
					flDat.WeightedObjectBuilders.ForEach(x => x.weight = 9999);
				}
			}
			else
				IncreaseWeightsBasedOnHolidays();

			BasePlugin.PostSetup(man);

			yield break;
		}

		static void IncreaseWeightsBasedOnHolidays()
		{

			bool isChristmas = Storage.IsChristmas;
			foreach (var floorDataPair in floorDatas)
			{
				// ******** Christmas Check ********

				// -- Npcs --
				var floorData = floorDataPair.Value;
				for (int i = 0; i < floorData.NPCs.Count; i++)
				{
					if (isChristmas && floorData.NPCs[i].selection.GetMeta().tags.Contains(Storage.ChristmasSpecial_TimesTag))
					{
						floorData.NPCs[i].weight = Mathf.FloorToInt(floorData.NPCs[i].weight * 1.45f);
						var dat = floorData.NPCs[i].selection.GetComponent<INPCPrefab>();
						if (dat != null) // Check if it has replacement too
							dat.ReplacementWeight = Mathf.FloorToInt(dat.ReplacementWeight * 1.45f);
					}
				}

				// -- Items --
				for (int i = 0; i < floorData.Items.Count; i++)
				{
					if (isChristmas && floorData.Items[i].selection.GetMeta().tags.Contains(Storage.ChristmasSpecial_TimesTag))
						floorData.Items[i].weight = Mathf.FloorToInt(floorData.Items[i].weight * 1.95f);
				}

				// -- Random Events --
				for (int i = 0; i < floorData.Events.Count; i++)
				{
					if (isChristmas && floorData.Events[i].selection.GetMeta().tags.Contains(Storage.ChristmasSpecial_TimesTag))
						floorData.Events[i].weight = Mathf.FloorToInt(floorData.Events[i].weight * 1.5f);
				}
			}
		}

		static void AddExtraComponentsForSomeObjects()
		{
			GenericExtensions.FindResourceObjects<MainGameManager>().Do(x => x.gameObject.AddComponent<MainGameManagerExtraComponent>()); // Adds extra component for every MainGameManager
			GenericExtensions.FindResourceObjects<EnvironmentController>().Do(x => x.gameObject.AddComponent<EnvironmentControllerData>());
			GenericExtensions.FindResourceObjects<PlayerManager>().Do(x => x.gameObject.AddComponent<PlayerAttributesComponent>()); // Basic setup
			GenericExtensions.FindResourceObjects<CullingManager>().Do(x => x.gameObject.AddComponent<NullCullingManager>().cullMan = x);
		}

		static void SetAssets()
		{
			//AverageCheck(RoomCategory.Class);
			//AverageCheck(RoomCategory.Faculty);
			//AverageCheck(RoomCategory.Office);

			//static void AverageCheck(RoomCategory cat)
			//{
			//	Debug.LogWarning($"------------- GETTING AVERAGE OF EVERY ROOM ASSET OF CATEGORY \"{cat}\" -------------");
			//	float magTotal = 0f;
			//	int total = 0;
			//	IntVector2 sizeAxis = new();

			//	foreach (var room in GenericExtensions.FindResourceObjects<RoomAsset>())
			//	{
			//		if (room.category != cat)
			//			continue;

			//		var foundSize = room.GetRoomSize();

			//		magTotal += foundSize.Magnitude();
			//		sizeAxis += foundSize;
			//		total++;
			//	}

			//	Debug.Log("Average magnitude size: " + (magTotal / total) + $" | Average size for ({sizeAxis.x / total},{sizeAxis.z / total})");
			//}

			try
			{
				CompatibilityInitializer.InitializeOnLoadMods();
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to load compatibility modules due to an error:");
				Debug.LogException(e);
				MTM101BaldiDevAPI.CauseCrash(plug.Info, e);
			}
			// Some materials
			ObjectCreationExtension.defaultMaterial = GenericExtensions.FindResourceObjectByName<Material>("Locker_Red"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultDustMaterial = GenericExtensions.FindResourceObjectByName<Material>("DustTest");
			ObjectCreationExtension.defaultCubemap = GenericExtensions.FindResourceObjectByName<Cubemap>("Cubemap_DayStandard");
			ObjectCreationExtension.mapMaterial = GenericExtensions.FindResourceObjectByName<MapIcon>("Icon_Prefab").spriteRenderer.material;
			man.Add("TransparentTileMaterial", GenericExtensions.FindResourceObjectByName<Material>("TileBase_Alpha"));
			man.Add("buttonPre", GenericExtensions.FindResourceObject<RotoHallBuilder>().buttonPre);
			man.AddFromResources<StandardDoorMats>();


			// Floor Data LevelObject reference setup
			foreach (var obj in Resources.FindObjectsOfTypeAll<SceneObject>())
			{
				var lds = obj.GetCustomLevelObjects();
				if (lds.Length == 0) continue;

				KeyValuePair<string, FloorData>? data = floorDatas.FirstOrDefault(x => x.Key == obj.levelTitle);
				if (!data.HasValue || data.Value.Value == null) // Why didn't I add this earlier, bruh
					continue;                                   // In case FloorData is null?? When does that happen?

				data.Value.Value.levelObjects = lds;

				// Some additional LevelObject
				foreach (var ld in lds)
					ld.SetCustomModValue(plug.Info, "Times_EnvConfig_ExtraWindowsToSpawn", new List<WindowObjectHolder>() { new(null, 100, [RoomCategory.Office]) });

			}

			// Make a transparent texture
			ObjectCreationExtension.transparentTex = TextureExtensions.CreateSolidTexture(1, 1, Color.clear);

			// Make a black texture
			ObjectCreationExtension.blackTex = TextureExtensions.CreateSolidTexture(1, 1, Color.black);

			// Base plane for easy.. quads
			var basePlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
			var renderer = basePlane.GetComponent<MeshRenderer>();
			renderer.material = GenericExtensions.FindResourceObjectByName<Material>("TileBase");
			basePlane.transform.localScale = Vector3.one * LayerStorage.TileBaseOffset; // Gives the tile size
			basePlane.name = "PlaneTemplate";
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			renderer.forceRenderingOff = false;
			man.Add("PlaneTemplate", basePlane);

			basePlane.gameObject.ConvertToPrefab(true);

			basePlane = basePlane.DuplicatePrefab();
			basePlane.GetComponent<MeshRenderer>().material = GenericExtensions.FindResourceObjectByName<Material>("TileBase_Alpha");
			man.Add("TransparentPlaneTemplate", basePlane);

			// Grass texture
			man.Add("Tex_Grass", GenericExtensions.FindResourceObjectByName<Texture2D>("Grass"));

			// Fence texture
			man.Add("Tex_Fence", GenericExtensions.FindResourceObjectByName<Texture2D>("fence"));

			// Setup Window hit audio
			WindowPatch.windowHitAudio = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "windowHit.wav")), "Vfx_WindowHit", SoundType.Effect, Color.white);

			// Setup wall clock poster
			man.Add("WallClock", ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("wall_clock.png")))]));

			// Principal's extra dialogues
			AddRule("breakingproperty", "principal_nopropertybreak.wav", "Vfx_PRI_NoPropertyBreak");
			AddRule("littering", "principal_noLittering.wav", "Vfx_PRI_NoLittering");
			AddRule("ugliness", "principal_nouglystun.wav", "Vfx_PRI_NoUglyStun");
			AddRule("stabbing", "principal_nostabbing.wav", "Vfx_PRI_NoStabbing");


			// Main Menu Stuff

			Sprite selectedSprite;
			SoundObject speechMenu;
			string jingle = AssetLoader.MidiFromMod("timeNewJingle", plug, "misc", "Audios", "newJingle.mid");

			if (Storage.IsChristmas)
			{
				selectedSprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("BBTChristmasV2.png"))), 1f);
				speechMenu = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_timesChristmas.wav")), "Vfx_BAL_BalMainMenuSpeech_Christmas_1", SoundType.Voice, Color.green);
				speechMenu.additionalKeys = [
					new() { key = "Vfx_BAL_BalMainMenuSpeech_1", time = 2.269f },
				new() { key = "Vfx_BAL_BalMainMenuSpeech_Christmas_2", time = 7.146f },
				];
			}
			// else if (plug.HasInfiniteFloors)
			// {
			// 	selectedSprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("endlessFloors.png"))), 1f);
			// 	speechMenu = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_InfFloorSpeech.wav")), "Vfx_BAL_BalMainMenuSpeech_1", SoundType.Voice, Color.green);
			// 	speechMenu.additionalKeys = [
			// 		new() { key = "Vfx_BAL_BalMainMenuSpeech_InfFloors_1", time = 5.144f },
			// 		];
			// }
			else
			{
				selectedSprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("mainMenu.png"))), 1f);
				speechMenu = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Speech.wav")), "Vfx_BAL_BalMainMenuSpeech_1", SoundType.Voice, Color.green);
				speechMenu.additionalKeys = [
					new() { key = "Vfx_BAL_BalMainMenuSpeech_2", time = 5.205f }
					];
			}

			MainMenuObject.CreateMenuObject("Men_TimesMainMenu_Name", selectedSprite, speechMenu, jingle);

			// Math Machine new Nums

			var machines = Resources.FindObjectsOfTypeAll<MathMachine>();

			var numList = machines[0].numberPres;
			var numPrefab = numList[0];
			var numTexs = TextureExtensions.LoadSpriteSheet(3, 3, 30f, BasePlugin.ModPath, "objects", "Math Machine", GetAssetName("numBalls.png"));

			List<MathMachineNumber> numbers = [];

			for (int i = 0; i < numTexs.Length; i++) // Fabricate them
			{
				var num = numPrefab.DuplicatePrefab();

				num.sprite.GetComponent<SpriteRenderer>().sprite = numTexs[i];
				num.value = i + 10;
				num.name = "NumBall_" + num.value;
				numbers.Add(num);
			}


			machines.Do((x) => x.numberPres = x.numberPres.AddRangeToArray([.. numbers]));

			// LITERALLY an empty object. Can be used for stuff like hiding those lightPre for example
			EmptyGameObject = new("NullObject");
			EmptyGameObject.ConvertToPrefab(false);

			// Gates for RUN
			MainGameManagerPatches.gateTextures = TextureExtensions.LoadTextureSheet(3, 1, MiscPath, TextureFolder, GetAssetName("RUN.png"));

			// Player Visual
			var playerSprites = TextureExtensions.LoadSpriteSheet(2, 1, 29.8f, MiscPath, TextureFolder, GetAssetName("player.png"));
			var playerVisual = ObjectCreationExtensions.CreateSpriteBillboard(playerSprites[0]).AddSpriteHolder(out var playerRenderer, -1.6f);

			var visualComp = playerVisual.gameObject.AddComponent<PlayerVisual>();
			visualComp.renderer = playerRenderer;
			visualComp.emotions = playerSprites;

			GameCameraPatch.playerVisual = visualComp;
			playerVisual.name = "PlayerVisual";
			playerRenderer.name = "PlayerVisualRenderer";
			playerVisual.gameObject.ConvertToPrefab(true);
			UnityEngine.Object.Destroy(playerVisual.GetComponent<RendererContainer>()); // Should make the game not cull the player's visual, if that happens

			// Global Assets
			man.Add("audExplosion", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("loudExplosion.wav"))), string.Empty, SoundType.Effect, Color.white));
			man.Get<SoundObject>("audExplosion").subtitle = false;

			man.Add("audRobloxDrink", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("potion_drink.wav"))), "Vfx_Roblox_drink", SoundType.Effect, Color.white));
			man.Add("audPencilStab", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("pc_stab.wav"))), "Vfx_PC_stab", SoundType.Effect, Color.yellow));
			man.Add("basketBall", TextureExtensions.LoadSpriteSheet(5, 1, 25f, GlobalAssetsPath, GetAssetName("basketball.png")));
			man.Add("Beartrap", TextureExtensions.LoadSpriteSheet(2, 1, 50f, GlobalAssetsPath, GetAssetName("trap.png")));
			man.Add("BeartrapCatch", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("trap_catch.wav"))), "Vfx_BT_catch", SoundType.Effect, Color.white));
			man.Add("audGenericPunch", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("punch.wav"))), "BB_Hit", SoundType.Effect, Color.white));
			man.Add("audGenericPunch_NoSub", man.Get<SoundObject>("audGenericPunch").GetNoSubCopy());
			man.Add("swingDoorPre", GenericExtensions.FindResourceObject<SwingDoorBuilder>().swingDoorPre);
			man.Add("audPop", GenericExtensions.FindResourceObjectByName<SoundObject>("Gen_Pop"));
			man.Add("audBuzz", GenericExtensions.FindResourceObjectByName<SoundObject>("Elv_Buzz"));
			man.Add("audRing", GenericExtensions.FindResourceObjectByName<SoundObject>("CashBell"));
			man.Add("audSlurp", GenericExtensions.FindResourceObjectByName<SoundObject>("WaterSlurp"));
			man.Add("outsideSkybox", Resources.FindObjectsOfTypeAll<Skybox>()[0]);
			man.Add("woodTexture", GenericExtensions.FindResourceObjectByName<Texture2D>("wood 1").MakeReadableTexture()); // Wood from the tables
			man.Add("plasticTexture", GenericExtensions.FindResourceObjectByName<Texture2D>("PlasticTable").MakeReadableTexture());
			man.Add("teleportAud", GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport"));
			man.Add("tierOnePickup", GenericExtensions.FindResourceObjectByName<SoundObject>("YTPPickup_0"));
			man.Add("tierTwoPickup", GenericExtensions.FindResourceObjectByName<SoundObject>("YTPPickup_1"));
			man.Add("tierThreePickup", GenericExtensions.FindResourceObjectByName<SoundObject>("YTPPickup_2"));
			man.Add("slipAud", GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Slip"));
			man.Add("whiteScreen", AssetLoader.SpriteFromTexture2D(TextureExtensions.CreateSolidTexture(480, 360, Color.white), 1f));
			man.Add("whitePix", AssetLoader.SpriteFromTexture2D(TextureExtensions.CreateSolidTexture(1, 1, Color.white), 1f));
			man.Add("fieldTripBucket", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("fieldTripBucket.png"))), 25f)); // PixelsPerUnit for world size
			man.Add("genericTextMesh", GenericExtensions.FindResourceObjectByName<TextMeshPro>("TotalDisplay")); // That text that advanced uses lol
			var sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("throw.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			man.Add("audGenericThrow", sd);

			sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("mildGrab.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			man.Add("audGenericGrab", sd);

			sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("audGenericStaminaYTPPickup.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			man.Add("audGenericStaminaYTPGrab", sd);

			sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("Snowhit.wav"))), "BB_Hit", SoundType.Effect, Color.white);
			man.Add("audGenericSnowHit", sd);

			// Eletricity Prefab
			Sprite[] anim = TextureExtensions.LoadSpriteSheet(2, 2, 25f, GlobalAssetsPath, GetAssetName("shock.png"));
			var eleRender = ObjectCreationExtensions.CreateSpriteBillboard(anim[0], false).AddSpriteHolder(out var eleRenderer, 0.1f, LayerStorage.ignoreRaycast);
			eleRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			eleRender.gameObject.ConvertToPrefab(true);
			eleRenderer.name = "Sprite";

			var ele = eleRender.gameObject.AddComponent<Eletricity>();
			ele.name = "Eletricity";
			var ani = ele.gameObject.AddComponent<AnimationComponent>();
			ani.animation = anim;
			ani.renderers = [eleRenderer];
			ani.speed = 15f;

			ele.ani = ani;

			sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("shock.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;

			ele.gameObject.CreatePropagatedAudioManager(10f, 40f).AddStartingAudiosToAudioManager(true, sd);

			ele.collider = ele.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * (LayerStorage.TileBaseOffset / 2), true);
			man.Add("EletricityPrefab", ele);

			// Slippery Water Prefab

			var watRender = ObjectCreationExtensions.CreateSpriteBillboard(null, false)
				.AddSpriteHolder(out var waterRenderer, 0.1f, LayerStorage.ignoreRaycast);

			watRender.name = "SlippingWater";
			watRender.gameObject.ConvertToPrefab(true);

			waterRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			waterRenderer.name = "SlippingWaterRender";

			var slipMatPre = watRender.gameObject.AddComponent<SlippingMaterial>();
			slipMatPre.audMan = slipMatPre.gameObject.CreatePropagatedAudioManager(10f, 40f);
			slipMatPre.audSlip = man.Get<SoundObject>("slipAud");
			slipMatPre.gameObject.AddBoxCollider(Vector3.up * 5f, new(9.9f, 10f, 9.9f), true);
			man.Add("SlipperyMatPrefab", slipMatPre);

			// Baldi Placeholder
			MainGameManagerPatches.bal_bangDoor = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_NormalEndingCutscene_Bang.wav")), string.Empty, SoundType.Effect, Color.white);
			MainGameManagerPatches.bal_explosionOutside = man.Get<SoundObject>("audExplosion");
			MainGameManagerPatches.cardboardBaldi = AssetLoader.SpriteFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("baldiCutOut.png")), Vector2.one * 0.5f, 34f);

			MainGameManagerPatches.placeholderBaldi = ObjectCreationExtensions.CreateSpriteBillboard(MainGameManagerPatches.cardboardBaldi); // As default
			MainGameManagerPatches.placeholderBaldi.gameObject.ConvertToPrefab(true);
			MainGameManagerPatches.placeholderBaldi.name = "PlaceholderBaldi";

			// Angry Baldi Placeholder
			MainGameManagerPatches.angryBaldiAnimation = TextureExtensions.LoadSpriteSheet(9, 8, 30f, MiscPath, TextureFolder, GetAssetName("baldiSuperAngry.png"));


			static void AddRule(string name, string audioName, string vfx) =>
				PrincipalPatches.ruleBreaks.Add(name, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "Principal", "Audios", GetAssetName(audioName))), vfx, SoundType.Voice, new(0, 0.1176f, 0.4824f)));
		}

		// Misc getters and methods
		internal static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc"); static string GlobalAssetsPath => Path.Combine(BasePlugin.ModPath, "GlobalAssets");

		internal static string GetAssetName(string name) => TimesAssetPrefix + name;

		internal const string
		AudioFolder = "Audios",
		TextureFolder = "Textures",
		TimesAssetPrefix = "BBTimesAsset_",
		F1 = "F1",
		F2 = "F2",
		F3 = "F3",
		F4 = "F4",
		F5 = "F5",
		END = "END";

		public static bool IsAFloorName(string str) =>
		str == F1 || str == F2 || str == F3 || str == F4 || str == F5 || str == END;

		internal const int MaximumNumballs = 18;

		public readonly static Dictionary<string, FloorData> floorDatas = new() {
			{ F1, new() },
			{ F2, new() },
			{ F3, new() },
			{ F4, new() },
			{ F5, new() },
			{ END, new() }
		};

		public readonly static AssetManager man = new();

		public static string CurrentFloor => Singleton<CoreGameManager>.Instance?.sceneObject.levelTitle ?? "None";

		public static GameObject EmptyGameObject;

		internal static List<Texture2D> specialRoomTextures = [];

		// All the npcs that are replacement marked will be added to this list

		// TODO: Incorporate the ReplacementNPC system into its own API (instead of being exclusive to this project)
		internal static List<INPCPrefab> replacementNpcs = [];

		internal static IEnumerable<Character> GetReplacementNPCs(params Character[] npcsReplaced) =>
			replacementNpcs.Where(x => npcsReplaced.Any(z => x.GetReplacementNPCs().Contains(z))).Select(x => x.Npc.Character);
	}
}
