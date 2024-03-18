using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.CustomItems;
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
			item = CreatorExtensions.CreateItem<Hammer, CustomItemData>("Hammer", "HAM_Name", "HAM_Desc", 25, 30).AddMeta(plug, ItemFlags.None).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 45});
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 65 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 85 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 85 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 75 });
		}
	}
}
