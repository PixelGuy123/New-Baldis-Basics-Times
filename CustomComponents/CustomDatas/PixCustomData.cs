using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PixCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() 
		{ 
			SoundObject[] sds = [
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Detected.wav")), "Vfx_Pix_TarDet", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Prepare.wav")), "Vfx_Pix_Prepare", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Stop.wav")), "Vfx_Pix_Stop1", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Easy.wav")), "Vfx_Pix_Ez", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Successful.wav")), "Vfx_Pix_MisSuc", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Failed.wav")), "Vfx_Pix_MisFail", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Grrr.wav")), "Vfx_Pix_Grr", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "Pix_Shoot.wav")), "Vfx_Pix_Shoot", SoundType.Voice, new(0.6f, 0f, 0f)),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "shock.wav")), string.Empty, SoundType.Voice, Color.white)
				];
			// 0 - 2: spot ; 3 - 4: success ; 5 - 6: fail

			sds[2].additionalKeys = [new() { key = "Vfx_Pix_Stop2", time = 0.732f}, new() { key = "Vfx_Pix_Stop3", time = 1.369f }];
			sds[8].subtitle = false;

			return sds;
		}
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var pix = (Pix)Npc;
			// Setup audio
			pix.audMan = GetComponent<PropagatedAudioManager>();
			pix.audReady = [soundObjects[0], soundObjects[1], soundObjects[2]];
			pix.audHappy = [soundObjects[3], soundObjects[4]];
			pix.audAngry = [soundObjects[5], soundObjects[6]];
			pix.audShoot = soundObjects[7];

			// setup animated sprites
			pix.rotator = pix.CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(4, storedSprites[0], storedSprites[2], storedSprites[4], storedSprites[6]), // Normal first frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[1], storedSprites[3], storedSprites[5], storedSprites[7]), // Normal second frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[8], storedSprites[10], storedSprites[4], storedSprites[12]), // Angry first frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[9], storedSprites[11], storedSprites[5], storedSprites[13]), // Angry second frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[14], storedSprites[16], storedSprites[4], storedSprites[18]), // Happy first frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[15], storedSprites[17], storedSprites[5], storedSprites[19]) // Happy second frame of rotation map
				);
			pix.normalSprites = [storedSprites[0], storedSprites[1]];
			pix.angrySprites = [storedSprites[8], storedSprites[9]];
			pix.happySprites = [storedSprites[14], storedSprites[15]];
			pix.idleShootingSprites = [storedSprites[20], storedSprites[21]];

			// laser (16, 17)
			var laserPre = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[23]).AddSpriteHolder(0f, LayerStorage.standardEntities);
			var laserHolder = laserPre.transform.parent;
			laserHolder.gameObject.SetAsPrefab(true);
			laserPre.name = "PixLaserBeam";

			var laser = laserHolder.gameObject.AddComponent<PixLaserBeam>();
			laser.flyingSprites = [storedSprites[22], storedSprites[23]];
			laser.shockSprites = [storedSprites[24], storedSprites[25], storedSprites[26], storedSprites[27]];
			laser.renderer = laserPre;

			laser.entity = laserHolder.gameObject.CreateEntity(2f, 2f, laserHolder.transform, [laser]).SetEntityCollisionLayerMask(LayerStorage.gumCollisionMask);
			laser.audMan = laserHolder.gameObject.CreatePropagatedAudioManager(15, 45).SetAudioManagerAsPrefab();
			laser.audShock = soundObjects[8];
			pix.laserPre = laser;
        }
	}
}
