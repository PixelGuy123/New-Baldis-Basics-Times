using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using BepInEx;
using MTM101BaldAPI.AssetTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using MTM101BaldAPI;
using HarmonyLib;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager // basically holds the logic to create everything to the game
    {
        internal static void InitializeContentCreation(BaseUnityPlugin plug)
        {
			try
			{
				LoadAssets();
				SetMaterials();
				CreateNPCs(plug);
				CreateItems(plug);
				CreateEvents(plug);
				CreateObjBuilders(plug);
			}
			catch(System.Exception e)
			{
				MTM101BaldiDevAPI.CauseCrash(plug.Info, e); // just in case
			}
        }

		static void SetMaterials()
		{
			ObjectCreationExtension.defaultMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "LockerTest"); // Actually a good material, has even lightmap
			ObjectCreationExtension.defaultDustMaterial = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "DustTest"); // Actually a good material, has even lightmap
		}

		static void LoadAssets()
		{
			var fieldInfo = AccessTools.Field(typeof(MainGameManager), "allNotebooksNotification");
			var sound = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AllNotebooksNormal.wav")), "Vfx_BAL_CongratsNormal_0", SoundType.Effect, Color.green);
			sound.additionalKeys = [
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_1", time = 2.103f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_2", time = 4.899f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_3", time = 8.174f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_4", time = 12.817f} // Tip: use audacity to know the audio length
			];

			var soundCRAZY = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AllNotebooksFinal.wav")), "Vfx_BAL_CongratsNormal_0", SoundType.Effect, Color.green);
			soundCRAZY.additionalKeys = [
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_1", time = 2.051f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_2", time = 4.842f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsAngry_0", time = 7.185f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_4", time = 12.694f} // Tip: use audacity to know the audio length
			];

			foreach (var man in Resources.FindObjectsOfTypeAll<MainGameManager>())
				fieldInfo.SetValue(man, man.name == "Lvl3_MainGameManager 1" ? soundCRAZY : sound);
			
		}

		static string MiscPath => Path.Combine(BasePlugin.ModPath, "misc");

		const string AudioFolder = "Audios", TextureFolder = "Textures";

        internal readonly static List<FloorData> floorDatas = [new("F1"), new("F2"), new("F3"), new("END")];

		public readonly static AssetManager man = new();

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
	}
}
