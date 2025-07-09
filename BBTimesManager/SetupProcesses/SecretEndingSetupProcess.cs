using System.IO;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.SecretEndingComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using PlusLevelFormat;
using PlusLevelLoader;
using UnityEngine;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
    {
        public static void SetupPreAssetsForSecretEnding()
        {
            var sprs = TextureExtensions.LoadSpriteSheet(6, 3, 25f, MiscPath, TextureFolder, "SecretEnding", "susComputerBaldi.png");

            var bal = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0])
                .AddSpriteHolder(out var balRenderer, 0.345f, 0);
            bal.gameObject.AddBoxCollider(Vector3.zero, new(65f, 5f, 65f), true);
            bal.name = "Times_SecretBaldi";

            bal.gameObject.AddObjectToEditor(); // Technically they exist in the editor, but none of them will be placeable in the final build

            var secBal = bal.gameObject.AddComponent<SecretBaldi>();
            secBal.audMan = secBal.gameObject.CreateAudioManager(45f, 135f);
            secBal.renderer = balRenderer;

            // Sprites for Baldi

            secBal.sprLookingComputer = [sprs[0]]; // Oh boy, this sprite selection will be a fun ride...
            secBal.sprOnlyPeek = [sprs[1]];
            secBal.sprSideEyeBack = sprs.TakeAPair(1, 4);
            secBal.sprFacingFront = sprs.TakeAPair(5, 6);
            secBal.sprFacingFrontNervous = sprs.TakeAPair(11, 6);

            sprs = TextureExtensions.LoadSpriteSheet(12, 3, 25f, MiscPath, TextureFolder, "SecretEnding", "evilBaldiSheet.png");
            secBal.sprAngryBal = sprs.TakeAPair(0, 6);
            secBal.sprAngryHappyBal = sprs.TakeAPair(6, 6);
            secBal.sprAngryHappySideEyeBal = sprs.TakeAPair(12, 6);
            secBal.sprThinkingBal = sprs.TakeAPair(18, 6);
            secBal.sprTakeRulerAnim = sprs.TakeAPair(24, 5);
            secBal.sprWithRulerBal = sprs.TakeAPair(28, 6);

            secBal.sprCatchBal = TextureExtensions.LoadSpriteSheet(3, 2, 27f, MiscPath, TextureFolder, "SecretEnding", "baldiCatchSheet.png");


            // Audios for Baldi
            secBal.audMeetMe1 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_MeetBaldi_1.wav")), "Vfx_SecBAL_Meet_1", SoundType.Voice, Color.green);
            secBal.audMeetMe1.additionalKeys = [new() { key = "Vfx_SecBAL_Meet_2", time = 2.976f }];

            secBal.audMeetMe2 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_MeetBaldi_2.wav")), "Vfx_SecBAL_Meet_3", SoundType.Voice, Color.green);

            secBal.audMeetMe3 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_MeetBaldi_3.wav")), "Vfx_SecBAL_Meet_4", SoundType.Voice, Color.green);
            secBal.audMeetMe3.additionalKeys = [
                new() { key = "Vfx_SecBAL_Meet_5", time = 2.266f },
                new() { key = "Vfx_SecBAL_Meet_6", time = 7.912f },
                new() { key = "Vfx_SecBAL_Meet_7", time = 12.687f },
                new() { key = "Vfx_SecBAL_Meet_8", time = 16.249f }
                ];

            secBal.audMeetMe4 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_MeetBaldi_4.wav")), "Vfx_SecBAL_Meet_9", SoundType.Voice, Color.green);
            secBal.audMeetMe4.additionalKeys = [
                new() { key = "Vfx_SecBAL_Meet_10", time = 1.572f }
                ];

            secBal.audAngry1 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence1.wav")), "Vfx_SecBAL_EndSequence_1", SoundType.Voice, Color.green);

            secBal.audAngry2 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence2.wav")), "Vfx_SecBAL_EndSequence_2", SoundType.Voice, Color.green);


            secBal.audAngry3 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence3.wav")), "Vfx_SecBAL_EndSequence_3", SoundType.Voice, Color.green);
            secBal.audAngry3.additionalKeys = [
                new() { key = "Vfx_SecBAL_EndSequence_4", time = 2.26f },
                new() { key = "Vfx_SecBAL_EndSequence_5", time = 5.031f },
                new() { key = "Vfx_SecBAL_EndSequence_6", time = 8.125f },
                new() { key = "Vfx_SecBAL_EndSequence_7", time = 10.566f },
                new() { key = "Vfx_SecBAL_EndSequence_8", time = 12.834f },
                new() { key = "Vfx_SecBAL_EndSequence_9", time = 15.658f },
                new() { key = "Vfx_SecBAL_EndSequence_10", time = 18.853f }
                ];

            secBal.audAngry4 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence4.wav")), "Vfx_SecBAL_EndSequence_11", SoundType.Voice, Color.green);
            secBal.audAngry5 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence5.wav")), "Vfx_SecBAL_EndSequence_12", SoundType.Voice, Color.green);

            secBal.audAngry6 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence6.wav")), "Vfx_SecBAL_EndSequence_13", SoundType.Voice, Color.green);
            secBal.audAngry6.additionalKeys = [
                new() { key = "Vfx_SecBAL_EndSequence_14", time = 1.629f },
                new() { key = "Vfx_SecBAL_EndSequence_15", time = 3.619f },
                new() { key = "Vfx_SecBAL_EndSequence_16", time = 7.354f },
                new() { key = "Vfx_SecBAL_EndSequence_17", time = 10.251f },
                new() { key = "Vfx_SecBAL_EndSequence_18", time = 12.914f }
                ];

            secBal.audAngry7 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence7.wav")), "Vfx_SecBAL_EndSequence_19", SoundType.Voice, Color.green);
            secBal.audAngry8 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence8.wav")), "Vfx_SecBAL_EndSequence_20", SoundType.Voice, Color.green);

            secBal.audAngry9 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence9.wav")), "Vfx_SecBAL_EndSequence_21", SoundType.Voice, Color.green);
            secBal.audAngry9.additionalKeys = [
                new() { key = "Vfx_SecBAL_EndSequence_22", time = 1.804f },
                new() { key = "Vfx_SecBAL_EndSequence_23", time = 4.561f }
                ];

            secBal.volumeAnimator = secBal.gameObject.AddComponent<SpriteVolumeAnimator>();
            secBal.volumeAnimator.audMan = secBal.audMan;
            secBal.volumeAnimator.renderer = balRenderer;
            secBal.volumeAnimator.volumeMultipler = 1.2f;
            secBal.UpdateSpriteTo(secBal.sprLookingComputer);



            var invisibleWall = ObjectCreationExtension.CreateCube(man.Get<Sprite>("whitePix").texture, false); // White texture to show where it is
            invisibleWall.transform.localScale = new Vector3(10f, 10f, 1f);
            invisibleWall.name = "Times_InvisibleWall";
            invisibleWall.AddObjectToEditor();

            var noRendererWall = invisibleWall.AddComponent<NoRendererOnStart>();
            noRendererWall.collider = noRendererWall.GetComponent<BoxCollider>();

            var destroyableInvisibleWall = noRendererWall.DuplicatePrefab();
            destroyableInvisibleWall.canBeDisabled = true;
            destroyableInvisibleWall.name = "Times_CanBeDisabledInvisibleWall";
            destroyableInvisibleWall.gameObject.AddObjectToEditor();

            var scewInvisibleWall = noRendererWall.DuplicatePrefab();
            scewInvisibleWall.affectedByScrewDriver = true;
            scewInvisibleWall.name = "Times_ScrewingInvisibleWall";
            scewInvisibleWall.gameObject.AddObjectToEditor();

            var keyLocked = noRendererWall.DuplicatePrefab();
            keyLocked.affectedByKey = true;
            keyLocked.name = "Times_KeyLockedInvisibleWall";
            keyLocked.gameObject.AddObjectToEditor();
            keyLocked.audMan = keyLocked.gameObject.CreatePropagatedAudioManager(45f, 65f);
            keyLocked.audUse = GenericExtensions.FindResourceObject<StandardDoor>().audDoorUnlock; // Unlock noises


            var generatorComp = new GameObject("Times_SecretGenerator");
            generatorComp.AddBoxCollider(Vector3.zero, new(19f, 10f, 21f), false);

            var generator = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SecretEnding", "SecretGeneratorBox.png")));
            generator.name = "Times_SecretGeneratorRenderer";
            Object.Destroy(generator.GetComponent<BoxCollider>());
            generator.transform.SetParent(generatorComp.transform);
            generator.transform.localPosition = new(9.5f, 2.5f, -10.5f);
            generator.transform.localScale = new(19f, 5f, 21f);
            generator.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            generatorComp.AddObjectToEditor();


            var genNoise = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "SecretBaldi", "generatorLoop.wav")), string.Empty, SoundType.Effect, Color.white);
            genNoise.subtitle = false;

            generatorComp.gameObject.CreatePropagatedAudioManager(10f, 65f)
                .AddStartingAudiosToAudioManager(true, genNoise);

            var generatorCylinderRenderer = ObjectCreationExtension.CreatePrimitiveObject(PrimitiveType.Cylinder, TextureExtensions.CreateSolidTexture(1, 1, new(0.35f, 0.35f, 0.35f)));
            var generatorCylinder = new GameObject("Times_GeneratorCylinder");
            generatorCylinder.AddBoxCollider(Vector3.up * 4.5f, Vector3.one * 2f, true); // Just so the Editor detects this one first
            generatorCylinderRenderer.transform.SetParent(generatorCylinder.transform);
            generatorCylinder.AddObjectToEditor();

            generatorCylinderRenderer.transform.localPosition = Vector3.zero;
            generatorCylinderRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, 50f);
            generatorCylinderRenderer.transform.localScale = new(7f, 12f, 7f);

            Sprite[] baldis = TextureExtensions.LoadSpriteSheet(4, 1, 35f, MiscPath, TextureFolder, "SecretEnding", "containedBaldis.png");
            for (int i = 0; i < baldis.Length; i++)
            {
                var baldiObj = ObjectCreationExtensions.CreateSpriteBillboard(baldis[i]).AddSpriteHolder(out var baldiRenderer, 0f, 0);
                baldiObj.name = "Times_ContainedBaldi_F" + (i + 1);
                baldiObj.gameObject.AddBoxCollider(Vector3.zero, new(1.5f, 5f, 1.5f), false);
                baldiObj.gameObject.AddObjectToEditor();

                baldiRenderer.name = "ContainedBaldiRenderer";
            }

            var yayComputer = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromFile(Path.Combine(MiscPath, TextureFolder, "SecretEnding", "theYAYComputer.png"), Vector2.one * 0.5f, 25f));
            yayComputer.name = "Times_theYAYComputer";
            yayComputer.gameObject.AddObjectToEditor();

            var lorePaper = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromFile(Path.Combine(MiscPath, TextureFolder, "SecretEnding", "TheTrueLorePaper.png"), Vector2.one * 0.5f, 40f), false)
                .AddSpriteHolder(out var paperRenderer, 0.07f);
            paperRenderer.name = "LorePaperRenderer";
            paperRenderer.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lorePaper.name = "Times_TrueLorePaper";
            lorePaper.gameObject.AddObjectToEditor();

            var genLeverSprs = TextureExtensions.LoadSpriteSheet(2, 1, 25f, MiscPath, TextureFolder, "SecretEnding", "generatorLever.png");

            var genLever = ObjectCreationExtensions.CreateSpriteBillboard(genLeverSprs[0], false)
                .AddSpriteHolder(out var genLeverRender, Vector3.forward * 0.24f, LayerStorage.iClickableLayer);
            genLever.name = "Times_GeneratorLever";
            genLeverRender.name = "GenLeverRenderer";
            genLever.gameObject.AddObjectToEditor();

            var lever = genLever.gameObject.AddComponent<FakeLever>();
            lever.renderer = genLeverRender;
            lever.sprUnscrewed = genLeverSprs[1];
            lever.gameObject.AddBoxCollider(Vector3.zero, new(4f, 4f, 1f), true);
        }

        public static void SetupPostAssetsForSecretEnding()
        {
            // --- Times Ending Manager Setup ---
            var sceneObjectClone = Object.Instantiate(GenericExtensions.FindResourceObjectByName<SceneObject>("EndlessPremadeMedium"));
            sceneObjectClone.name = "TimesSecretEnding";

            sceneObjectClone.extraAsset = Object.Instantiate(sceneObjectClone.extraAsset);
            sceneObjectClone.extraAsset.name = "TimesSecretExtraAsset";
            sceneObjectClone.extraAsset.npcSpawnPoints.Clear();
            sceneObjectClone.extraAsset.npcsToSpawn.Clear();
            sceneObjectClone.extraAsset.lightMode = LightMode.Additive;
            sceneObjectClone.extraAsset.minLightColor = Color.white;

            sceneObjectClone.levelContainer = null;
            sceneObjectClone.levelNo = 99;
            sceneObjectClone.nextLevel = null;
            sceneObjectClone.levelTitle = "???";
            sceneObjectClone.nameKey = "???";
            sceneObjectClone.shopItems = [];
            sceneObjectClone.skyboxColor = Color.black;
            sceneObjectClone.usesMap = false;
            sceneObjectClone.levelObject = null;


            using (BinaryReader reader = new(File.OpenRead(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLevel.cbld"))))
            {
                // --- Setup secret ending level asset and textures ---
                sceneObjectClone.levelAsset = CustomLevelLoader.LoadLevelAsset(LevelExtensions.ReadLevel(reader));
                sceneObjectClone.levelAsset.name = "TimesSecretEndingAsset";
                sceneObjectClone.levelAsset.rooms[0].ceilTex = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLabCeiling.png"));
                sceneObjectClone.levelAsset.rooms[0].wallTex = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLabWall.png"));
                sceneObjectClone.levelAsset.rooms[0].florTex = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretLabFloor.png"));

                // --- Setup door materials and mask ---
                sceneObjectClone.levelAsset.rooms[0].doorMats = ObjectCreators.CreateDoorDataObject("TimesSecretLabMetalDoor",
                    AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "smallMetalDoorOpen.png")),
                    AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "smallMetalDoorClosed.png")));

                var doorTextureMask = AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "metalDoorMask.png"));
                sceneObjectClone.levelAsset.rooms[0].doorMats.open.SetTexture("_Mask", doorTextureMask);
                sceneObjectClone.levelAsset.rooms[0].doorMats.shut.SetTexture("_Mask", doorTextureMask);

                // --- Add posters to the secret ending room ---
                sceneObjectClone.levelAsset.posters.Add(new()
                {
                    poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "liveTubeMakeUp.png"))]),
                    position = new(16, 12),
                    direction = Direction.North
                });
                sceneObjectClone.levelAsset.posters.Add(new()
                {
                    poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "levelGenMakeUp.png"))]),
                    position = new(15, 10),
                    direction = Direction.South
                });
                sceneObjectClone.levelAsset.posters.Add(new()
                {
                    poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "chk_funFormula.png"))]),
                    position = new(15, 7),
                    direction = Direction.South
                });
                sceneObjectClone.levelAsset.posters.Add(new()
                {
                    poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "chk_theNoWinFormula.png"))]),
                    position = new(15, 9),
                    direction = Direction.North
                });
                sceneObjectClone.levelAsset.posters.Add(new()
                {
                    poster = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "chk_noRealWin.png"))]),
                    position = new(16, 8),
                    direction = Direction.East
                });
                sceneObjectClone.levelAsset.rooms[0].hasActivity = false;
                sceneObjectClone.levelAsset.rooms[0].activity = null;
                // --- Setup door clone ---
                var newDoor = (StandardDoor)sceneObjectClone.levelAsset.doors[0].doorPre.SafeDuplicatePrefab(true);
                newDoor.audDoorShut = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "metalDoorShut.wav")), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
                newDoor.audDoorOpen = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "metalDoorOpen.wav")), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
                newDoor.name = "SmallMetalDoor";

                sceneObjectClone.levelAsset.doors.ForEach(d => d.doorPre = newDoor);
            }

            // --- Setup secret ending manager object ---
            var newManager = new BaseGameManagerBuilder<TimesSecretEndingManager>()
            .SetObjectName("TimesSecretEndingManager")
            .SetNPCSpawnMode(GameManagerNPCAutomaticSpawn.Never)
            .SetNameKey("???")
            .SetLevelNumber(99)
            .Build();

            newManager.canvas = ObjectCreationExtensions.CreateCanvas();
            newManager.canvas.transform.SetParent(newManager.transform);
            newManager.canvas.gameObject.SetActive(false);
            newManager.canvas.name = newManager.name + "Canvas";

            newManager.activeImage = ObjectCreationExtensions.CreateImage(newManager.canvas, TextureExtensions.CreateSolidTexture(480, 360, Color.black));
            newManager.activeImage.name = "TimesEndScreen";
            var baldiReference = GenericExtensions.FindResourceObject<Baldi>();

            newManager.audSlap = baldiReference.slap;
            newManager.audLoseSounds = baldiReference.loseSounds;

            newManager.timesScreen = AssetLoader.SpriteFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "SecretEnding", "secretTimesEnd.jpg"), Vector2.one * 0.5f);
            newManager.audSeeYaSoon = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "Secret_BAL_EndSequence_End.wav")), "Vfx_SecBAL_EndSequence_SeeYa", SoundType.Voice, Color.green);

            newManager.audMan = newManager.gameObject.CreateAudioManager(15f, 25f).MakeAudioManagerNonPositional();
            newManager.audHummmmm = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "SecretBaldi", "spookyNoisesForEnding.mp3")), string.Empty, SoundType.Music, Color.white);
            newManager.audHummmmm.subtitle = false;

            sceneObjectClone.manager = newManager;
            MainGameManagerPatches.secretEndingObj = sceneObjectClone;
        }
    }
}