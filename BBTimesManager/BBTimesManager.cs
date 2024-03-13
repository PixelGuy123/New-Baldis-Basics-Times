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

			// Base plane for easy.. planes
			var basePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			var renderer = basePlane.GetComponent<MeshRenderer>();
			renderer.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "TileBase");
			Object.DontDestroyOnLoad(basePlane);
			basePlane.SetActive(false);
			basePlane.name = "PlaneTemplate";
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			man.Add("PlaneTemplate", basePlane);

			// Grass texture
			man.Add("Tex_Grass", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Grass"));

			// Fence texture
			man.Add("Tex_Fence", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "fence"));

			// Sprite Billboard object
			var baseSprite = new GameObject("SpriteBillBoard").AddComponent<SpriteRenderer>();
			baseSprite.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "SpriteStandard_Billboard");
			baseSprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			baseSprite.receiveShadows = false;

			baseSprite.gameObject.SetActive(false);
			Object.DontDestroyOnLoad(baseSprite.gameObject);
			man.Add("SpriteBillboardTemplate", baseSprite.gameObject);

			// Setup Window hit audio

			WindowPatch.windowHitAudio = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "windowHit.wav")), "Vfx_WindowHit", SoundType.Voice, Color.white);
		}

		static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc");

		const string AudioFolder = "Audios", TextureFolder = "Textures", SfsFolder = "Sfs";

        public readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")];

		public readonly static AssetManager man = new();

		public static string CurrentFloor => Singleton<CoreGameManager>.Instance.sceneObject.levelTitle ?? "None";

		public static FloorData CurrentFloorData => floorDatas.FirstOrDefault(x => x.Floor == CurrentFloor);

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
