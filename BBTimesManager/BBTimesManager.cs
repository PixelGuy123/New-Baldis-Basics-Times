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
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			Resources.FindObjectsOfTypeAll<MainGameManager>().Do(x => x.gameObject.AddComponent<MainGameManagerExtraComponent>()); // Adds extra component for every MainGameManager
		}

		static void SetAssets()
		{
			// Some materials
			ObjectCreationExtension.defaultMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "LockerTest" && x.GetInstanceID() > 0); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultDustMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "DustTest" && x.GetInstanceID() > 0); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultCubemap = Resources.FindObjectsOfTypeAll<Cubemap>().First(x => x.name == "Cubemap_DayStandard" && x.GetInstanceID() > 0);
			ObjectCreationExtension.mapMaterial = Resources.FindObjectsOfTypeAll<MapIcon>().First(x => x.name == "Icon_Prefab" && x.GetInstanceID() > 0).spriteRenderer.material;

			// Make a transparent texture
			ObjectCreationExtension.transparentTex = TextureExtensions.CreateSolidTexture(256, 256, Color.clear);

			// Make a black texture
			ObjectCreationExtension.blackTex = TextureExtensions.CreateSolidTexture(256, 256, Color.black);

			// Base plane for easy.. quads
			var basePlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
			var renderer = basePlane.GetComponent<MeshRenderer>();
			renderer.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "TileBase" && x.GetInstanceID() > 0);
			DontDestroyOnLoad(basePlane);
			basePlane.transform.localScale = Vector3.one * TileBaseOffset; // Gives the tile size
			basePlane.name = "PlaneTemplate";
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			man.Add("PlaneTemplate", basePlane);
			cacheToDisableAfterSetup.Add(basePlane);

			prefabs.Add(basePlane);

			// Grass texture
			man.Add("Tex_Grass", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Grass" && x.GetInstanceID() > 0));

			// Fence texture
			man.Add("Tex_Fence", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "fence" && x.GetInstanceID() > 0));

			// Sprite Billboard object
			var baseSprite = new GameObject("SpriteBillBoard").AddComponent<SpriteRenderer>();
			baseSprite.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "SpriteStandard_Billboard" && x.GetInstanceID() > 0);
			baseSprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			baseSprite.receiveShadows = false;

			baseSprite.gameObject.layer = LayerStorage.billboardLayer;
			DontDestroyOnLoad(baseSprite.gameObject);
			man.Add("SpriteBillboardTemplate", baseSprite.gameObject);
			cacheToDisableAfterSetup.Add(baseSprite.gameObject);

			prefabs.Add(baseSprite.gameObject);

			// Sprite Non-Billboard object
			baseSprite = new GameObject("SpriteNoBillBoard").AddComponent<SpriteRenderer>();
			baseSprite.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "SpriteStandard_NoBillboard" && x.GetInstanceID() > 0);
			baseSprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			baseSprite.receiveShadows = false;

			baseSprite.gameObject.layer = LayerStorage.billboardLayer;
			DontDestroyOnLoad(baseSprite.gameObject);
			man.Add("SpriteNoBillboardTemplate", baseSprite.gameObject);
			cacheToDisableAfterSetup.Add(baseSprite.gameObject);

			prefabs.Add(baseSprite.gameObject);

			// Setup Window hit audio

			WindowPatch.windowHitAudio = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "windowHit.wav")), "Vfx_WindowHit", SoundType.Voice, Color.white);


			// Fog music change
			var fogSound = AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "events", "Fog", "Audios", "new_CreepyOldComputer.wav"));
			var field = AccessTools.Field(typeof(FogEvent), "music");

			Resources.FindObjectsOfTypeAll<FogEvent>().Do(x => ((SoundObject)field.GetValue(x)).soundClip = fogSound); // Replace fog music

			// Principal's extra dialogues
			AddRule("breakingproperty", "principal_nopropertybreak.wav", "Vfx_PRI_NoPropertyBreak");
			AddRule("gumming", "principal_nospittinggums.wav", "Vfx_PRI_NoGumming");
			AddRule("littering", "principal_noLittering.wav", "Vfx_PRI_NoLittering");


			// Math Machine WOOOOW noises
			MathMachinePatches.aud_BalWow = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Wow.wav")), "Vfx_Bal_WOW", SoundType.Voice, Color.green);
			FieldTripManagerPatch.fieldTripYay = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "MUS_FieldTripWin.wav")), string.Empty, SoundType.Music, Color.white);
			FieldTripManagerPatch.fieldTripYay.subtitle = false; // Of course

			// Main Menu Stuff
			MainMenuPatch.mainMenu = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "mainMenu.png")), 1f);
			MainMenuPatch.aud_welcome = AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Speech.wav"));

			// Math Machine new Nums
			FieldInfo nums = AccessTools.Field(typeof(MathMachine), "numberPres");
			FieldInfo sprite = AccessTools.Field(typeof(MathMachineNumber), "sprite");
			FieldInfo value = AccessTools.Field(typeof(MathMachineNumber), "value"); // Setup fields
			var machines = Resources.FindObjectsOfTypeAll<MathMachine>();

			var numList = (MathMachineNumber[])nums.GetValue(machines[0]);
			var numPrefab = numList[0];
			var numTexs = Directory.GetFiles(Path.Combine(BasePlugin.ModPath, "objects", "Math Machine", "newNumBalls")); // Get all the new numballs

			List<MathMachineNumber> numbers = [];

			for (int i = 0; i < numTexs.Length; i++) // Fabricate them
			{
				var num = Instantiate(numPrefab);
				num.GetComponent<Entity>().SetActive(false);
				num.gameObject.layer = LayerStorage.iClickableLayer;
				((Transform)sprite.GetValue(num)).GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(numTexs[i]), 30f);
				value.SetValue(num, int.Parse(Path.GetFileNameWithoutExtension(numTexs[i]).Split('_')[1]));
				num.gameObject.SetActive(false);
				num.name = "NumBall_" + value.GetValue(num);
				DontDestroyOnLoad(num);
				numbers.Add(num);
			}

			machines.Do((x) => // Now just put them to every machine
			{
				var numList = (MathMachineNumber[])nums.GetValue(x);
				numList = numList.AddRangeToArray([.. numbers]);
				nums.SetValue(x, numList);
			});
			//F2
			floorDatas[1].MinNumberBallAmount = 9;
			floorDatas[1].MaxNumberBallAmount = 12;
			//F3
			floorDatas[2].MinNumberBallAmount = 12;
			floorDatas[2].MaxNumberBallAmount = MaximumNumballs;
			//END
			floorDatas[3].MinNumberBallAmount = 9;
			floorDatas[3].MaxNumberBallAmount = 14;

			// LITERALLY an empty object. Can be used for stuff like hiding those lightPre for example
			EmptyGameObject = new("NullObject");
			DontDestroyOnLoad(EmptyGameObject);

			// Gates for RUN
			MainGameManagerPatches.gateTextures[0] = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "GateR.png"));
			MainGameManagerPatches.gateTextures[1] = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "GateU.png"));
			MainGameManagerPatches.gateTextures[2] = AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "GateN.png")); // R U N

			// Player Visual
			var tex = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "player.png")), 225f);
			var playerVisual = ObjectCreationExtension.CreateSpriteBillboard(tex, -1.6f);
			DontDestroyOnLoad(playerVisual);

			GameCameraPatch.playerVisual = playerVisual.transform;

			prefabs.Add(playerVisual); // prefab too

			// Gotta sweep audio
			var aud = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "GottaSweep", "GS_Sweeping.wav")), "Vfx_GottaSweep", SoundType.Voice, new(0, 0.6226f, 0.0614f));
			Resources.FindObjectsOfTypeAll<GottaSweep>().Do((x) =>
			{
				var c = x.gameObject.AddComponent<GottaSweepComponent>();
				c.aud_sweep = aud;
			});


			// Local Methods
			static void AddRule(string name, string audioName, string vfx) =>
				PrincipalPatches.ruleBreaks.Add(name, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "Principal", "Audios", audioName)), vfx, SoundType.Voice, new(0, 0.1176f, 0.4824f)));
		} // 

		static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc");

		const string AudioFolder = "Audios", TextureFolder = "Textures", SfsFolder = "Sfs";

		internal const int MaximumNumballs = 18;

		public readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")];

		public readonly static AssetManager man = new();

		public static string CurrentFloor => Singleton<CoreGameManager>.Instance?.sceneObject.levelTitle ?? "None";

		public static FloorData CurrentFloorData => floorDatas.FirstOrDefault(x => x.Floor == CurrentFloor);

		internal const float TileBaseOffset = 10f;

		readonly internal static List<GameObject> prefabs = [];

		public static GameObject EmptyGameObject;

		static List<GameObject> cacheToDisableAfterSetup = [];



	}
	// Floor data
	internal class FloorData(string floor = "none")
	{
		public string Floor => _floor;
		readonly string _floor = floor;

		readonly List<WeightedNPC> _npcs = [];
		public List<WeightedNPC> NPCs => _npcs;

		readonly List<WeightedItemObject> _items = [];
		public List<WeightedItemObject> Items => _items;

		readonly List<WeightedItemObject> _shopitems = [];
		public List<WeightedItemObject> ShopItems => _shopitems;

		readonly List<WeightedItem> _fieldTripitems = [];
		public List<WeightedItem> FieldTripItems => _fieldTripitems;

		readonly List<WeightedRandomEvent> _events = [];
		public List<WeightedRandomEvent> Events => _events;

		readonly List<SchoolTextureHolder> _texs = [];
		public List<SchoolTextureHolder> SchoolTextures => _texs;


		// Object Builders

		readonly List<ObjectBuilder> _forcedObjBuilders = [];
		public List<ObjectBuilder> ForcedObjectBuilders => _forcedObjBuilders;

		readonly List<WeightedObjectBuilder> _weightedObjectBlders = [];
		public List<WeightedObjectBuilder> WeightedObjectBuilders => _weightedObjectBlders;

		readonly List<RandomHallBuilder> _hallBuilders = [];
		public List<RandomHallBuilder> HallBuilders => _hallBuilders;

		//readonly List<GenericHallBuilder> _genericHallBuilders = [];
		//public List<GenericHallBuilder> GenericHallBuilders => _genericHallBuilders;

		// Misc Fields
		readonly List<string> _midiFiles = [];
		public List<string> MidiFiles => _midiFiles;

		readonly List<WindowObjectHolder> _windowObjects = [];
		public List<WindowObjectHolder> WindowObjects => _windowObjects;

		public int MaxNumberBallAmount = 9; // Default
		public int MinNumberBallAmount = 9; // Default
	}
}
