using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PresentCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("prs_unbox.wav", "Vfx_PRS_Unbox", SoundType.Effect, UnityEngine.Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<ITM_Present>().aud_unbox = soundObjects[0];
		}

		protected override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			GetComponent<ITM_Present>().items = [.. ItemMetaStorage.Instance.FindAll(x => x.id != myItmObj.itemType && !x.flags.HasFlag(ItemFlags.InstantUse)).ConvertAll(x => x.value)];

			// Another workaround for this stupid bug
			//List<ItemObject> list = new(ItemMetaStorage.Instance.FindAll(x => x.id != myEnum).ConvertAll(x => x.value));

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
			//GetComponent<ITM_Present>().items = [.. list];
		}
	}
}
