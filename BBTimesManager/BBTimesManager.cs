using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using BepInEx;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using MTM101BaldAPI;
using BBTimes.CustomComponents;
using HarmonyLib;
using BBTimes.Manager.SelectionHolders;
using BBTimes.ModPatches;
using BBTimes.ModPatches.NpcPatches;
using System.Reflection;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager // basically holds the logic to create everything to the game
    {

        internal static void InitializeContentCreation(BaseUnityPlugin plug)
        {
			try
			{
				AddExtraComponentsForSomeObjects();
				SetAssets();
				CreateSpriteBillboards();
				CreateCubeMaps();
				GetMusics();
				CreateNPCs(plug);
				CreateItems(plug);
				CreateEvents(plug);
				CreateObjBuilders(plug);
				CreateWindows();
				CreateRoomFunctions();
			}
			catch(System.Exception e)
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
			ObjectCreationExtension.defaultMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "LockerTest"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultDustMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "DustTest"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultCubemap = Resources.FindObjectsOfTypeAll<Cubemap>().First(x => x.name == "Cubemap_DayStandard");
			
			// Make a transparent texture
			var tex = new Texture2D(256, 256);
			Color[] c = new Color[tex.width * tex.height];
			for (int i = 0; i < c.Length; i++)
				c[i] = new(1f, 1f, 1f, 0f);
			tex.SetPixels(c);
			tex.Apply();
			ObjectCreationExtension.transparentTex = tex;

			// Make a black texture
			tex = new Texture2D(256, 256);
			c = new Color[tex.width * tex.height];
			for (int i = 0; i < c.Length; i++)
				c[i] = new(0f, 0f, 0f, 1f);
			tex.SetPixels(c);
			tex.Apply();
			ObjectCreationExtension.blackTex = tex;

			// Base plane for easy.. quads
			var basePlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
			var renderer = basePlane.GetComponent<MeshRenderer>();
			renderer.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "TileBase");
			Object.DontDestroyOnLoad(basePlane);
			basePlane.transform.localScale = Vector3.one * 10f; // Gives the tile size
			basePlane.name = "PlaneTemplate";
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			man.Add("PlaneTemplate", basePlane);

			prefabs.Add(basePlane);

			// Grass texture
			man.Add("Tex_Grass", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Grass"));

			// Fence texture
			man.Add("Tex_Fence", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "fence"));

			// Sprite Billboard object
			var baseSprite = new GameObject("SpriteBillBoard").AddComponent<SpriteRenderer>();
			baseSprite.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "SpriteStandard_Billboard");
			baseSprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			baseSprite.receiveShadows = false;			

			baseSprite.gameObject.layer = billboardLayer;
			Object.DontDestroyOnLoad(baseSprite.gameObject);
			man.Add("SpriteBillboardTemplate", baseSprite.gameObject);

			prefabs.Add(baseSprite.gameObject);

			// Sprite Non-Billboard object

			// Sprite Billboard object
			baseSprite = new GameObject("SpriteNoBillBoard").AddComponent<SpriteRenderer>();
			baseSprite.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "SpriteStandard_NoBillboard");
			baseSprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			baseSprite.receiveShadows = false;

			baseSprite.gameObject.layer = billboardLayer;
			Object.DontDestroyOnLoad(baseSprite.gameObject);
			man.Add("SpriteNoBillboardTemplate", baseSprite.gameObject);

			prefabs.Add(baseSprite.gameObject);

			// Setup Window hit audio

			WindowPatch.windowHitAudio = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "windowHit.wav")), "Vfx_WindowHit", SoundType.Voice, Color.white);

			// Setup Gum animation
			var gumHolder = new GameObject("gumSplash");
			var gum = Object.Instantiate(man.Get<GameObject>("SpriteNoBillboardTemplate")); // front of the gum
			gum.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "gumSplash.png"))
				, 25f);
			gum.layer = billboardLayer;
			gum.transform.SetParent(gumHolder.transform);
			gum.transform.localPosition = Vector3.zero;

			gum = Object.Instantiate(man.Get<GameObject>("SpriteNoBillboardTemplate")); // Back of the gum
			gum.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "gumSplash_back.png"))
				, 25f);
			gum.transform.SetParent(gumHolder.transform);
			gum.transform.localPosition = Vector3.zero + gum.transform.forward * -0.01f;
			gumHolder.SetActive(false);
			Object.DontDestroyOnLoad(gumHolder);

			gumHolder.AddComponent<EmptyMonoBehaviour>(); // For coroutines
			GumSplash.gumSplash = gumHolder.transform;

			GumSplash.splash = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "gumSplash.wav")), "Vfx_GumSplash", SoundType.Voice, new(0.99609f, 0, 0.99609f));

			prefabs.Add(gumHolder);


			// Fog music change
			var fogSound = AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "events", "Fog", "Audios", "new_CreepyOldComputer.wav"));
			var field = AccessTools.Field(typeof(FogEvent), "music");

			Resources.FindObjectsOfTypeAll<FogEvent>().Do(x => ((SoundObject)field.GetValue(x)).soundClip = fogSound); // Replace fog music

			// Principal's extra dialogues
			AddRule("breakingproperty", "principal_nopropertybreak.wav", "Vfx_PRI_NoPropertyBreak");
			AddRule("gumming", "principal_nospittinggums.wav", "Vfx_PRI_NoGumming");


			// Math Machine WOOOOW noises
			MathMachinePatches.aud_BalWow = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Wow.wav")), "Vfx_Bal_WOW", SoundType.Voice, Color.green);
			FieldTripManagerPatch.fieldTripYay = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "MUS_FieldTripWin.wav")), string.Empty, SoundType.Music, Color.white);
			FieldTripManagerPatch.fieldTripYay.subtitle = false; // Of course

			// Cloudy Copter PAH
			CloudyCopterPatch.aud_PAH = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "Cloudy Copter", "CC_PAH.wav")), "Vfx_Cumulo_PAH", SoundType.Voice, Color.white);
			FieldInfo minDistance = AccessTools.Field(typeof(PropagatedAudioManager), "minDistance");
			FieldInfo maxDistance = AccessTools.Field(typeof(PropagatedAudioManager), "maxDistance");

			Resources.FindObjectsOfTypeAll<Cumulo>().Do((x) =>
			{
				var audman = x.gameObject.AddComponent<PropagatedAudioManager>();
				minDistance.SetValue(audman, 15f);
				maxDistance.SetValue(audman, 45f);
			});

			// Main Menu Stuff
			MainMenuPatch.mainMenu = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "mainMenu.png")), 1f);
			MainMenuPatch.aud_welcome = AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Speech.wav"));

			// Hanging ceiling light for cafeteria
			var hangingLightHolder = new GameObject("HugeHangingCeilingLight");
			var hangingLight = Object.Instantiate(man.Get<GameObject>("SpriteBillboardTemplate"), hangingLightHolder.transform);

			hangingLight.transform.localPosition = Vector3.up * 40f;
			hangingLight.transform.localScale = Vector3.one * 1.4f;
			hangingLight.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cafeHangingLight.png")), 25f);

			hangingLightHolder.SetActive(false);
			Object.DontDestroyOnLoad(hangingLightHolder);



			prefabs.Add(hangingLightHolder);
			man.Add("prefab_cafeHangingLight", hangingLightHolder);



			// Local Methods
			static void AddRule(string name, string audioName, string vfx) =>
				PrincipalPatches.ruleBreaks.Add(name, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "npcs", "Principal", "Audios", audioName)), vfx, SoundType.Voice, new(0, 0.1176f, 0.4824f)));
		} // 

		static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc");

		const string AudioFolder = "Audios", TextureFolder = "Textures", SfsFolder = "Sfs";

        public readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")];

		public readonly static AssetManager man = new();

		public static string CurrentFloor => Singleton<CoreGameManager>.Instance.sceneObject.levelTitle ?? "None";

		public static FloorData CurrentFloorData => floorDatas.FirstOrDefault(x => x.Floor == CurrentFloor);

		internal const float TileBaseOffset = 10f;

		readonly internal static List<GameObject> prefabs = [];

		

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
	}
}
