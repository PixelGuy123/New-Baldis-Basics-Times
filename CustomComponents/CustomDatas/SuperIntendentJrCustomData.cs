using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SuperIntendentJrCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[
			GetSound("spj_principal.wav", "Vfx_Spj_Found", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_wonder.wav", "Vfx_Spj_Wander", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_step1.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			GetSound("spj_step2.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f))
			];

		protected override Sprite[] GenerateSpriteOrder() =>
			[
			GetSprite(pixels, "spj_walk1.png"),
			GetSprite(pixels, "spj_walk2.png"),
			GetSprite(pixels, "spj_scream1.png"),
			GetSprite(pixels, "spj_scream2.png")
			];
		public override void SetupPrefab() // edit me
		{
			base.SetupPrefab();
			var spj = (SuperIntendentJr)Npc;
			spj.anim = storedSprites;
			spj.audWarn = soundObjects[0];
			spj.audWonder = soundObjects[1];
			spj.audStep1 = soundObjects[2];
			spj.audStep2 = soundObjects[3];
			spj.audMan = GetComponent<PropagatedAudioManager>();
			spj.stepMan = gameObject.CreatePropagatedAudioManager(spj.audMan.minDistance, spj.audMan.maxDistance);
			spj.renderer = spj.spriteRenderer[0];
		}

		const float pixels = 72f;
	}
}
