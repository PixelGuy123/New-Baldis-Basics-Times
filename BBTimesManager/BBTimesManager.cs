using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Misc.SelectionHolders;
using BBTimes.ModPatches;
using BBTimes.ModPatches.NpcPatches;
using BBTimes.Plugin;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reflection;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager // basically holds the logic to create everything to the game
	{
		
		internal static void InitializeContentCreation(BaseUnityPlugin plug)
		{
			try
			{
				SetAssets();
				AddExtraComponentsForSomeObjects();
				CreateSpriteBillboards();
				CreateCubeMaps();
				GetMusics();
				CreateNPCs(plug);
				CreateItems(plug);
				CreateEvents(plug);
				CreateObjBuilders(plug);
				CreateWindows();
				CreateRoomFunctions();
				CreateSchoolTextures();
				GetIcons();

				for (int i = 0; i < cacheToDisableAfterSetup.Count; i++)
				{
					cacheToDisableAfterSetup[i].SetActive(false);
					cacheToDisableAfterSetup.RemoveAt(i);
					i--;
				}
				cacheToDisableAfterSetup = null;
				GC.Collect(); // Get any garbage I guess
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				MTM101BaldiDevAPI.CauseCrash(plug.Info, e); // just in case
			}
		}

		static void AddExtraComponentsForSomeObjects()
		{
			GenericExtensions.FindResourceObjects<MainGameManager>().Do(x => x.gameObject.AddComponent<MainGameManagerExtraComponent>()); // Adds extra component for every MainGameManager
			GenericExtensions.FindResourceObjects<EnvironmentController>().Do(x => x.gameObject.AddComponent<EnvironmentControllerData>());
		}

		static void SetAssets()
		{
			// Some materials
			ObjectCreationExtension.defaultMaterial = GenericExtensions.FindResourceObjectByName<Material>("LockerTest"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultDustMaterial = GenericExtensions.FindResourceObjectByName<Material>("DustTest"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultCubemap = GenericExtensions.FindResourceObjectByName<Cubemap>("Cubemap_DayStandard");
			ObjectCreationExtension.mapMaterial = GenericExtensions.FindResourceObjectByName<MapIcon>("Icon_Prefab").spriteRenderer.material;

			// Make a transparent texture
			ObjectCreationExtension.transparentTex = TextureExtensions.CreateSolidTexture(256, 256, Color.clear);

			// Make a black texture
			ObjectCreationExtension.blackTex = TextureExtensions.CreateSolidTexture(256, 256, Color.black);

			// Base plane for easy.. quads
			var basePlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
			var renderer = basePlane.GetComponent<MeshRenderer>();
			renderer.material = GenericExtensions.FindResourceObjectByName<Material>("TileBase");
			basePlane.transform.localScale = Vector3.one * LayerStorage.TileBaseOffset; // Gives the tile size
			basePlane.name = "PlaneTemplate";
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			man.Add("PlaneTemplate", basePlane);
			cacheToDisableAfterSetup.Add(basePlane);

			basePlane.gameObject.SetAsPrefab().AddAsGeneratorPrefab();

			// Grass texture
			man.Add("Tex_Grass", GenericExtensions.FindResourceObjectByName<Texture2D>("Grass"));

			// Fence texture
			man.Add("Tex_Fence", GenericExtensions.FindResourceObjectByName<Texture2D>("fence"));

			// Canvas renderer instance
			man.Add("CanvasPrefab", GenericExtensions.FindResourceObjectByName<Canvas>("GumOverlay"));

			// Setup Window hit audio

			WindowPatch.windowHitAudio = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "windowHit.wav")), "Vfx_WindowHit", SoundType.Voice, Color.white);


			// Fog music change
			var fogSound = AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "events", "Fog", "Audios", "new_CreepyOldComputer.wav"));
			var field = AccessTools.Field(typeof(FogEvent), "music");

			GenericExtensions.FindResourceObjects<FogEvent>().Do(x => ((SoundObject)field.GetValue(x)).soundClip = fogSound); // Replace fog music

			// Principal's extra dialogues
			AddRule("breakingproperty", "principal_nopropertybreak.wav", "Vfx_PRI_NoPropertyBreak");
			AddRule("gumming", "principal_nospittinggums.wav", "Vfx_PRI_NoGumming");
			AddRule("littering", "principal_noLittering.wav", "Vfx_PRI_NoLittering");
			AddRule("ugliness", "principal_nouglystun.wav", "Vfx_PRI_NoUglyStun");
			AddRule("stabbing", "principal_nostabbing.wav", "Vfx_PRI_NoStabbing");


			// Math Machine WOOOOW noises
			MathMachinePatches.aud_BalWow = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Wow.wav")), "Vfx_Bal_WOW", SoundType.Voice, Color.green);
			FieldTripManagerPatch.fieldTripYay = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "MUS_FieldTripWin.wav")), string.Empty, SoundType.Music, Color.white);
			FieldTripManagerPatch.fieldTripYay.subtitle = false; // Of course

			// Main Menu Stuff
			MainMenuPatch.mainMenu = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "mainMenu.png")), 1f);
			MainMenuPatch.aud_welcome = AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Speech.wav"));

			// Math Machine new Nums
			//FieldInfo nums = AccessTools.Field(typeof(MathMachine), "numberPres");
			//FieldInfo sprite = AccessTools.Field(typeof(MathMachineNumber), "sprite");
			//FieldInfo value = AccessTools.Field(typeof(MathMachineNumber), "value"); // Setup fields
			var machines = GenericExtensions.FindResourceObjects<MathMachine>();

			var numList = machines[0].numberPres; //(MathMachineNumber[])nums.GetValue(machines[0]);
			var numPrefab = numList[0];
			var numTexs = Directory.GetFiles(Path.Combine(BasePlugin.ModPath, "objects", "Math Machine", "newNumBalls")); // Get all the new numballs

			List<MathMachineNumber> numbers = [];

			for (int i = 0; i < numTexs.Length; i++) // Fabricate them
			{
				var num = Instantiate(numPrefab);
				num.GetComponent<Entity>().SetActive(false);
				num.gameObject.layer = LayerStorage.iClickableLayer;
				num.sprite.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(numTexs[i]), 30f); // ((Transform)sprite.GetValue(num)).GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(numTexs[i]), 30f);
				num.value = int.Parse(Path.GetFileNameWithoutExtension(numTexs[i]).Split('_')[1]); //value.SetValue(num, int.Parse(Path.GetFileNameWithoutExtension(numTexs[i]).Split('_')[1]));
				num.gameObject.SetActive(false);
				num.name = "NumBall_" + num.value; // value.GetValue(num);
				DontDestroyOnLoad(num);
				numbers.Add(num);
			}

			//machines.Do((x) => // Now just put them to every machine
			//{
			//	var numList = (MathMachineNumber[])nums.GetValue(x);
			//	numList = numList.AddRangeToArray([.. numbers]);
			//	nums.SetValue(x, numList);
			//});

			machines.Do((x) => x.numberPres = x.numberPres.AddRangeToArray([.. numbers]));

			//F2
			floorDatas[1].MinNumberBallAmount = 9;
			floorDatas[1].MaxNumberBallAmount = 12;
			floorDatas[1].LockdownDoorSpeedOffset = 3;
			//F3
			floorDatas[2].MinNumberBallAmount = 12;
			floorDatas[2].MaxNumberBallAmount = MaximumNumballs;
			floorDatas[2].ConveyorSpeedOffset = 4;
			floorDatas[2].LockdownDoorSpeedOffset = 4;
			//END
			floorDatas[3].MinNumberBallAmount = 9;
			floorDatas[3].MaxNumberBallAmount = 14;
			floorDatas[3].ConveyorSpeedOffset = 3;
			floorDatas[3].LockdownDoorSpeedOffset = 2;

			// LITERALLY an empty object. Can be used for stuff like hiding those lightPre for example
			EmptyGameObject = new("NullObject");
			DontDestroyOnLoad(EmptyGameObject);

			// Gates for RUN
			MainGameManagerPatches.gateTextures[0] = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "GateR.png"));
			MainGameManagerPatches.gateTextures[1] = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "GateU.png"));
			MainGameManagerPatches.gateTextures[2] = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "GateN.png")); // R U N

			// Player Visual
			var tex = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "player.png")), 225f);
			var playerVisual = ObjectCreationExtensions.CreateSpriteBillboard(tex).AddSpriteHolder(-1.6f);

			GameCameraPatch.playerVisual = playerVisual.transform.parent;
			playerVisual.transform.parent.gameObject.SetAsPrefab().AddAsGeneratorPrefab();

			// Global Assets
			man.Add("audRobloxDrink", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, "potion_drink.wav")), "Vfx_Roblox_drink", SoundType.Effect, Color.white));
			man.Add("audPencilStab", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(GlobalAssetsPath, "pc_stab.wav")), "Vfx_PC_stab", SoundType.Voice, Color.yellow));


			// Local Methods
			static void AddRule(string name, string audioName, string vfx) =>
				PrincipalPatches.ruleBreaks.Add(name, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "Principal", "Audios", audioName)), vfx, SoundType.Voice, new(0, 0.1176f, 0.4824f)));
		} // 

		static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc"); static string GlobalAssetsPath => Path.Combine(BasePlugin.ModPath, "GlobalAssets");

		const string AudioFolder = "Audios", TextureFolder = "Textures", SfsFolder = "Sfs";

		internal const int MaximumNumballs = 18;

		public readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")]; // floor datas

		public readonly static AssetManager man = new();

		public static string CurrentFloor => Singleton<CoreGameManager>.Instance?.sceneObject.levelTitle ?? "None";

		public static FloorData CurrentFloorData => floorDatas.FirstOrDefault(x => x.Floor == CurrentFloor);

		public static GameObject EmptyGameObject;

		static List<GameObject> cacheToDisableAfterSetup = [];



	}
	// Floor data
	internal class FloorData(string floor = "none")
	{
		public string Floor => _floor;
		readonly string _floor = floor;

		public readonly List<WeightedNPC> NPCs = [];
		public readonly List<WeightedItemObject> Items = [];
		public readonly List<WeightedItemObject> ShopItems = [];
		public readonly List<WeightedItem> FieldTripItems = [];
		public readonly List<WeightedRandomEvent> Events = [];
		public readonly List<SchoolTextureHolder> SchoolTextures = [];


		// Object Builders
		public readonly List<ObjectBuilder> ForcedObjectBuilders = [];
		public readonly List<WeightedObjectBuilder> WeightedObjectBuilders = [];
		public readonly List<RandomHallBuilder> HallBuilders = [];

		//readonly List<GenericHallBuilder> _genericHallBuilders = [];
		//public List<GenericHallBuilder> GenericHallBuilders => _genericHallBuilders;

		// Misc Fields
		public readonly List<string> MidiFiles = [];
		public readonly List<WindowObjectHolder> WindowObjects = [];

		public int MaxNumberBallAmount = 9; // Default
		public int MinNumberBallAmount = 9; // Default

		public int ConveyorSpeedOffset = 2;
		public int LockdownDoorSpeedOffset = 2;
	}
}
