using BBTimes.CustomContent.NPCs;
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
			GetSprite(30f, "spj_scream1.png"),
			GetSprite(30f, "spj_scream2.png"),
			GetSprite(30f, "spj_walk1.png"),
			GetSprite(30f, "spj_walk2.png")
			];
		public override void SetupPrefab() // edit me
		{
			base.SetupPrefab();
			var spj = (SuperIntendentJr)Npc;

		}
	}
}
