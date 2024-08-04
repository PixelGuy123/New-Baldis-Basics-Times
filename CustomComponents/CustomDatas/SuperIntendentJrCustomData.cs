using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SuperIntendentJrCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [
			GetSound("spj_principal.wav", "Vfx_Spj_Found", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_wonder.wav", "Vfx_Spj_Wander", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_step1.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_step2.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_wtfisthis.wav", "Vfx_Spj_FoundLong1", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f))
			];

			sds[4].additionalKeys = [
				new() { key = "Vfx_Spj_FoundLong2", time = 7.27f },
				new() { key = "Vfx_Spj_FoundLong3", time = 17.911f },
				new() { key = "Vfx_Spj_FoundLong4", time = 27.379f }
				];

			return sds;
		}

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(2, 2, pixels, "spj.png");
		public override void SetupPrefab() // edit me
		{
			base.SetupPrefab();
			var spj = (SuperIntendentJr)Npc;
			spj.anim = storedSprites;
			spj.audWarn = soundObjects[0];
			spj.audWonder = soundObjects[1];
			spj.audStep1 = soundObjects[2];
			spj.audStep2 = soundObjects[3];
			spj.audLongAssInstructions = soundObjects[4];
			spj.audMan = GetComponent<PropagatedAudioManager>();
			spj.stepMan = gameObject.CreatePropagatedAudioManager(spj.audMan.minDistance, spj.audMan.maxDistance);
			spj.renderer = spj.spriteRenderer[0];
		}

		const float pixels = 72f;
	}
}
