﻿using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PixCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() 
		{ 
			SoundObject[] sds = [
				GetSound("Pix_Detected.wav", "Vfx_Pix_TarDet", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Prepare.wav", "Vfx_Pix_Prepare", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Stop.wav", "Vfx_Pix_Stop1", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Easy.wav", "Vfx_Pix_Ez", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Successful.wav", "Vfx_Pix_MisSuc", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Failed.wav", "Vfx_Pix_MisFail", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Grrr.wav", "Vfx_Pix_Grr", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_NextTime.wav", "Vfx_Pix_GetYou", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSound("Pix_Shoot.wav", "Vfx_Pix_Shoot", SoundType.Voice, new(0.6f, 0f, 0f)),
				GetSoundNoSub("shock.wav", SoundType.Voice)
				];
			// 0 - 2: spot ; 3 - 4: success ; 5 - 7: fail

			sds[2].additionalKeys = [new() { key = "Vfx_Pix_Stop2", time = 1.3f}, new() { key = "Vfx_Pix_Stop3", time = 1.724f }];

			return sds;
		}

		protected override Sprite[] GenerateSpriteOrder() =>
		[..GetSpriteSheet(22, 1, 12f, "pix.png"), ..GetSpriteSheet(2, 1, 25f, "laserBeam.png"), ..GetSpriteSheet(4, 1, 15f, "shock.png")];
		
		public override void SetupPrefab()
		{
			base.SetupPrefab();

			// Setup audio
			var pix = (Pix)Npc;
			pix.audMan = GetComponent<PropagatedAudioManager>();
			pix.audReady = [soundObjects[0], soundObjects[1], soundObjects[2]];
			pix.audHappy = [soundObjects[3], soundObjects[4]];
			pix.audAngry = [soundObjects[5], soundObjects[6], soundObjects[7]];
			pix.audShoot = soundObjects[8];

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
			laserPre.name = "PixLaserBeamRenderer";
			laserHolder.name = "PixLaserBeam";

			var laser = laserHolder.gameObject.AddComponent<PixLaserBeam>();
			laser.flyingSprites = [storedSprites[22], storedSprites[23]];
			laser.shockSprites = [storedSprites[24], storedSprites[25], storedSprites[26], storedSprites[27]];
			laser.renderer = laserPre;

			laser.entity = laserHolder.gameObject.CreateEntity(2f, 2f, laserHolder.transform).SetEntityCollisionLayerMask(LayerStorage.gumCollisionMask);
			laser.audMan = laserHolder.gameObject.CreatePropagatedAudioManager(15, 45);
			laser.audShock = soundObjects[9];
			pix.laserPre = laser;
		}
	}
}
