using BBTimes.CustomContent.NPCs;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HappyHolidaysCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
				
				[MTM101BaldAPI.ObjectCreators.CreateSoundObject(
				MTM101BaldAPI.AssetTools.AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "HappyHolidays.wav")), // Gets just one audio
				"Vfx_HapH_MerryChristmas", SoundType.Voice, new(0.796875f, 0f, 0f)
				)];

		public override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			// GetComponent<HappyHolidays>().objects = ItemMetaStorage.Instance.GetAllWithFlags(ItemFlags.None | ItemFlags.Persists | ItemFlags.CreatesEntity).ToValues();

			// TEMPORARY WORKAROUND TO REMOVE DUPLICATES (api bug)
			List<ItemObject> list = new(ItemMetaStorage.Instance.GetAllWithFlags(ItemFlags.None | ItemFlags.Persists | ItemFlags.CreatesEntity).ToValues());

			HashSet<ItemObject> duplicates = [];
			for (int i = 0; i < list.Count; i++)
			{
				if (duplicates.Contains(list[i]))
				{
					list.RemoveAt(i);
					i--;
					continue;
				}
				duplicates.Add(list[i]);
			}

			GetComponent<HappyHolidays>().objects = [.. list];
		}

	}
}
