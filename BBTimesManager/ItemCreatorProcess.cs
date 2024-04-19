using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.Registers;
using PixelInternalAPI;

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
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 86 });
			// Banana
			item = CreatorExtensions.CreateItem<Banana, BananaCustomData>("Banana", "BN_Name", "BN_Desc", 45, 25).AddMeta(plug, ItemFlags.Persists | ItemFlags.CreatesEntity).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 75 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 35 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			// present
			item = CreatorExtensions.CreateItem<Present, PresentCustomData>("Present", "PRS_Name", "PRS_Desc", 85, 0).AddMeta(plug, ItemFlags.None).value;
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 35 });
			// gum
			item = CreatorExtensions.CreateItem<GumItem, GumCustomData>("Gum", "GUM_Name", "GUM_Desc", 45, 20).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 10 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			// bell
			item = CreatorExtensions.CreateItem<Bell, BellCustomData>("Bell", "BEL_Name", "BEL_Desc", 25, 25).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 5 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			// headache pill
			item = CreatorExtensions.CreateItem<HeadachePill, HeadachePillCustomData>("Headachepill", "HDP_Name", "HDP_Desc", 45, 30).AddMeta(plug, ItemFlags.None).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });
		}
	}
}
