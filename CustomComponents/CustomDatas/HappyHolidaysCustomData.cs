using BBTimes.CustomContent.NPCs;
using MTM101BaldAPI.Registers;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HappyHolidaysCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
				[GetSound("HappyHolidays.wav", "Vfx_HapH_MerryChristmas", SoundType.Voice, new(0.796875f, 0f, 0f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(65f, "happyholidays.png")];

		public override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			((HappyHolidays)Npc).objects = ItemMetaStorage.Instance.GetAllWithFlags(ItemFlags.None | ItemFlags.Persists | ItemFlags.CreatesEntity).Where(x => !x.flags.HasFlag(ItemFlags.InstantUse)).ToArray().ToValues();

			//// TEMPORARY WORKAROUND TO REMOVE DUPLICATES (api bug)
			//List<ItemObject> list = new(ItemMetaStorage.Instance.GetAllWithFlags(ItemFlags.None | ItemFlags.Persists | ItemFlags.CreatesEntity).ToValues());

			//HashSet<ItemObject> duplicates = [];
			//for (int i = 0; i < list.Count; i++)
			//{
			//	if (duplicates.Contains(list[i]))
			//	{
			//		list.RemoveAt(i);
			//		i--;
			//		continue;
			//	}
			//	duplicates.Add(list[i]);
			//}

			//GetComponent<HappyHolidays>().objects = [.. list];
		}

	}
}
