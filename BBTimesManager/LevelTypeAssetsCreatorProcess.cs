
using System.IO;
using System.Linq;
using BBTimes.CustomContent.Builders;
using BBTimes.Helpers;
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
            levelTypeAssetManager.Add("spr_ClassStandard_Off", AssetLoader.SpriteFromFile(Path.Combine(LevelTypeAssetPath, "Sewer", "Textures", "ClassStandard_Off.png"), Vector2.one / 2, 10));
            levelTypeAssetManager.Add("spr_ClassStandard_On", AssetLoader.SpriteFromFile(Path.Combine(LevelTypeAssetPath, "Sewer", "Textures", "ClassStandard_On.png"), Vector2.one / 2, 10));
            GameObject prefabDoorLocker = new GameObject("Structure_DoorLocker");
            prefabDoorLocker.ConvertToPrefab(true);
            Structure_DoorLockerSmth doorLocker = prefabDoorLocker.AddComponent<Structure_DoorLockerSmth>();


            var lightGO = new GameObject("RendererLight");
            lightGO.ConvertToPrefab(true);
            lightGO.transform.localPosition = new Vector3(0, 0f, 0);
            lightGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
            var sprRender = lightGO.AddComponent<SpriteRenderer>();
            sprRender.sprite = BBTimesManager.levelTypeAssetManager.Get<Sprite>("spr_ClassStandard_Off");
            doorLocker.Prefab = lightGO;
            levelTypeAssetManager.Add("Structure_DL", doorLocker);
        }
    }
}