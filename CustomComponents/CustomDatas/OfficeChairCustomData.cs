using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class OfficeChairCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
				[GetSound("ChairRolling.wav","Vfx_OFC_Walk", SoundType.Voice, new(0.74609375f, 0.74609375f, 0.74609375f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(24f, "officechair.png"), GetSprite(24f, "officechairDisabled.png")];
	}
}
