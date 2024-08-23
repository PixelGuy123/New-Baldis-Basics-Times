using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.CustomItems;
using BepInEx.Bootstrap;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.Registers;
using PixelInternalAPI;
using BBTimes.Plugin;
using MTM101BaldAPI.ObjectCreation;
using BBTimes.CustomContent.Objects;
using BBTimes.CustomContent.Misc;
using MTM101BaldAPI;
using BBTimes.CustomContent.NPCs;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void CreateItems(BaseUnityPlugin plug)
		{
			// ********** Items ************
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			ItemObject item;

		
			// Hammer
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Hammer>()
				.SetGeneratorCost(30)
				.SetShopPrice(205)
				.SetNameAndDescription("HAM_Name", "HAM_Desc")
				.Build<CustomItemData>("Hammer");
			//CreatorExtensions.CreateItem<ITM_Hammer, CustomItemData>("Hammer", "HAM_Name", "HAM_Desc", 125, 30).AddMeta(plug, ItemFlags.None).value;


			floorDatas[0].Items.Add(new() { selection = item, weight = 45});
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 65 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 86 });
			Mugh.AddHittableItem(item.itemType);
			((ITM_Hammer)item.item).item = item.itemType;
			// Present
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Present>()
				.SetGeneratorCost(0)
				.SetShopPrice(245)
				.SetNameAndDescription("PRS_Name", "PRS_Desc")
				.Build<PresentCustomData>("Present");

			// CreatorExtensions.CreateItem<ITM_Present, PresentCustomData>("Present", "PRS_Name", "PRS_Desc", 245, 0).AddMeta(plug, ItemFlags.None).value;

			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 35 });

			// GUM
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Gum>()
				.SetGeneratorCost(20)
				.SetShopPrice(325)
				.SetNameAndDescription("GUM_Name", "GUM_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.Build<GumCustomData>("Gum");

			//CreatorExtensions.CreateItem<ITM_Gum, GumCustomData>("Gum", "GUM_Name", "GUM_Desc", 225, 20).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 10 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			JoeChef.AddFood(item, 25);
			// Bell
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Bell>()
				.SetGeneratorCost(25)
				.SetShopPrice(240)
				.SetNameAndDescription("BEL_Name", "BEL_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.Build<BellCustomData>("Bell");

			//CreatorExtensions.CreateItem<ITM_Bell, BellCustomData>("Bell", "BEL_Name", "BEL_Desc", 155, 25).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 5 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			// headache pill
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_HeadachePill>()
				.SetGeneratorCost(20)
				.SetShopPrice(243)
				.SetNameAndDescription("HDP_Name", "HDP_Desc")
				.Build<HeadachePillCustomData>("Headachepill");
			//CreatorExtensions.CreateItem<ITM_HeadachePill, HeadachePillCustomData>("Headachepill", "HDP_Name", "HDP_Desc", 145, 20).AddMeta(plug, ItemFlags.None).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			if (!Chainloader.PluginInfos.ContainsKey(BasePlugin.CharacterRadarGUID)) // What's the point of the gps when there's already a gps
			{
				// GPS Item
				item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_GPS>()
				.SetGeneratorCost(30)
				.SetShopPrice(500)
				.SetNameAndDescription("GPS_Name", "GPS_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<GpsCustomData>("Gps");

				//CreatorExtensions.CreateItem<ITM_GPS, GpsCustomData>("Gps", "GPS_Name", "GPS_Desc", 185, 30).AddMeta(plug, ItemFlags.Persists).value;
				floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
				floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
				floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
				floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
				floorDatas[2].ShopItems.Add(new() { selection = item, weight = 40 });
				floorDatas[3].ShopItems.Add(new() { selection = item, weight = 75 });
				floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });
				ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			}
			// Golden Quarter
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_GoldenQuarter>()
				.SetGeneratorCost(22)
				.SetShopPrice(750)
				.SetNameAndDescription("gquarter_Name", "gquarter_Desc")
				.Build<CustomItemData>("GoldenQuarter");
			//CreatorExtensions.CreateItem<ITM_GoldenQuarter, CustomItemData>("GoldenQuarter", "gquarter_Name", "gquarter_Desc", 175, 22).AddMeta(plug, ItemFlags.None).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 70 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });

			// BSED
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_StaminaDrinkable>()
				.SetGeneratorCost(20)
				.SetShopPrice(450)
				.SetNameAndDescription("BSED_Name", "BSED_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<CustomItemData>("BSED");
			//CreatorExtensions.CreateItem<ITM_StaminaDrinkable, CustomItemData>("BSED", "BSED_Name", "BSED_Desc", 245, 20).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 48 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 10 });
			((ITM_StaminaDrinkable)item.item).SetMod(1f, 2f, 0.5f);
			((ITM_StaminaDrinkable)item.item).audDrink = man.Get<SoundObject>("audRobloxDrink");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 90 });
			JoeChef.AddFood(item, 55);

			// Speed Potion
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SpeedPotion>()
				.SetGeneratorCost(28)
				.SetShopPrice(600)
				.SetNameAndDescription("SPP_Name", "SPP_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<SpeedPotionCustomData>("SpeedPotion");

			//CreatorExtensions.CreateItem<ITM_SpeedPotion, SpeedPotionCustomData>("SpeedPotion", "SPP_Name", "SPP_Desc", 165, 28).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 36 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 18 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 47 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 56 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 42 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 17 });
			ITM_SpeedPotion.audDrink = man.Get<SoundObject>("audRobloxDrink");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 90 });

			// Pencil
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Pencil>()
				.SetGeneratorCost(20)
				.SetShopPrice(250)
				.SetNameAndDescription("PC_Name", "PC_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<PencilCustomData>("Pencil");
			//CreatorExtensions.CreateItem<ITM_Pencil, PencilCustomData>("Pencil", "PC_Name", "PC_Desc", 255, 20).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 5 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 36 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 18 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 47 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 56 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 17 });
			ITM_Pencil.audStab = man.Get<SoundObject>("audPencilStab");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Water Bottle
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_WaterBottle>()
				.SetGeneratorCost(20)
				.SetShopPrice(350)
				.SetNameAndDescription("WBottle_Name", "WBottle_Desc")
				.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [])
				.Build<CustomItemData>("WaterBottle");
			//CreatorExtensions.CreateItem<ITM_WaterBottle, CustomItemData>("WaterBottle", "WBottle_Name", "WBottle_Desc", 155, 20).AddMeta(plug, ItemFlags.MultipleUse | ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 25 });
			ITM_WaterBottle.audDrink = man.Get<SoundObject>("audRobloxDrink");
			ITM_EmptyWaterBottle.waterBottle = item;
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Empty Water Bottle
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_EmptyWaterBottle>()
				.SetGeneratorCost(18)
				.SetShopPrice(200)
				.SetNameAndDescription("EBottle_Name", "EBottle_Desc")
				.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [])
				.Build<CustomItemData>("EmptyWaterBottle");
			//CreatorExtensions.CreateItem<ITM_EmptyWaterBottle, CustomItemData>("EmptyWaterBottle", "EBottle_Name", "EBottle_Desc", 90, 18).AddMeta(plug, ItemFlags.MultipleUse | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 44 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 105 });
			ITM_WaterBottle.emptyBottle = item;

			// Pogostick
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Pogostick>()
				.SetGeneratorCost(65)
				.SetShopPrice(800)
				.SetNameAndDescription("POGST_Name3", "POGST_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists | ItemFlags.MultipleUse, [])
				.Build<PogostickCustomData>("Pogostick");
			//CreatorExtensions.CreateItem<ITM_Pogostick, PogostickCustomData>("Pogostick", "POGST_Name3", "POGST_Desc", 475, 25);
			var meta = item.GetMeta();
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 20 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			ResourceManager.AddMysteryItem(new() { selection = item, weight = 45 });
			var secondItem = item.DuplicateItem(meta, "POGST_Name2");
			((ITM_Pogostick)item.item).pogoStickReplacement = secondItem;
			((ITM_Pogostick)secondItem.item).pogoStickReplacement = secondItem.DuplicateItem(meta, "POGST_Name1");

			// BearTrap
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Beartrap>()
				.SetGeneratorCost(18)
				.SetShopPrice(350)
				.SetNameAndDescription("BT_Name", "BT_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.Build<BearTrapCustomData>("Beartrap");
			//CreatorExtensions.CreateItem<ITM_Beartrap, BearTrapCustomData>("Beartrap", "BT_Name", "BT_Desc", 335, 18).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 75 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });

			// Basketball
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Basketball>()
				.SetGeneratorCost(35)
				.SetShopPrice(365)
				.SetNameAndDescription("BB_Name", "BB_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.Build<BasketballCustomData>("Basketball");
			//CreatorExtensions.CreateItem<ITM_Basketball, BasketballCustomData>("Basketball", "BB_Name", "BB_Desc", 365, 29).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 10 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 10 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 44 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddMysteryItem(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Hot Chocolate
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_StaminaDrinkable>()
				.SetGeneratorCost(24)
				.SetShopPrice(250)
				.SetNameAndDescription("HotChoc_Name", "HotChoc_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<CustomItemData>("HotChocolate");
			//CreatorExtensions.CreateItem<ITM_StaminaDrinkable, CustomItemData>("HotChocolate", "HotChoc_Name", "HotChoc_Desc", 365, 24).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 55 });
			((ITM_StaminaDrinkable)item.item).SetMod(1f, 0.3f, 0.6f);
			((ITM_StaminaDrinkable)item.item).audDrink = man.Get<SoundObject>("audRobloxDrink");
			((ITM_StaminaDrinkable)item.item).attribute = "hotchocactive";
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });
			JoeChef.AddFood(item, 46);

			// Invisibility Controller
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_InvisibilityController>()
				.SetGeneratorCost(29)
				.SetShopPrice(355)
				.SetNameAndDescription("InvCon_Name", "InvCon_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<InvisibilityControllerCustomData>("InvRemControl");
			//CreatorExtensions.CreateItem<ITM_InvisibilityController, InvisibilityControllerCustomData>("InvRemControl", "InvCon_Name", "InvCon_Desc", 265, 29).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });

			// Screwdriver
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Screwdriver>()
				.SetGeneratorCost(45)
				.SetShopPrice(350)
				.SetNameAndDescription("SD_Name", "SD_Desc")
				.Build<ScrewdriverCustomData>("Screwdriver");
			//CreatorExtensions.CreateItem<ITM_Screwdriver, ScrewdriverCustomData>("Screwdriver", "SD_Name", "SD_Desc", 175, 25).AddMeta(plug, ItemFlags.None).value;
			floorDatas[0].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 32 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 32 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 46 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 54 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 20 });
			floorDatas[0].ForcedItems.Add(item);
			floorDatas[1].ForcedItems.Add(item);
			floorDatas[1].ForcedItems.Add(item);
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			EventMachine.AddItemToTrigger(item.itemType);

			// Hardhat
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_HardHat>()
				.SetGeneratorCost(27)
				.SetShopPrice(250)
				.SetNameAndDescription("Hardhat_Name", "Hardhat_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<HardHatCustomData>("Hardhat");
			//CreatorExtensions.CreateItem<ITM_HardHat, HardHatCustomData>("Hardhat", "Hardhat_Name", "Hardhat_Desc", 250, 27).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 21 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 32 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 22 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 65 });

			// Times icon
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_TimesYTP>()
				.SetGeneratorCost(60)
				.SetShopPrice(9999)
				.SetNameAndDescription("TimesIcon", string.Empty)
				.SetAsInstantUse()
				.Build<GenericYTPItemData>("TimesIcon", Items.Points);

			floorDatas[0].Items.Add(new() { selection = item, weight = 4 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 20 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ForcedItems.Add(item);

			// Cherry Bsoda
			var itemBs = ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value;

			itemBs = itemBs.DuplicateItem("CherryBsoda_Name");

			itemBs.price += 100;
			itemBs.descKey = "CherryBsoda_Desc";
			itemBs.value += 5;

			var cherryCustomData = itemBs.item.gameObject.AddComponent<CherryBsodaCustomData>();
			cherryCustomData.SetupItemData("CherryBsoda", itemBs);
			cherryCustomData.name = "ITM_CherryBsoda";
			var ch = cherryCustomData.GetComponent<ITM_CherryBsoda>();
			ch.time = 30f;

			itemBs.item = ch;
			itemBs.itemType = EnumExtensions.ExtendEnum<Items>("CherryBsoda");
			itemBs.AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists);

			item = itemBs;



			floorDatas[0].Items.Add(new() { selection = item, weight = 5 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 39 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ForcedItems.Add(item);
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 65 });

			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			JoeChef.AddFood(item, 15);

			// Soap Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Soap>()
				.SetGeneratorCost(42)
				.SetShopPrice(450)
				.SetNameAndDescription("Soap_Name", "Soap_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build<SoapCustomData>("Soap");

			floorDatas[1].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 65 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 35 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Coal (useless lmao)
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<Item>() // Item class returns false
				.SetShopPrice(450)
				.SetNameAndDescription("Coal_Name", string.Empty)
				.SetMeta(ItemFlags.Persists | ItemFlags.NoUses, [])
				.Build<CustomItemData>("Coal");

			HappyHolidays.itmCoal = item;

			// Division icon
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_DivideYTP>()
				.SetGeneratorCost(50)
				.SetShopPrice(9999)
				.SetNameAndDescription("DivisionPoint", string.Empty)
				.SetAsInstantUse()
				.Build<GenericYTPItemData>("DivisionPoint", Items.Points);

			floorDatas[0].Items.Add(new() { selection = item, weight = 10 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 30 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[1].ForcedItems.Add(item);

			// Blow Drier Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_BlowDrier>()
				.SetGeneratorCost(45)
				.SetShopPrice(600)
				.SetNameAndDescription("BlowDrier_Name", "BlowDrier_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<BlowDrierCustomData>("BlowDrier");

			floorDatas[1].Items.Add(new() { selection = item, weight = 65 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 75 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 85 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 50 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 80 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 15 });
			ResourceManager.AddMysteryItem(new() { selection = item, weight = 35 });

			// Comically Large Trumpet Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ComicallyLargeTrumpet>()
				.SetGeneratorCost(35)
				.SetShopPrice(400)
				.SetNameAndDescription("ComicLargTrum_Name", "ComicLargTrum_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build<ComicallyLargeTrumpetCustomData>("ComicallyLargeTrumpet");

			floorDatas[0].Items.Add(new() { selection = item, weight = 15 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 45 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 76 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 25 });

			// Magnet Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Magnet>()
				.SetGeneratorCost(35)
				.SetShopPrice(650)
				.SetNameAndDescription("StrMag_Name", "StrMag_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build<MagnetCustomData>("Magnet");

			floorDatas[0].Items.Add(new() { selection = item, weight = 4 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 55 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 45 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 20 });

			// Throwable Teleporter Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ThrowableTeleporter>()
				.SetGeneratorCost(30)
				.SetShopPrice(400)
				.SetNameAndDescription("ThrTelep_Name", "ThrTelep_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build<ThrowableTeleporterCustomData>("ThrowableTeleporter");

			floorDatas[0].Items.Add(new() { selection = item, weight = 10 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 30 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 60 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 70 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 60 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 70 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 55 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Sugar Flavor Zesty Bar Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SugarFlavoredZestyBar>()
				.SetGeneratorCost(55)
				.SetShopPrice(750)
				.SetNameAndDescription("SugZestyBar_Name", "SugZestyBar_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build<SugarFlavoredZestyBarCustomData>("SugarFlavorZestyBar");

			floorDatas[0].Items.Add(new() { selection = item, weight = 12 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 35 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 80 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 70 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });

			// Rotten Cheese Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_RottenCheese>()
				.SetGeneratorCost(70)
				.SetShopPrice(900)
				.SetNameAndDescription("RotCheese_Name", "RotCheese_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build<RottenCheeseCustomData>("RottenCheese");

			floorDatas[0].Items.Add(new() { selection = item, weight = 10 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 18 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 20 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 10 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 10 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			JoeChef.AddFood(item, 15);

			// Soap Bubbles Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SoapBubbles>()
				.SetGeneratorCost(55)
				.SetShopPrice(1100)
				.SetNameAndDescription("SoapBub_Name3", "SoapBub_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity | ItemFlags.MultipleUse, [])
				.Build<SoapBubblesCustomData>("SoapBubbles");

			floorDatas[0].Items.Add(new() { selection = item, weight = 25 });
			floorDatas[1].Items.Add(new() { selection = item, weight = 40 });
			floorDatas[2].Items.Add(new() { selection = item, weight = 60 });
			floorDatas[3].Items.Add(new() { selection = item, weight = 40 });
			floorDatas[0].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[1].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[3].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[1].FieldTripItems.Add(new() { selection = item, weight = 30 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			meta = item.GetMeta();
			secondItem = item.DuplicateItem(meta, "SoapBub_Name2");
			((ITM_SoapBubbles)item.item).itmObjToReplace = secondItem;
			((ITM_SoapBubbles)secondItem.item).itmObjToReplace = secondItem.DuplicateItem(meta, "SoapBub_Name1");
		}
	}
}
