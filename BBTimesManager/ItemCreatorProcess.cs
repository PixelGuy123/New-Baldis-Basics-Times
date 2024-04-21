using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.CustomItems;
using BepInEx.Bootstrap;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.Registers;
using PixelInternalAPI;
using BBTimes.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

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
			item = CreatorExtensions.CreateItem<ITM_Hammer, CustomItemData>("Hammer", "HAM_Name", "HAM_Desc", 25, 30).AddMeta(plug, ItemFlags.None).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 45});
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 65 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 85 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 85 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 75 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 86 });
			// Banana
			item = CreatorExtensions.CreateItem<ITM_Banana, BananaCustomData>("Banana", "BN_Name", "BN_Desc", 45, 25).AddMeta(plug, ItemFlags.Persists | ItemFlags.CreatesEntity).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 75 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 35 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			// present
			item = CreatorExtensions.CreateItem<ITM_Present, PresentCustomData>("Present", "PRS_Name", "PRS_Desc", 85, 0).AddMeta(plug, ItemFlags.None).value;
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 35 });
			// gum
			item = CreatorExtensions.CreateItem<ITM_Gum, GumCustomData>("Gum", "GUM_Name", "GUM_Desc", 45, 20).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
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
			item = CreatorExtensions.CreateItem<ITM_Bell, BellCustomData>("Bell", "BEL_Name", "BEL_Desc", 25, 25).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
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
			item = CreatorExtensions.CreateItem<ITM_HeadachePill, HeadachePillCustomData>("Headachepill", "HDP_Name", "HDP_Desc", 45, 20).AddMeta(plug, ItemFlags.None).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });
			if (!Chainloader.PluginInfos.ContainsKey(BasePlugin.CharacterRadarGUID)) // What's the point of the gps when there's already a gps
			{
				// GPS Item
				item = CreatorExtensions.CreateItem<ITM_GPS, GpsCustomData>("Gps", "GPS_Name", "GPS_Desc", 65, 30).AddMeta(plug, ItemFlags.Persists).value;
				floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
				floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
				floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
				floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
				floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
				floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });
				ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			}
			// Golden Quarter
			item = CreatorExtensions.CreateItem<ITM_GoldenQuarter, CustomItemData>("GoldenQuarter", "gquarter_Name", "gquarter_Desc", 75, 22).AddMeta(plug, ItemFlags.None).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });

			// BSED
			// Future note: make a vending machine from this
			item = CreatorExtensions.CreateItem<ITM_BSED, CustomItemData>("BSED", "BSED_Name", "BSED_Desc", 45, 20).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 48 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 10 });
			ITM_BSED.audDrink = man.Get<SoundObject>("audRobloxDrink");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 90 });

			// Speed Potion
			item = CreatorExtensions.CreateItem<ITM_SpeedPotion, SpeedPotionCustomData>("SpeedPotion", "SPP_Name", "SPP_Desc", 65, 28).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 36 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 18 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 47 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 56 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 17 });
			ITM_SpeedPotion.audDrink = man.Get<SoundObject>("audRobloxDrink");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 90 });

			// Pencil
			item = CreatorExtensions.CreateItem<ITM_Pencil, PencilCustomData>("Pencil", "PC_Name", "PC_Desc", 55, 20).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 5 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 36 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 18 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 47 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 56 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 17 });
			ITM_Pencil.audStab = man.Get<SoundObject>("audPencilStab");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Water Bottle
			item = CreatorExtensions.CreateItem<ITM_WaterBottle, CustomItemData>("WaterBottle", "WBottle_Name", "WBottle_Desc", 55, 20).AddMeta(plug, ItemFlags.MultipleUse | ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 45 });
			ITM_WaterBottle.audDrink = man.Get<SoundObject>("audRobloxDrink");
			ITM_EmptyWaterBottle.waterBottle = item;
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Empty Water Bottle
			item = CreatorExtensions.CreateItem<ITM_EmptyWaterBottle, CustomItemData>("EmptyWaterBottle", "EBottle_Name", "EBottle_Desc", 35, 18).AddMeta(plug, ItemFlags.MultipleUse | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 55 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 105 });
			ITM_WaterBottle.emptyBottle = item;

			// Pogostick
			item = CreatorExtensions.CreateItem<ITM_Pogostick, PogostickCustomData>("Pogostick", "POGST_Name3", "POGST_Desc", 75, 25);
			var meta = item.AddMeta(plug, ItemFlags.MultipleUse | ItemFlags.Persists | ItemFlags.CreatesEntity);
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 20 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 30 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			var secondItem = item.DuplicateItem(meta, "POGST_Name2");
			((ITM_Pogostick)item.item).pogoStickReplacement = secondItem;
			((ITM_Pogostick)secondItem.item).pogoStickReplacement = secondItem.DuplicateItem(meta, "POGST_Name1");

			// BearTrap
			item = CreatorExtensions.CreateItem<ITM_Beartrap, BearTrapCustomData>("Beartrap", "BT_Name", "BT_Desc", 35, 18).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });


		}
	}
}
