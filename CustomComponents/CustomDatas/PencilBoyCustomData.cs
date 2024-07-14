using UnityEngine;
using BBTimes.CustomContent.NPCs;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PencilBoyCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[
			GetSound("PB_Angry0.wav", "Vfx_PB_Wander1", SoundType.Voice, Color.yellow),
			GetSound("PB_Angry1.wav", "Vfx_PB_Wander2", SoundType.Voice, Color.yellow),
			GetSound("PB_Angry2.wav", "Vfx_PB_Wander3", SoundType.Voice, Color.yellow),
			GetSound("PB_EvilLaught.wav", "Vfx_PB_Catch", SoundType.Voice, Color.yellow),
			GetSound("PB_SeeLaught.wav", "Vfx_PB_Spot", SoundType.Voice, Color.yellow),
			GetSound("PB_DeathIncoming.wav", "Vfx_PB_SuperAngry", SoundType.Voice, Color.yellow)
			];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(2, 2, 65f, "pencilBoy.png");
		public override void SetupPrefab()
		{
			base.SetupPrefab();

			var boy = (PencilBoy)Npc;
			boy.audMan = GetComponent<PropagatedAudioManager>();

			boy.audWandering = [soundObjects[0], soundObjects[1], soundObjects[2]];
			boy.audEvilLaught = soundObjects[3];
			boy.audSeeLaught = soundObjects[4];
			boy.audSuperAngry = soundObjects[5];

			boy.angrySprite = storedSprites[0];
			boy.findPlayerSprite = storedSprites[1];
			boy.happySprite = storedSprites[2];
			boy.superAngrySprite = storedSprites[3];
		}

		public override void Stabbed()
		{
			base.Stabbed();
			((PencilBoy)Npc).GetSuperAngry();
		}
	}
}
