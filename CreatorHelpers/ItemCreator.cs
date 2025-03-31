using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.Object;
using MTM101BaldAPI.ObjectCreation;
using BBTimes.CustomComponents;
using PlusLevelLoader;
using BBTimes.Manager;
using BBTimes.CustomContent.CustomItems;
using System.Xml.Linq;

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

		public static void SetupItemData(this IItemPrefab data, string name, ItemObject itemObj)
		{
			var sprites = GetAllItemSpritesFrom(name); // Get all sprites from its folder (0 is small icon, 1 is big icon)

			itemObj.itemSpriteLarge = sprites[1];
			itemObj.itemSpriteSmall = sprites[0];

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

			var duplicate = item.item.DuplicatePrefab();
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
				Debug.LogWarning("Failed to grab folder: " + path);
				return [];
			}

			string[] files = Directory.GetFiles(path);
			var sprs = new Sprite[files.Length];

			// Pre found ones
			var text = files.First(x => Path.GetFileName(x).StartsWith(BBTimesManager.TimesAssetPrefix + itemBigIconPrefix));
#if CHEAT
			Debug.Log("Item: " + name);
			Debug.Log("Path used for the item sprite selection: " + path);
			Debug.Log("Files found in path: " + files.Length);
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			var tex = AssetLoader.TextureFromFile(text);
			sprs[1] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one * 0.5f, 50f);

			text = files.First(x => Path.GetFileName(x).StartsWith(BBTimesManager.TimesAssetPrefix + itemSmallIconPrefix));
#if CHEAT
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			tex = AssetLoader.TextureFromFile(text);
			sprs[0] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one * 0.5f, 1f);

			return sprs;
		}

		const string itemBigIconPrefix = "bigicon_", itemSmallIconPrefix = "smallicon_";
	}
}
