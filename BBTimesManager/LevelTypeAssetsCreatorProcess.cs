
using System.IO;
using System.Linq;
using BBTimes.CustomContent.Builders;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using UnityEngine;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
    {
        public static AssetManager levelTypeAssetManager;
        public static string LevelTypeAssetPath => Path.Combine(BasePlugin.ModPath, "LevelTypes");
        public static void LoadLevelTypeAssets()
        {
            // load 
            levelTypeAssetManager = new AssetManager();

            // start loading Sewer floor type textures
            levelTypeAssetManager.Add("tex_sewer_floor", AssetLoader.TextureFromFile(Path.Combine(LevelTypeAssetPath, "Sewer", "Textures", "floortex.png")));
            levelTypeAssetManager.Add("tex_sewer_wall", AssetLoader.TextureFromFile(Path.Combine(LevelTypeAssetPath, "Sewer", "Textures", "walltex.png")));
            levelTypeAssetManager.Add("tex_sewer_ceil", AssetLoader.TextureFromFile(Path.Combine(LevelTypeAssetPath, "Sewer", "Textures", "ceiltex.png")));

            Material WaterFlood = new Material(Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "FloodWater"));
            WaterFlood.SetVector("_Tiling", new Vector4(100, 100, 1, 1));
            // its time to drink (sewage) water
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.ConvertToPrefab(true);
            quad.transform.localScale = new Vector3(2500, 2500, 0.01f);
            quad.GetComponent<MeshRenderer>().material = WaterFlood;
            quad.transform.rotation = Quaternion.Euler(90, 0, 0);
            quad.AddComponent<WaterMover>();

            GameObject prefabWaterCreatorStructure = new GameObject("Structure_WaterCreator");
            prefabWaterCreatorStructure.ConvertToPrefab(true);
            Structure_WaterCreator waterCreator = prefabWaterCreatorStructure.AddComponent<Structure_WaterCreator>();
            waterCreator.waterPrefab = quad.GetComponent<WaterMover>();
            levelTypeAssetManager.Add("Structure_WC", waterCreator);
            levelTypeAssetManager.Add("snd_floodloop",
               ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(LevelTypeAssetPath, "Sewer", "Audios", "FloodLoop.wav")), "imagine needing sounds for environmental stuff lol", SoundType.Effect, Color.gray));
            levelTypeAssetManager.Get<SoundObject>("snd_floodloop").subtitle = false;

        }
    }
}