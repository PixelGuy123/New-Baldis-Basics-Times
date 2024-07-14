using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using System.Linq;
using UnityEngine;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class GlubotronyCustomData : CustomNPCData
	{
		protected override Sprite[] GenerateSpriteOrder() => 
			[.. GetSpriteSheet(8, 1, pixelsPerUnit, "gluebotronyIdle.png"), .. GetSpriteSheet(4, 4, pixelsPerUnit, "gluebotronyMoving.png"), GetSprite(25f, "glue.png")];
		
		

		protected override SoundObject[] GenerateSoundObjects() =>
		[GetSoundNoSub("PrepareWalk.wav", SoundType.Voice),
		GetSound("step.wav", "Vfx_Gboy_Walk", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		GetSound("GB_There.wav", "Vfx_Gboy_putGlue2", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		GetSound("GB_Done.wav", "Vfx_Gboy_putGlue1", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		GetSound("GB_PrankingTime.wav", "Vfx_Gboy_PrankTime", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		GetSound("GB_Situation.wav", "Vfx_Gboy_Situation", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		GetSound("GB_Mischievous.wav", "Vfx_Gboy_Mischiveous", SoundType.Voice, new(0.19921875f, 0.59765625f, 0.99609375f)),
		GetSoundNoSub("glueSplash.wav", SoundType.Voice),
		GetSoundNoSub("glueStep.wav", SoundType.Voice)
		];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var gb = (Glubotrony)Npc;
			gb.audMan = GetComponent<PropagatedAudioManager>();
			gb.stepAudMan = gameObject.CreatePropagatedAudioManager(90f, 165f);
			gb.audPrepareStep = soundObjects[0];
			gb.audStep = soundObjects[1];
			gb.audPutGlue = [soundObjects[2], soundObjects[3]];
			gb.audWander = [soundObjects[4], soundObjects[5], soundObjects[6]];

			gb.renderer = gb.CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Take(8)]),
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Skip(8).Take(8)]),
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Skip(8).Skip(8).Take(8)])
				);

			gb.sprIdle = storedSprites[0];
			gb.sprStep1 = storedSprites[8];
			gb.sprStep2 = storedSprites[16];

			// Glue setup
			var glueRender = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[storedSprites.Length - 1], false).AddSpriteHolder(-4.9f, 0);
			glueRender.gameObject.layer = 0;
			glueRender.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			glueRender.transform.parent.gameObject.ConvertToPrefab(true);

			var glue = glueRender.transform.parent.gameObject.AddComponent<Glue>();
			glue.render = glueRender.transform;

			glue.audMan = glue.gameObject.CreatePropagatedAudioManager(45f, 65f).AddStartingAudiosToAudioManager(false, soundObjects[7]);
			glue.audSteppedOn = soundObjects[8];

			glue.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * (LayerStorage.TileBaseOffset / 2), true);			

			gb.gluePre = glue;

		}

		const float pixelsPerUnit = 55f;
	}
}
