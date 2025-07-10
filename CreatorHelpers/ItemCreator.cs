using System.IO;
using System.Linq;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using PlusLevelLoader;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
	public static partial class CreatorExtensions
	{
		public static ItemObject Build(this ItemBuilder itmB, string name) =>
			Build(itmB, name, itemsEnum: Items.None);
		public static ItemObject Build(this ItemBuilder itmB, string name, Items itemsEnum = Items.None)
		{

			var en = itemsEnum == Items.None ? EnumExtensions.ExtendEnum<Items>(name) : itemsEnum; // Make enum
			itmB.SetEnum(en);
			var actualItem = itmB.Build();

			actualItem.name = name;
			actualItem.item.gameObject.GetComponent<IItemPrefab>().SetupItemData(name, actualItem);

			BBTimesManager.man.Add("times_itemPrefab_" + name, actualItem.item);
			BBTimesManager.man.Add("times_itemObject_" + name, actualItem);

			return actualItem;
		}

		public static void SetupItemObjectSprites(this ItemObject itemObj, string name)
		{
			var sprites = GetAllItemSpritesFrom(name); // Get all sprites from its folder (0 is small icon, 1 is big icon)

			itemObj.itemSpriteLarge = sprites[1];
			itemObj.itemSpriteSmall = sprites[0];
		}

		public static void SetupItemData(this IItemPrefab data, string name, ItemObject itemObj)
		{
			itemObj.SetupItemObjectSprites(name);

			if (data != null)
			{
				data.ItmObj = itemObj;
				data.Name = name;
				data.SetupPrefab();
				BasePlugin._cstData.Add(data);
			}

			PlusLevelLoaderPlugin.Instance.itemObjects.Add("times_" + name, itemObj);
		}

		public static ItemObject DuplicateItem(this ItemObject item, string nameKey, bool createNewMeta, string newItemEnumString = "", bool registerToLevelLoader = true)
		// Used for making unique items out of others or if they accomplish different functions (like variants)
		{
			var it = Instantiate(item);
			it.nameKey = nameKey;
			it.name = Singleton<LocalizationManager>.Instance.GetLocalizedText(nameKey)
				.Split(' ') // Remove any spaces
				.Join(delimiter: string.Empty); // Put all together into a single string

			Item duplicate = item.item.SafeDuplicatePrefab(true);
			it.item = duplicate;
			it.item.name = it.name;

			if (createNewMeta)
			{
				var ogMeta = item.GetMeta();
				ItemMetaData itemMetaData = new(BBTimesManager.plug.Info, it);
				itemMetaData.tags.AddRange(ogMeta.tags);
				itemMetaData.flags = ogMeta.flags;
				it.AddMeta(itemMetaData);
			}

			bool newEnum = !string.IsNullOrEmpty(newItemEnumString);

			if (newEnum)
				it.itemType = EnumExtensions.ExtendEnum<Items>(newItemEnumString);

			if (registerToLevelLoader)
				PlusLevelLoaderPlugin.Instance.itemObjects.Add("times_" + (newEnum ? newItemEnumString : it.name), it);

			return it;
		}

		public static ItemObject DuplicateItem(this ItemObject item, ItemMetaData data, string nameKey) // Only used when the item has MULTIPLE USE (like grappling hooks)
		{
			var it = DuplicateItem(item, nameKey, false, registerToLevelLoader: false);
			data.itemObjects = data.itemObjects.AddToArray(it);

			it.AddMeta(data);

			return it;
		}

		public static Sprite[] GetAllItemSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "items", name, "Textures");
			if (!Directory.Exists(path))
			{
				throw new DirectoryNotFoundException("Failed to grab folder: " + path);
			}

			string[] files = Directory.GetFiles(path);
			var sprs = new Sprite[2]; // Only two icons: small and big
			bool foundSmall = false, foundBig = false;

			// Get first the small icon; it's ALWAYS required
			var text = files.FirstOrDefault(x => Path.GetFileName(x).StartsWith(BBTimesManager.TimesAssetPrefix + itemSmallIconPrefix));

			if (!string.IsNullOrEmpty(text))
			{
				sprs[0] = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(text), Vector2.one * 0.5f, 50f);
				foundSmall = true;
			}

			// Then, get big icon
			text = files.FirstOrDefault(x => Path.GetFileName(x).StartsWith(BBTimesManager.TimesAssetPrefix + itemBigIconPrefix));

			if (!string.IsNullOrEmpty(text))
			{
				sprs[1] = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(text), Vector2.one * 0.5f, 50f);
				foundBig = true;
			}

			if (!foundSmall && foundBig)
				sprs[0] = sprs[1]; // Equalize both as big
			else if (foundSmall && !foundBig)
				sprs[1] = sprs[0]; // Equalize both as small
			else if (!foundSmall && !foundBig) // if both are false
				throw new System.InvalidOperationException("No big or small icon has been found for item: " + name);

			return sprs;
		}

		const string itemBigIconPrefix = "bigicon_", itemSmallIconPrefix = "smallicon_";
	}
}
