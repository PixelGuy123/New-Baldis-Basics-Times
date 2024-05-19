using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	internal class SuperintendentCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("Superintendent.wav","Vfx_SI_BaldiHere", SoundType.Voice, new(0f, 0f, 0.796875f))];
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(46f, "Superintendent.png")];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var s = (Superintendent)Npc;
			s.audMan = s.GetComponent<AudioManager>();
			s.data = this;
		}
	}
}
