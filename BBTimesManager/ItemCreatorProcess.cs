using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomItems;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.Registers;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void CreateItems(BaseUnityPlugin plug)
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			ItemObject item;
			// Hammer
			item = CreatorExtensions.CreateItem<Hammer, CustomItemData>("Hammer", "HAM_Name", "HAM_Desc", 25, 30).AddMeta(plug, ItemFlags.NoUses).value;

			floorDatas[0].Items.Add(new()
			{
				selection = item,
				weight = 1000
			});
		}
	}
}
