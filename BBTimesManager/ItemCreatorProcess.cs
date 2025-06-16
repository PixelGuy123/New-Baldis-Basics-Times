using System.Linq;
using BBTimes.CompatibilityModule.EditorCompat;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.Misc;
using BBTimes.CustomContent.NPCs;
using BBTimes.CustomContent.Objects;
using BBTimes.Helpers;
using BBTimes.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using PixelInternalAPI;
using PixelInternalAPI.Extensions;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateItems()
		{
			// ********** Items ************
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			ItemObject item;
			const string
			PIRATE_CANN_HATE = "cann_hate",
			CRIMINALPACK_CONTRABAND = "crmp_contraband",
			FOODTAG = Storage.FOOD_TAG,
			DRINKTAG = Storage.DRINK_TAG;
			LayerMaskObject playerClickLayer = GenericExtensions.FindResourceObjectByName<LayerMaskObject>("PlayerClickLayerMask");

			// Hammer
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Hammer>()
				.SetGeneratorCost(30)
				.SetShopPrice(200)
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.SetNameAndDescription("HAM_Name", "HAM_Desc")
				.Build("Hammer");
			//CreatorExtensions.CreateItem<ITM_Hammer, CustomItemData>("Hammer", "HAM_Name", "HAM_Desc", 125, 30).AddMeta(plug, ItemFlags.None).value;


			floorDatas[F1].Items.Add(new(item, 65));
			floorDatas[F2].Items.Add(new(item, 45));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 25));
			floorDatas[F5].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 65));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 86 });
			Mugh.AddHittableItem(item.itemType);
			// Present
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Present>()
				.SetGeneratorCost(0)
				.SetShopPrice(950)
				.SetMeta(ItemFlags.None, [Storage.ChristmasSpecial_TimesTag])
				.SetNameAndDescription("PRS_Name", "PRS_Desc")
				.Build("Present");

			// CreatorExtensions.CreateItem<ITM_Present, PresentCustomData>("Present", "PRS_Name", "PRS_Desc", 245, 0).AddMeta(plug, ItemFlags.None).value;

			floorDatas[F1].Items.Add(new(item, 5));
			floorDatas[F2].Items.Add(new(item, 10));
			floorDatas[F3].Items.Add(new(item, 15));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));

			// GUM
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Gum>()
				.SetGeneratorCost(20)
				.SetShopPrice(325)
				.SetNameAndDescription("GUM_Name", "GUM_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [FOODTAG])
				.Build("Gum");

			//CreatorExtensions.CreateItem<ITM_Gum, GumCustomData>("Gum", "GUM_Name", "GUM_Desc", 225, 20).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[F1].Items.Add(new(item, 35));
			floorDatas[F2].Items.Add(new(item, 45));
			floorDatas[F3].Items.Add(new(item, 45));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 10 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			JoeChef.AddFood(item, 25);
			// Bell
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Bell>()
				.SetGeneratorCost(25)
				.SetShopPrice(200)
				.SetNameAndDescription("BEL_Name", "BEL_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.Build("Bell");

			//CreatorExtensions.CreateItem<ITM_Bell, BellCustomData>("Bell", "BEL_Name", "BEL_Desc", 155, 25).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[F1].Items.Add(new(item, 25));
			floorDatas[F2].Items.Add(new(item, 55));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			// headache pill
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_HeadachePill>()
				.SetGeneratorCost(20)
				.SetShopPrice(240)
				.SetNameAndDescription("HDP_Name", "HDP_Desc")
				.Build("Headachepill");
			//CreatorExtensions.CreateItem<ITM_HeadachePill, HeadachePillCustomData>("Headachepill", "HDP_Name", "HDP_Desc", 145, 20).AddMeta(plug, ItemFlags.None).value;
			floorDatas[F2].Items.Add(new(item, 55));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// GPS Item
			item = new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_GPS>()
			.SetGeneratorCost(30)
			.SetShopPrice(250)
			.SetNameAndDescription("GPS_Name", "GPS_Desc")
			.SetMeta(ItemFlags.Persists, [])
			.Build("Gps");

			//CreatorExtensions.CreateItem<ITM_GPS, GpsCustomData>("Gps", "GPS_Name", "GPS_Desc", 185, 30).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F2].Items.Add(new(item, 75));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });

			// Golden Quarter
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_GoldenQuarter>()
				.SetGeneratorCost(22)
				.SetShopPrice(750)
				.SetMeta(ItemFlags.MultipleUse, [])
				.SetNameAndDescription("gquarter_Name3", "gquarter_Desc")
				.Build("GoldenQuarter");
			//CreatorExtensions.CreateItem<ITM_GoldenQuarter, CustomItemData>("GoldenQuarter", "gquarter_Name", "gquarter_Desc", 175, 22).AddMeta(plug, ItemFlags.None).value;
			floorDatas[F2].Items.Add(new(item, 45));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 70 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			var meta = item.GetMeta();
			var secondItem = item.DuplicateItem(meta, "gquarter_Name2");
			((ITM_GoldenQuarter)item.item).nextGoldQuart = secondItem;
			((ITM_GoldenQuarter)secondItem.item).nextGoldQuart = secondItem.DuplicateItem(meta, "gquarter_Name1");
			meta.itemObjects = [.. meta.itemObjects.Reverse()];

			// BSED
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_StaminaDrinkable>()
				.SetGeneratorCost(20)
				.SetShopPrice(400)
				.SetNameAndDescription("BSED_Name", "BSED_Desc")
				.SetMeta(ItemFlags.Persists, [DRINKTAG])
				.Build("BSED");
			//CreatorExtensions.CreateItem<ITM_StaminaDrinkable, CustomItemData>("BSED", "BSED_Name", "BSED_Desc", 245, 20).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 48));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 10 });
			((ITM_StaminaDrinkable)item.item).SetMod(1f, 1.8f, 0.75f);
			((ITM_StaminaDrinkable)item.item).SetStaminaIncrease(5f);
			((ITM_StaminaDrinkable)item.item).audDrink = man.Get<SoundObject>("audRobloxDrink");
			((ITM_StaminaDrinkable)item.item).gaugeSprite = item.itemSpriteSmall;
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 90 });
			JoeChef.AddFood(item, 55);

			// Speed Potion
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SpeedPotion>()
				.SetGeneratorCost(28)
				.SetShopPrice(450)
				.SetNameAndDescription("SPP_Name", "SPP_Desc")
				.SetMeta(ItemFlags.Persists, [DRINKTAG, CRIMINALPACK_CONTRABAND])
				.Build("SpeedPotion");

			//CreatorExtensions.CreateItem<ITM_SpeedPotion, SpeedPotionCustomData>("SpeedPotion", "SPP_Name", "SPP_Desc", 165, 28).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F1].Items.Add(new(item, 5));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 50));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 47 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 56 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 42 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 17 });
			ITM_SpeedPotion.audDrink = man.Get<SoundObject>("audRobloxDrink");
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 90 });

			// Pencil
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Pencil>()
				.SetGeneratorCost(20)
				.SetShopPrice(250)
				.SetNameAndDescription("PC_Name", "PC_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("Pencil");
			//CreatorExtensions.CreateItem<ITM_Pencil, PencilCustomData>("Pencil", "PC_Name", "PC_Desc", 255, 20).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F1].Items.Add(new(item, 75));
			floorDatas[F2].Items.Add(new(item, 45));
			floorDatas[F3].Items.Add(new(item, 15));
			floorDatas[END].Items.Add(new(item, 18));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 47 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 56 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 17 });
			ITM_Pencil.audStab = man.Get<SoundObject>("audPencilStab");
			PencilBoy.AddStabbingItem(item.itemType);
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Water Bottle
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_WaterBottle>()
				.SetGeneratorCost(20)
				.SetShopPrice(200)
				.SetNameAndDescription("WBottle_Name", "WBottle_Desc")
				.SetMeta(ItemFlags.MultipleUse | ItemFlags.Persists, [DRINKTAG])
				.Build("WaterBottle");
			//CreatorExtensions.CreateItem<ITM_WaterBottle, CustomItemData>("WaterBottle", "WBottle_Name", "WBottle_Desc", 155, 20).AddMeta(plug, ItemFlags.MultipleUse | ItemFlags.Persists).value;
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Empty Bottle
			var newItem = item.DuplicateItem("EBottle_Name", true, newItemEnumString: "EmptyWaterBottle");
			newItem.descKey = "EBottle_Desc";
			((ITM_WaterBottle)newItem.item).waterBottle = item;

			var iconSprs = CreatorExtensions.GetAllItemSpritesFrom("EmptyBottle");
			newItem.itemSpriteLarge = iconSprs[1];
			newItem.itemSpriteSmall = iconSprs[0];
			newItem.value = 18;
			newItem.price = 100;

			((ITM_WaterBottle)item.item).emptyBottle = newItem;

			// Empty Bottle added to the Generator
			item = newItem;

			floorDatas[F1].Items.Add(new(item, 50));
			floorDatas[F2].Items.Add(new(item, 45));
			floorDatas[F3].Items.Add(new(item, 55));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 44 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 105 });

			// Pogostick
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Pogostick>()
				.SetGeneratorCost(65)
				.SetShopPrice(550)
				.SetNameAndDescription("POGST_Name3", "POGST_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists | ItemFlags.MultipleUse, [CRIMINALPACK_CONTRABAND])
				.Build("Pogostick");
			//CreatorExtensions.CreateItem<ITM_Pogostick, PogostickCustomData>("Pogostick", "POGST_Name3", "POGST_Desc", 475, 25);
			meta = item.GetMeta();
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 20));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			ResourceManager.AddMysteryItem(new() { selection = item, weight = 45 });
			secondItem = item.DuplicateItem(meta, "POGST_Name2");
			((ITM_Pogostick)item.item).pogoStickReplacement = secondItem;
			((ITM_Pogostick)secondItem.item).pogoStickReplacement = secondItem.DuplicateItem(meta, "POGST_Name1");
			meta.itemObjects = [.. meta.itemObjects.Reverse()];

			// BearTrap
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Beartrap>()
				.SetGeneratorCost(18)
				.SetShopPrice(300)
				.SetNameAndDescription("BT_Name", "BT_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [CRIMINALPACK_CONTRABAND])
				.Build("Beartrap");
			//CreatorExtensions.CreateItem<ITM_Beartrap, BearTrapCustomData>("Beartrap", "BT_Name", "BT_Desc", 335, 18).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 75 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });

			// Basketball
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Basketball>()
				.SetGeneratorCost(35)
				.SetShopPrice(350)
				.SetNameAndDescription("BB_Name", "BB_Desc")
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
				.Build("Basketball");
			//CreatorExtensions.CreateItem<ITM_Basketball, BasketballCustomData>("Basketball", "BB_Name", "BB_Desc", 365, 29).AddMeta(plug, ItemFlags.CreatesEntity | ItemFlags.Persists).value;
			floorDatas[F1].Items.Add(new(item, 45));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 10));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 44 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Hot Chocolate
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_HotChocolate>()
				.SetGeneratorCost(24)
				.SetShopPrice(200)
				.SetNameAndDescription("HotChoc_Name", "HotChoc_Desc")
				.SetMeta(ItemFlags.Persists, [DRINKTAG])
				.Build("HotChocolate");
			//CreatorExtensions.CreateItem<ITM_StaminaDrinkable, CustomItemData>("HotChocolate", "HotChoc_Name", "HotChoc_Desc", 365, 24).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F1].Items.Add(new(item, 35));
			floorDatas[F2].Items.Add(new(item, 45));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 55 });
			((ITM_HotChocolate)item.item).SetMod(1f, 0.3f, 0.6f);
			((ITM_HotChocolate)item.item).audDrink = man.Get<SoundObject>("audRobloxDrink");
			((ITM_HotChocolate)item.item).attribute = Storage.HOTCHOCOLATE_ATTR_TAG;
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });
			JoeChef.AddFood(item, 46);

			// Invisibility Controller
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_InvisibilityController>()
				.SetGeneratorCost(29)
				.SetShopPrice(350)
				.SetNameAndDescription("InvCon_Name", "InvCon_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("InvRemControl");
			//CreatorExtensions.CreateItem<ITM_InvisibilityController, InvisibilityControllerCustomData>("InvRemControl", "InvCon_Name", "InvCon_Desc", 265, 29).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });

			// Screwdriver
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Screwdriver>()
				.SetGeneratorCost(45)
				.SetShopPrice(225)
				.SetNameAndDescription("SD_Name", "SD_Desc")
				.Build("Screwdriver");
			//CreatorExtensions.CreateItem<ITM_Screwdriver, ScrewdriverCustomData>("Screwdriver", "SD_Name", "SD_Desc", 175, 25).AddMeta(plug, ItemFlags.None).value;
			floorDatas[F1].Items.Add(new(item, 15));
			floorDatas[F2].Items.Add(new(item, 55));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 32));
			floorDatas[F4].Items.Add(new(item, 35));
			floorDatas[F5].Items.Add(new(item, 35));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 32 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 46 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 54 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 32 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 32 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 20 });
			floorDatas[F1].ForcedItems.Add(new(item));
			floorDatas[F2].ForcedItems.Add(new(item));
			floorDatas[F2].ForcedItems.Add(new(item));
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			EventMachine.AddItemToTrigger(item.itemType);
			OfficeChair.AddFixableItem(item.itemType);
			RollingBot.AddFixableItem(item.itemType);
			ZapZap.AddDeactivator(item.itemType);
			DetentionBot.AddDisablingItem(item.itemType);
			JerryTheAC.AddDisablingItem(item.itemType);
			ItemAlarm.disablingItems.Add(item.itemType);
			MetalWindow.acceptableItems.Add(item.itemType);
			NotebookMachine.unlockableItems.Add(item.itemType);

			// Hardhat
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_HardHat>()
				.SetGeneratorCost(27)
				.SetShopPrice(250)
				.SetNameAndDescription("Hardhat_Name", "Hardhat_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("Hardhat");
			//CreatorExtensions.CreateItem<ITM_HardHat, HardHatCustomData>("Hardhat", "Hardhat_Name", "Hardhat_Desc", 250, 27).AddMeta(plug, ItemFlags.Persists).value;
			floorDatas[F1].ForcedItems.Add(new(item));
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 45));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 21 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 32 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 22 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 65 });

			// Times icon
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_TimesYTP>()
				.SetGeneratorCost(60)
				.SetShopPrice(9999)
				.SetNameAndDescription("TimesIcon", string.Empty)
				.SetAsInstantUse()
				.Build("TimesIcon", Items.Points);

			floorDatas[F1].Items.Add(new(item, 4));
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 20));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F3].ForcedItems.Add(new(item));
			if (EditorExists)
				EditorLevelPatch.AddPoint(item);

			// Cherry Bsoda
			var bsodaMeta = ItemMetaStorage.Instance.FindByEnum(Items.Bsoda);
			var normBsoda = bsodaMeta.value;

			var itemBs = normBsoda.DuplicateItem("CherryBsoda_Name", true);

			itemBs.price += 50;
			itemBs.descKey = "CherryBsoda_Desc";
			itemBs.value += 5;

			var ch = itemBs.item.gameObject.AddComponent<ITM_CherryBsoda>();
			itemBs.item.gameObject.GetComponent<IItemPrefab>().SetupItemData("CherryBsoda", itemBs);
			itemBs.item.name = "ITM_CherryBsoda";

			ch.time = 30f;

			itemBs.item = ch;
			itemBs.itemType = EnumExtensions.ExtendEnum<Items>("CherryBsoda");

			item = itemBs;



			floorDatas[F1].Items.Add(new(item, 5));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 39));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F3].ForcedItems.Add(new(item));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 65 });

			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			JoeChef.AddFood(item, 15);

			// Soap Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Soap>()
				.SetGeneratorCost(42)
				.SetShopPrice(400)
				.SetNameAndDescription("Soap_Name", "Soap_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("Soap");

			floorDatas[F2].Items.Add(new(item, 55));
			floorDatas[F3].Items.Add(new(item, 65));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 35 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Coal (useless lmao)
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<Item>() // Item class returns false
				.SetShopPrice(450)
				.SetNameAndDescription("Coal_Name", string.Empty)
				.SetMeta(ItemFlags.Persists | ItemFlags.NoUses, [PIRATE_CANN_HATE])
				.Build("Coal");

			HappyHolidays.itmCoal = item;

			// Division icon
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_DivideYTP>()
				.SetGeneratorCost(50)
				.SetShopPrice(9999)
				.SetNameAndDescription("DivisionPoint", string.Empty)
				.SetAsInstantUse()
				.Build("DivisionPoint", Items.Points);

			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 30));
			floorDatas[END].Items.Add(new(item, 45));
			floorDatas[F2].ForcedItems.Add(new(item));
			if (EditorExists)
				EditorLevelPatch.AddPoint(item);

			// Blow Drier Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_BlowDrier>()
				.SetGeneratorCost(45)
				.SetShopPrice(550)
				.SetNameAndDescription("BlowDrier_Name", "BlowDrier_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("BlowDrier");

			floorDatas[F2].Items.Add(new(item, 65));
			floorDatas[F3].Items.Add(new(item, 75));
			floorDatas[END].Items.Add(new(item, 45));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 85 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 50 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 80 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 15 });

			// Comically Large Trumpet Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ComicallyLargeTrumpet>()
				.SetGeneratorCost(35)
				.SetShopPrice(400)
				.SetNameAndDescription("ComicLargTrum_Name", "ComicLargTrum_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("ComicallyLargeTrumpet");

			floorDatas[F1].Items.Add(new(item, 15));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 45));
			floorDatas[F4].Items.Add(new(item, 30)); // Loud, disruptive, fits Factory/Laboratory
			floorDatas[F5].Items.Add(new(item, 30)); // Fits Maintenance (noise, disruption)
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 76 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 25 });

			// Magnet Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Magnet>()
				.SetGeneratorCost(35)
				.SetShopPrice(650)
				.SetNameAndDescription("StrMag_Name", "StrMag_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("Magnet");

			floorDatas[F1].Items.Add(new(item, 4));
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 55));
			floorDatas[F4].Items.Add(new(item, 20)); // Magnet fits Factory/Laboratory
			floorDatas[F5].Items.Add(new(item, 20)); // Magnet fits Maintenance (tools, machinery)
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 20 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 20 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 45 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 20 });

			// Throwable Teleporter Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ThrowableTeleporter>()
				.SetGeneratorCost(30)
				.SetShopPrice(250)
				.SetNameAndDescription("ThrTelep_Name", "ThrTelep_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("ThrowableTeleporter");

			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 10));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 55 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Sugar Flavor Zesty Bar Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SugarFlavoredZestyBar>()
				.SetGeneratorCost(55)
				.SetShopPrice(600)
				.SetNameAndDescription("SugZestyBar_Name", "SugZestyBar_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [FOODTAG])
				.Build("SugarFlavorZestyBar");

			floorDatas[F1].Items.Add(new(item, 12));
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 80 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 70 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 55 });

			// Rotten Cheese Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_RottenCheese>()
				.SetGeneratorCost(70)
				.SetShopPrice(450)
				.SetNameAndDescription("RotCheese_Name", "RotCheese_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("RottenCheese");

			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 18));
			floorDatas[F3].Items.Add(new(item, 20));
			floorDatas[END].Items.Add(new(item, 10));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 10 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });
			JoeChef.AddFood(item, 15);

			// Soap Bubbles Item
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SoapBubbles>()
				.SetGeneratorCost(55)
				.SetShopPrice(500)
				.SetNameAndDescription("SoapBub_Name3", "SoapBub_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity | ItemFlags.MultipleUse, [])
				.Build("SoapBubbles");

			floorDatas[F1].Items.Add(new(item, 25));
			floorDatas[F2].Items.Add(new(item, 40));
			floorDatas[F3].Items.Add(new(item, 60));
			floorDatas[END].Items.Add(new(item, 40));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 30 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 45 });
			meta = item.GetMeta();
			secondItem = item.DuplicateItem(meta, "SoapBub_Name2");
			((ITM_SoapBubbles)item.item).itmObjToReplace = secondItem;
			((ITM_SoapBubbles)secondItem.item).itmObjToReplace = secondItem.DuplicateItem(meta, "SoapBub_Name1");
			meta.itemObjects = [.. meta.itemObjects.Reverse()];

			// GSoda
			itemBs = normBsoda.DuplicateItem("GSoda_Name", true);

			itemBs.price += 175;
			itemBs.descKey = "GSoda_Desc";
			itemBs.value += 7;

			var gs = itemBs.item.gameObject.AddComponent<ITM_GSoda>();
			itemBs.item.gameObject.GetComponent<IItemPrefab>().SetupItemData("GSoda", itemBs);
			itemBs.item.name = "ITM_GSoda";

			itemBs.item = gs;
			itemBs.itemType = EnumExtensions.ExtendEnum<Items>("GSoda");

			item = itemBs;
			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 20));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 60 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 50 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 45 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 25 });

			// Beehive
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Beehive>()
				.SetGeneratorCost(50)
				.SetShopPrice(750)
				.SetNameAndDescription("Beehive_Name", "Beehive_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("Beehive");

			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 40));
			floorDatas[END].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 70 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 60 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 8 });

			// SuperCamera
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_SuperCamera>()
				.SetGeneratorCost(45)
				.SetShopPrice(500)
				.SetNameAndDescription("Supercam_Name", "Supercam_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("SuperCamera");

			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 40));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 10));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 50 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 5 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 10 });

			// Sticky Gun
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_PickupGun>()
				.SetGeneratorCost(40)
				.SetShopPrice(200)
				.SetNameAndDescription("Pickupgun_Name", "Pickupgun_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("GrabGun");

			floorDatas[F1].Items.Add(new(item, 45));
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 10 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 10 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 10 });

			// Toilet Paper
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ToiletPaper>()
				.SetGeneratorCost(45)
				.SetShopPrice(150)
				.SetNameAndDescription("Toiletpaper_Name", "Toiletpaper_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("ToiletPaper");

			// You should get toiler paper from a bathroom
			//floorDatas[F1].Items.Add(new() { selection = item, weight = 15 });
			//floorDatas[F2].Items.Add(new() { selection = item, weight = 25 });
			//floorDatas[F3].Items.Add(new() { selection = item, weight = 35 });
			//floorDatas[END].Items.Add(new() { selection = item, weight = 40 });
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 16 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// FidgetSpinner
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_FidgetSpinner>()
				.SetGeneratorCost(55)
				.SetShopPrice(650)
				.SetNameAndDescription("FidgetSpinner_Name", "FidgetSpinner_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("FidgetSpinner");

			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 40));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 16 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 20 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 20 });
			ResourceManager.AddMysteryItem(new() { selection = item, weight = 20 });

			// FryingPan
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_FryingPan>()
				.SetGeneratorCost(48)
				.SetShopPrice(450)
				.SetNameAndDescription("FryingPan_Name", "FryingPan_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("FryingPan");

			floorDatas[F2].Items.Add(new(item, 20));
			floorDatas[F3].Items.Add(new(item, 40));
			floorDatas[END].Items.Add(new(item, 20));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 34 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 22 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 20 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 15 });

			// Electrical Gel
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ElectricalGel>()
				.SetGeneratorCost(48)
				.SetShopPrice(500)
				.SetNameAndDescription("ElectricalGel_Name", "ElectricalGel_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("ElectricalGel");

			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 50));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 34 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 22 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 20 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 15 });

			// Door Stopper
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_DoorStopper>()
				.SetGeneratorCost(35)
				.SetShopPrice(450)
				.SetNameAndDescription("DoorStopper_Name", "DoorStopper_Desc")
				.SetMeta(ItemFlags.Persists, [])
				.Build("DoorStopper");

			floorDatas[F1].Items.Add(new(item, 35));
			floorDatas[F2].Items.Add(new(item, 44));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 15));
			floorDatas[F5].Items.Add(new(item, 15));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 34 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 22 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 20 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// Storm in a Bag
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_StormInABag>()
				.SetGeneratorCost(45)
				.SetShopPrice(650)
				.SetNameAndDescription("StormInABag_Name", "StormInABag_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("StormInABag");

			floorDatas[F2].Items.Add(new(item, 12));
			floorDatas[F3].Items.Add(new(item, 24));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 21 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 26 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 45 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 25 });

			// ChocolateYTP
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_StaminaYTP>()
				.SetGeneratorCost(30)
				.SetShopPrice(9999)
				.SetNameAndDescription("ChocolateYTP", string.Empty)
				.SetAsInstantUse()
				.Build("ChocolateYTP", Items.Points);

			floorDatas[F1].Items.Add(new(item, 5));
			floorDatas[F2].Items.Add(new(item, 10));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 20));
			floorDatas[F1].ForcedItems.Add(new(item));
			if (EditorExists)
				EditorLevelPatch.AddPoint(item);
			((ITM_StaminaYTP)item.item).staminaGain = 200;

			// WaterYTP
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_StaminaYTP>()
				.SetGeneratorCost(30)
				.SetShopPrice(9999)
				.SetNameAndDescription("WaterYTP", string.Empty) // Clear rule for ytps: nameKey should be the same name as the one registered in the editor, for this to work properly
				.SetAsInstantUse()
				.Build("WaterYTP", Items.Points);

			floorDatas[F1].Items.Add(new(item, 10));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 45));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F2].ForcedItems.Add(new(item));
			if (EditorExists)
				EditorLevelPatch.AddPoint(item);

			// SmallTimesKey
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Acceptable>()
				.SetGeneratorCost(99)
				.SetShopPrice(9999)
				.SetNameAndDescription("SmallTimesKey_Name", "SmallTimesKey_Desc")
				.SetMeta(ItemFlags.Persists, [CRIMINALPACK_CONTRABAND])
				.Build("SmallTimesKey");

			floorDatas[F1].ForcedItems.Add(new(item));

			((ITM_Acceptable)item.item).item = item.itemType;
			((ITM_Acceptable)item.item).layerMask = playerClickLayer;

			// Mr Molar
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_MrMolar>()
				.SetGeneratorCost(27)
				.SetShopPrice(500)
				.SetNameAndDescription("MrMolar_Name", "MrMolar_Desc")
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, [])
				.Build("MrMolar");

			floorDatas[F2].Items.Add(new(item, 12));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 10));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 12 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 9 });

			// Cleaning Cloth
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_CleaningCloth>()
				.SetGeneratorCost(25)
				.SetShopPrice(450)
				.SetNameAndDescription("CleaningCloth_Name", "CleaningCloth_Desc")
				.SetMeta(ItemFlags.MultipleUse, [])
				.Build("CleaningCloth");

			floorDatas[F1].Items.Add(new(item, 35));
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 75));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 55 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 35 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 15 });

			// Dirty Cleaning Cloth
			newItem = item.DuplicateItem("DirtyCleaningCloth_Name", true, "DirtyCleaningCloth");
			newItem.descKey = "DirtyCleaningCloth_Desc";
			((ITM_CleaningCloth)newItem.item).cleanClothItem = item;
			((ITM_CleaningCloth)item.item).dirtyClothItem = newItem;
			iconSprs = CreatorExtensions.GetAllItemSpritesFrom("DirtyCleaningCloth");
			newItem.itemSpriteLarge = iconSprs[1];
			newItem.itemSpriteSmall = iconSprs[0];



			// Baldi's Year Book
			new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_BaldiYearbook>()
			.SetGeneratorCost(99)
			.SetShopPrice(25)
			.SetNameAndDescription("BaldiYearbook_Name", "BaldiYearbook_Desc")
			.SetMeta(ItemFlags.Persists | ItemFlags.MultipleUse, [])
			.Build("BaldiYearbook");
			//floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 25 });
			//floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			//floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 25 });

			// Chilly Chilli
			item = new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_ChillyChilli>()
			.SetGeneratorCost(32)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.Persists, [FOODTAG, PIRATE_CANN_HATE])
			.SetNameAndDescription("ChillyChilli_Name", "ChillyChilli_Desc")
			.Build("ChillyChilli");

			floorDatas[F1].Items.Add(new(item, 5));
			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 35));
			floorDatas[F4].Items.Add(new(item, 5));
			floorDatas[F5].Items.Add(new(item, 5));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 40 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 45 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			// Comically Large Jello
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_ComicallyLargeJello>()
				.SetGeneratorCost(38)
				.SetShopPrice(450)
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [FOODTAG, PIRATE_CANN_HATE])
				.SetNameAndDescription("ComicallyLargeJello_Name", "ComicallyLargeJello_Desc")
				.Build("ComicallyLargeJello");

			floorDatas[F1].Items.Add(new(item, 15));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[F4].Items.Add(new(item, 50));
			floorDatas[F5].Items.Add(new(item, 50));
			floorDatas[END].Items.Add(new(item, 25));
			floorDatas[F1].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[F4].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F5].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 76 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 25 });

			// Sketchbook
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Sketchbook>()
				.SetGeneratorCost(48)
				.SetShopPrice(800)
				.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [CRIMINALPACK_CONTRABAND])
				.SetNameAndDescription("Sketchbook_Name", "Sketchbook_Desc")
				.Build("Sketchbook");
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 20));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 50 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });
			ResourceManager.AddMysteryItem(new() { selection = item, weight = 45 });

			// Firework Rocket
			item = new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_FireworkRocket>()
			.SetGeneratorCost(39)
			.SetShopPrice(700)
			.SetMeta(ItemFlags.Persists, [CRIMINALPACK_CONTRABAND])
			.SetNameAndDescription("FireworkRocket_Name", "FireworkRocket_Desc")
			.Build("FireworkRocket");
			floorDatas[F2].Items.Add(new(item, 15));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 20));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 18 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 30 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 15 });

			// Slingshot
			item = new ItemBuilder(plug.Info)
				.SetItemComponent<ITM_Slingshot>()
				.SetGeneratorCost(40)
				.SetShopPrice(700)
				.SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity | ItemFlags.MultipleUse, [CRIMINALPACK_CONTRABAND])
				.SetNameAndDescription("Slingshot_Name3", "Slingshot_Desc")
				.Build("Slingshot");

			floorDatas[F1].Items.Add(new(item, 15));
			floorDatas[F2].Items.Add(new(item, 55));
			floorDatas[F3].Items.Add(new(item, 15));
			floorDatas[END].Items.Add(new(item, 45));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 65 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 55 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 75 });

			meta = item.GetMeta();
			secondItem = item.DuplicateItem(meta, "Slingshot_Name2");
			((ITM_Slingshot)item.item).nextSlingShot = secondItem;
			((ITM_Slingshot)secondItem.item).nextSlingShot = secondItem.DuplicateItem(meta, "Slingshot_Name1");
			meta.itemObjects = [.. meta.itemObjects.Reverse()];

			// Good Grades
			item = new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_GoodGrades>()
			.SetGeneratorCost(30)
			.SetShopPrice(750)
			.SetMeta(ItemFlags.Persists, [])
			.SetNameAndDescription("GoodGrades_Name", "GoodGrades_Desc")
			.Build("GoodGrades");

			floorDatas[F2].Items.Add(new(item, 25));
			floorDatas[F3].Items.Add(new(item, 25));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 15 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 25 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 35 });

			// AAAAH Tomato
			item = new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_AaaahTomato>()
			.SetGeneratorCost(28)
			.SetShopPrice(450)
			.SetMeta(ItemFlags.Persists, [FOODTAG])
			.SetNameAndDescription("AAAHTomato_Name", "AAAHTomato_Desc")
			.Build("AAAHTomato");

			floorDatas[F1].Items.Add(new(item, 45));
			floorDatas[F2].Items.Add(new(item, 35));
			floorDatas[F3].Items.Add(new(item, 45));
			floorDatas[END].Items.Add(new(item, 15));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 50 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 35 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 40 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 60 });

			// Ice Skates
			item = new ItemBuilder(plug.Info)
			.SetItemComponent<ITM_IceSkates>()
			.SetGeneratorCost(30)
			.SetShopPrice(650)
			.SetMeta(ItemFlags.CreatesEntity | ItemFlags.Persists, [])
			.SetNameAndDescription("IceSkates_Name", "IceSkates_Desc")
			.Build("IceSkates");

			floorDatas[F1].Items.Add(new(item, 25));
			floorDatas[F2].Items.Add(new(item, 55));
			floorDatas[F3].Items.Add(new(item, 35));
			floorDatas[END].Items.Add(new(item, 55));
			floorDatas[F2].ShopItems.Add(new() { selection = item, weight = 75 });
			floorDatas[F3].ShopItems.Add(new() { selection = item, weight = 25 });
			floorDatas[END].ShopItems.Add(new() { selection = item, weight = 45 });
			floorDatas[F2].FieldTripItems.Add(new() { selection = item, weight = 15 });
			ResourceManager.AddWeightedItemToCrazyMachine(new() { selection = item, weight = 25 });

		}
	}
}
