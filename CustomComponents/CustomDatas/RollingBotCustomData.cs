using UnityEngine;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using System.Linq;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class RollingBotCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("rol_warning.wav", "Vfx_Rollbot_Warning", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			GetSound("rol_error.wav", "Vfx_Rollbot_Error", SoundType.Voice, new(0.7f, 0.7f, 0.7f)),
			GetSoundNoSub("shock.wav", SoundType.Voice),
			GetSound("motor.wav", "Sfx_1PR_Motor", SoundType.Voice, new(0.7f, 0.7f, 0.7f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[.. GetSpriteSheet(4, 4, 25f, "rollBotSheet.png"), ..GetSpriteSheet(2, 2, 25f, "shock.png")];
		

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			// eletricity creation
			Sprite[] anim = [.. storedSprites.Skip(spriteAmount)];
			var eleRender = ObjectCreationExtensions.CreateSpriteBillboard(anim[0], false).AddSpriteHolder(0.1f, 0);
			eleRender.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			eleRender.transform.parent.gameObject.ConvertToPrefab(true);
			eleRender.name = "Sprite";

			var ele = eleRender.transform.parent.gameObject.AddComponent<Eletricity>();
			ele.name = "RollingEletricity";
			var ani = ele.gameObject.AddComponent<AnimationComponent>();
			ani.animation = anim;
			ani.renderer = eleRender;
			ani.speed = 15f;

			ele.ani = ani;

			ele.gameObject.CreatePropagatedAudioManager(5f, 35f).AddStartingAudiosToAudioManager(true, soundObjects[2]);

			ele.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * (LayerStorage.TileBaseOffset / 2), true);


			// npc setup
			var bot = (RollingBot)Npc;
			bot.audError = soundObjects[1];
			bot.audWarning = soundObjects[0];
			bot.audMan = GetComponent<PropagatedAudioManager>();

			bot.gameObject.CreatePropagatedAudioManager(10f, 115f).AddStartingAudiosToAudioManager(true, soundObjects[3]);

			bot.CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(spriteAmount, [.. storedSprites.Take(spriteAmount)])
				);

			bot.eletricityPre = ele;
		}

		const int spriteAmount = 16;
	}
}
