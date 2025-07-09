using System.IO;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
    {
        public static void SetupChristmasHoliday()
        {
            // --- Setup Christmas Baldi prefab and audio ---
            var baldiSPrites = TextureExtensions.LoadSpriteSheet(6, 1, 30f, MiscPath, TextureFolder, GetAssetName("christmasBaldi.png"));
            var chBaldi = ObjectCreationExtensions.CreateSpriteBillboard(baldiSPrites[0])
                .AddSpriteHolder(out var chBaldiRenderer, 4f, LayerStorage.iClickableLayer); // Baldo offset should be exactly 5f + hisDefaultoffset
            chBaldi.gameObject.AddBoxCollider(Vector3.up * 5f, new(2.5f, 10f, 2.5f), true);
            chBaldi.name = "Times_ChristmasBaldi";
            chBaldiRenderer.name = "Times_ChristmasBaldi_Renderer";

            chBaldi.gameObject.ConvertToPrefab(true); // He won't be in the editor, he's made specifically for christmas mode

            var christmasBaldi = chBaldi.gameObject.AddComponent<ChristmasBaldi>();

            // --- Assign audio and present references ---
            christmasBaldi.audMan = christmasBaldi.gameObject.CreatePropagatedAudioManager(95f, 175f);
            christmasBaldi.present = man.Get<ItemObject>("times_itemObject_Present");
            christmasBaldi.audBell = man.Get<SoundObject>("audRing");
            christmasBaldi.audIntro = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_PresentIntro.wav")), "Vfx_BAL_Pitstop_PresentIntro_1", SoundType.Voice, Color.green);
            christmasBaldi.audIntro.additionalKeys = [
                new() { key = "Vfx_BAL_Pitstop_PresentIntro_2", time = 2.417f },
                    new() { key = "Vfx_BAL_Pitstop_PresentIntro_3", time = 5.492f },
                    new() { key = "Vfx_BAL_Wow", time = 9.544f },
                    new() { key = "Vfx_BAL_Pitstop_PresentIntro_4", time = 11.029f },
                    new() { key = "Vfx_BAL_Pitstop_PresentIntro_5", time = 14.059f }
                ];

            christmasBaldi.audBuyItem = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_buypresent.wav")), "Vfx_BAL_Pitstop_MerryChristmas", SoundType.Voice, Color.green);

            christmasBaldi.audNoYtps = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_needYtpsForPresent.wav")), "Vfx_BAL_Pitstop_Nopresent_1", SoundType.Voice, Color.green);
            christmasBaldi.audNoYtps.additionalKeys = [
                new() { key = "Vfx_BAL_Pitstop_Nopresent_2", time = 0.818f }
                ];

            christmasBaldi.audGenerous = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Pitstop_Generous.wav")), "Vfx_BAL_Pitstop_Generous_1", SoundType.Voice, Color.green);
            christmasBaldi.audGenerous.additionalKeys = [
                new() { key = "Vfx_BAL_Pitstop_Generous_2", time = 2.597f }
                ];

            christmasBaldi.audCollectingPresent = [
                ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Pitstop_Thanks1.wav")), "Vfx_BAL_Pitstop_Thanks1", SoundType.Voice, Color.green),
                    ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_Pitstop_Thanks2.wav")), "Vfx_BAL_Pitstop_Thanks2", SoundType.Voice, Color.green)
                ];

            // --- Add sprite volume animator ---
            var volumeAnimator = christmasBaldi.gameObject.AddComponent<SpriteVolumeAnimator>();
            volumeAnimator.audMan = christmasBaldi.audMan;
            volumeAnimator.renderer = chBaldiRenderer;
            volumeAnimator.volumeMultipler = 1.2f;
            volumeAnimator.sprites = baldiSPrites;

            // --- Add Christmas Baldi to pitstop asset ---
            var pitstopAsset = GenericExtensions.FindResourceObjectByName<LevelAsset>("Pitstop"); // Find shop pitstop here
            pitstopAsset.tbos.Add(new() { direction = Direction.North, position = new(30, 11), prefab = christmasBaldi });
        }
    }
}