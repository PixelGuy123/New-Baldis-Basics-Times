using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class LeapyCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("leapy_jump.wav", "Vfx_Leapy_Leap", SoundType.Voice, new Color(0f, 0.3984f, 0f)),
		GetSound("leapy_stomp.wav", "Vfx_Leapy_Stomp", SoundType.Voice, new Color(0f, 0.3984f, 0f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(3, 1, 25f, "leapy.png");

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var le = (Leapy)Npc;
			le.audMan = GetComponent<PropagatedAudioManager>();
			le.audJump = soundObjects[0];
			le.audStomp = soundObjects[1];

			le.renderer = le.spriteRenderer[0];
			le.sprIdle = storedSprites[0];
			le.sprPrepare = storedSprites[1];
			le.sprJump = storedSprites[2];
		}
	}
}
