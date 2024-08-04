using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HappyHolidaysCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
				[GetSound("HappyHolidays.wav", "Vfx_HapH_MerryChristmas", SoundType.Voice, new(0.796875f, 0f, 0f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(65f, "happyholidays.png")];

		protected override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			
			((HappyHolidays)Npc).objects = [.. GameExtensions.GetAllShoppingItems()];
			((HappyHolidays)Npc).audHappyHolidays = soundObjects[0];
		}

	}
}
