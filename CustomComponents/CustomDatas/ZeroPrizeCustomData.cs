using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ZeroPrizeCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[
		GetSound("0thprize_mustsweep.wav", "Vfx_0TH_Sweep", SoundType.Voice, new(0.99609375f, 0.99609375f, 0.796875f)),
		GetSound("0thprize_timetosweep.wav", "Vfx_0TH_WannaSweep", SoundType.Voice, new(0.99609375f, 0.99609375f, 0.796875f))
			];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(2, 1, 45f, "0thprize.png");

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var prize = (ZeroPrize)Npc;

			prize.audStartSweep = soundObjects[1];
			prize.audSweep = soundObjects[0];

			prize.activeSprite = storedSprites[0];
			prize.deactiveSprite = storedSprites[1];

			prize.audMan = GetComponent<PropagatedAudioManager>();

			((CapsuleCollider)prize.baseTrigger[0]).radius = 4f; // default radius of Gotta Sweep
		}
	}
}
