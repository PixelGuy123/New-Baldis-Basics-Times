using BBTimes.CompatibilityModule;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches;
using BBTimes.ModPatches.NpcPatches;
using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager // basically holds the logic to create everything to the game
	{
		public static bool EditorExists => Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.leveleditor");
		internal static BasePlugin plug;
		internal static IEnumerator InitializeContentCreation()
		{
			yield return 14 + (EditorExists ? 1 : 0);

			yield return "Loading assets...";
			GameExtensions.TryRunMethod(SetAssets);
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
			CreateWindows();
			yield return "Creating custom rooms...";
			GameExtensions.TryRunMethod(CreateCustomRooms);
			yield return "Creating room functions...";
			GameExtensions.TryRunMethod(CreateRoomFunctions);
			yield return "Creating school textures...";
			GameExtensions.TryRunMethod(CreateSchoolTextures);
			yield return "Creating map icons...";
			GameExtensions.TryRunMethod(GetIcons);

			yield return "Calling GC Collect...";
			GC.Collect(); // Get any garbage I guess

			BasePlugin.PostSetup(man);

			yield break;
		}

		static void AddExtraComponentsForSomeObjects()
		{
			GenericExtensions.FindResourceObjects<MainGameManager>().Do(x => x.gameObject.AddComponent<MainGameManagerExtraComponent>()); // Adds extra component for every MainGameManager
			GenericExtensions.FindResourceObjects<EnvironmentController>().Do(x => x.gameObject.AddComponent<EnvironmentControllerData>());
			GenericExtensions.FindResourceObjects<PlayerManager>().Do(x => x.gameObject.AddComponent<PlayerAttributesComponent>());
			GenericExtensions.FindResourceObjects<CullingManager>().Do(x => x.gameObject.AddComponent<NullCullingManager>().cullMan = x);
		}

		static void SetAssets()
		{
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
			GameExtensions.detentionUiPre = GenericExtensions.FindResourceObject<DetentionUi>();
			man.Add("buttonPre", GenericExtensions.FindResourceObject<RotoHallBuilder>().buttonPre);
			man.AddFromResources<StandardDoorMats>();


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

			// Baldi Super Duper Rare Placeholder
			MainGameManagerPatches.placeholderBaldi = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("baldiCutOut.png")), Vector2.one * 0.5f, 32f)).gameObject;
			MainGameManagerPatches.placeholderBaldi.ConvertToPrefab(true);
			MainGameManagerPatches.placeholderBaldi.name = "PlaceholderBaldi";

			// Setup Window hit audio

			WindowPatch.windowHitAudio = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "windowHit.wav")), "Vfx_WindowHit", SoundType.Voice, Color.white);

			// Principal's extra dialogues
			AddRule("breakingproperty", "principal_nopropertybreak.wav", "Vfx_PRI_NoPropertyBreak");
			AddRule("littering", "principal_noLittering.wav", "Vfx_PRI_NoLittering");
			AddRule("ugliness", "principal_nouglystun.wav", "Vfx_PRI_NoUglyStun");
			AddRule("stabbing", "principal_nostabbing.wav", "Vfx_PRI_NoStabbing");


			// Main Menu Stuff
			MainMenuPatch.mainMenu = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("mainMenu.png"))), 1f);
			var mainSpeech = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Speech.wav")), "Vfx_BAL_BalMainMenuSpeech_1", SoundType.Effect, Color.green);
			mainSpeech.additionalKeys = [
				new() { key = "Vfx_BAL_BalMainMenuSpeech_2", time = 5.56f },
				new() { key = "Vfx_BAL_BalMainMenuSpeech_3", time = 11.718f }
				];
			MainMenuPatch.aud_welcome = mainSpeech;

			MainMenuPatch.mainMenuEndless = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("endlessFloors.png"))), 1f);
			mainSpeech = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_InfFloorSpeech.wav")), "Vfx_BAL_BalMainMenuSpeech_1", SoundType.Effect, Color.green);
			mainSpeech.additionalKeys = [
				new() { key = "Vfx_BAL_BalMainMenuSpeech_InfFloors_1", time = 5.961f },
				new() { key = "Vfx_BAL_BalMainMenuSpeech_InfFloors_2", time = 9.988f },
				new() { key = "Vfx_BAL_BalMainMenuSpeech_InfFloors_3", time = 13.014f },
				new() { key = "Vfx_BAL_BalMainMenuSpeech_InfFloors_4", time = 18.108f }
				];
			MainMenuPatch.aud_welcome_endless = mainSpeech;

			if (File.Exists(Path.Combine(MiscPath, AudioFolder, "BAL_VeryDifferentSpeechForFun.wav")))
			{
				mainSpeech = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_VeryDifferentSpeechForFun.wav")), "Vfx_BAL_SecretSpecificForUserSpeech1", SoundType.Effect, Color.green);
				mainSpeech.encrypted = true;
				mainSpeech.additionalKeys = [
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech2", time = 1.093f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech3", time = 3.576f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech4", time = 12.087f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech5", time = 15.651f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech6", time = 20.931f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech7", time = 28.62f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech8", time = 32.523f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech9", time = 36.192f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech10", time = 40.598f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech11", time = 43.181f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech12", time = 44.377f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech13", time = 46.727f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech14", time = 50.45f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech15", time = 52.595f, encrypted = true },
					new() { key = "Vfx_BAL_SecretSpecificForUserSpeech16", time = 54.739f, encrypted = true }
					];
				MainMenuPatch.aud_superSecretOnlyReservedForThoseIselect = mainSpeech;
			}

			// Math Machine new Nums

			var machines = GenericExtensions.FindResourceObjects<MathMachine>();

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

			//F2
			floorDatas[1].MinNumberBallAmount = 9;
			floorDatas[1].MaxNumberBallAmount = 12;
			floorDatas[1].LockdownDoorSpeedOffset = 3;
			//F3
			floorDatas[2].MinNumberBallAmount = 12;
			floorDatas[2].MaxNumberBallAmount = MaximumNumballs;
			//floorDatas[2].ConveyorSpeedOffset = 4;
			floorDatas[2].LockdownDoorSpeedOffset = 4;
			//END
			floorDatas[3].MinNumberBallAmount = 9;
			floorDatas[3].MaxNumberBallAmount = 14;
			//floorDatas[3].ConveyorSpeedOffset = 3;
			floorDatas[3].LockdownDoorSpeedOffset = 2;

			// LITERALLY an empty object. Can be used for stuff like hiding those lightPre for example
			EmptyGameObject = new("NullObject");
			EmptyGameObject.ConvertToPrefab(false);

			// Gates for RUN
			MainGameManagerPatches.gateTextures = TextureExtensions.LoadTextureSheet(3, 1, MiscPath, TextureFolder, GetAssetName("RUN.png"));

			// Player Visual
			var tex = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("player.png"))), 225f);
			var playerVisual = ObjectCreationExtensions.CreateSpriteBillboard(tex).AddSpriteHolder(-1.6f);
			playerVisual.gameObject.AddComponent<PlayerVisual>();

			GameCameraPatch.playerVisual = playerVisual.transform.parent;
			playerVisual.transform.parent.gameObject.ConvertToPrefab(true);

			// Global Assets
			man.Add("audRobloxDrink", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("potion_drink.wav"))), "Vfx_Roblox_drink", SoundType.Effect, Color.white));
			man.Add("audPencilStab", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("pc_stab.wav"))), "Vfx_PC_stab", SoundType.Voice, Color.yellow));
			man.Add("basketBall", TextureExtensions.LoadSpriteSheet(5, 1, 25f, GlobalAssetsPath, GetAssetName("basketball.png")));
			man.Add("Beartrap", TextureExtensions.LoadSpriteSheet(2, 1, 50f, GlobalAssetsPath, GetAssetName("trap.png")));
			man.Add("BeartrapCatch", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("trap_catch.wav"))), "Vfx_BT_catch", SoundType.Voice, Color.white));
			man.Add("audGenericPunch", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("punch.wav"))), "BB_Hit", SoundType.Voice, Color.white));
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
			var sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("throw.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			man.Add("audGenericThrow", sd);

			sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("mildGrab.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;
			man.Add("audGenericGrab", sd);

			// Eletricity Prefab
			Sprite[] anim = TextureExtensions.LoadSpriteSheet(2, 2, 25f, GlobalAssetsPath, GetAssetName("shock.png"));
			var eleRender = ObjectCreationExtensions.CreateSpriteBillboard(anim[0], false).AddSpriteHolder(0.1f, LayerStorage.ignoreRaycast);
			eleRender.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			eleRender.transform.parent.gameObject.ConvertToPrefab(true);
			eleRender.name = "Sprite";

			var ele = eleRender.transform.parent.gameObject.AddComponent<Eletricity>();
			ele.name = "Eletricity";
			var ani = ele.gameObject.AddComponent<AnimationComponent>();
			ani.animation = anim;
			ani.renderers = [eleRender];
			ani.speed = 15f;

			ele.ani = ani;

			sd = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, GetAssetName("shock.wav"))), string.Empty, SoundType.Effect, Color.white);
			sd.subtitle = false;

			ele.gameObject.CreatePropagatedAudioManager(10f, 30f).AddStartingAudiosToAudioManager(true, sd);

			ele.collider = ele.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * (LayerStorage.TileBaseOffset / 2), true);
			man.Add("EletricityPrefab", ele);

			ele = UnityEngine.Object.Instantiate(ele);
			ele.gameObject.ConvertToPrefab(true);
			ele.collider.size = new(ele.collider.size.x, 1.5f, ele.collider.size.z);

			eleRender = ObjectCreationExtensions.CreateSpriteBillboard(anim[0], false);
			eleRender.transform.SetParent(ele.transform);
			eleRender.transform.localPosition = new(0f, -0.1f, 0f);
			eleRender.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			eleRender.name = "SpriteBackwards";

			ele.ani.renderers = ele.GetComponentsInChildren<SpriteRenderer>();
			ele.name = "DoorEletricity";
			man.Add("DoorEletricityPrefab", ele);

			// Slippery Water Prefab

			var watRender = ObjectCreationExtensions.CreateSpriteBillboard(null, false).AddSpriteHolder(0.1f, 0);
			watRender.transform.parent.name = "SlippingWater";
			watRender.transform.parent.gameObject.ConvertToPrefab(true);
			watRender.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			watRender.name = "SlippingWaterRender";
			var slipMatPre = watRender.transform.parent.gameObject.AddComponent<SlippingMaterial>();
			slipMatPre.audMan = slipMatPre.gameObject.CreatePropagatedAudioManager(45f, 60f);
			slipMatPre.audSlip = man.Get<SoundObject>("slipAud");
			slipMatPre.gameObject.AddBoxCollider(Vector3.zero, new(9.9f, 10f, 9.9f), true);
			man.Add("SlipperyMatPrefab", slipMatPre);

			static void AddRule(string name, string audioName, string vfx) =>
				PrincipalPatches.ruleBreaks.Add(name, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "Principal", "Audios", GetAssetName(audioName))), vfx, SoundType.Voice, new(0, 0.1176f, 0.4824f)));
		}

		internal static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc"); static string GlobalAssetsPath => Path.Combine(BasePlugin.ModPath, "GlobalAssets");

		internal static string GetAssetName(string name) => TimesAssetPrefix + name;

		internal const string TimesAssetPrefix = "BBTimesAsset_";

		internal const string AudioFolder = "Audios", TextureFolder = "Textures";

		internal const int MaximumNumballs = 18;

		public readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")]; // floor datas

		public readonly static AssetManager man = new();

		public static string CurrentFloor => Singleton<CoreGameManager>.Instance?.sceneObject.levelTitle ?? "None";

		public static FloorData CurrentFloorData { get
			{
				var data = floorDatas.FirstOrDefault(x => x.Floor == CurrentFloor);
				if (data != null || !Singleton<CoreGameManager>.Instance)
					return data;

				if (Singleton<CoreGameManager>.Instance.sceneObject.levelNo >= 35) // If Infinite Floors. This levelNo should be like this
					return floorDatas[2];

				if (Singleton<CoreGameManager>.Instance.sceneObject.levelNo >= 15)
					return floorDatas[1];

				return floorDatas[0];
			}
		}

		public static GameObject EmptyGameObject;

		internal static List<Texture2D> specialRoomTextures = [];

		// All the npcs that are replacement marked will be added to this list
		internal static List<INPCPrefab> replacementNpcs = [];

		internal static IEnumerable<Character> GetReplacementNPCs(params Character[] npcsReplaced) =>
			replacementNpcs.Where(x => npcsReplaced.Any(z => x.GetReplacementNPCs().Contains(z))).Select(x => x.Npc.Character);



	}
	// Floor data
	internal class FloorData(string floor = "none")
	{
		public string Floor => _floor;
		readonly string _floor = floor;

		public readonly List<WeightedNPC> NPCs = [];
		public readonly List<WeightedItemObject> Items = [];
		public readonly List<ItemObject> ForcedItems = [];
		public readonly List<WeightedItemObject> ShopItems = [];
		public readonly List<WeightedItemObject> FieldTripItems = [];
		public readonly List<WeightedRandomEvent> Events = [];
		public readonly List<SchoolTextureHolder> SchoolTextures = [];

		// Rooms
		public readonly List<RoomGroup> RoomAssets = [];
		public readonly List<WeightedRoomAsset> SpecialRooms = [];
		public readonly List<WeightedRoomAsset> Classrooms = [];
		public readonly List<WeightedRoomAsset> Faculties = [];
		public readonly List<WeightedRoomAsset> Offices = [];
		public readonly Dictionary<WeightedRoomAsset, bool> Halls = [];


		// Object Builders
		public readonly List<ObjectBuilder> ForcedObjectBuilders = [];
		public readonly List<WeightedObjectBuilder> WeightedObjectBuilders = [];
		public readonly List<RandomHallBuilder> HallBuilders = [];

		//readonly List<GenericHallBuilder> _genericHallBuilders = [];
		//public List<GenericHallBuilder> GenericHallBuilders => _genericHallBuilders;

		// Misc Fields
		public readonly List<WindowObjectHolder> WindowObjects = [];

		public int MaxNumberBallAmount = 9; // Default
		public int MinNumberBallAmount = 9; // Default

	//	public int ConveyorSpeedOffset = 2;
		public int LockdownDoorSpeedOffset = 2;
	}
}
