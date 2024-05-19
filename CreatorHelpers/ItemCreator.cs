using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Plugin;
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

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		public static ItemObject Build<C>(this ItemBuilder itmB, string name) where C : CustomItemData =>
			Build<C>(itmB, name, itemsEnum: Items.None);
		public static ItemObject Build<C>(this ItemBuilder itmB, string name, Items itemsEnum = Items.None) where C : CustomItemData
		{

			var en = itemsEnum == Items.None ? EnumExtensions.ExtendEnum<Items>(name) : itemsEnum; // Make enum

			itmB.SetEnum(en);
			var actualItem = itmB.Build();

			var item = actualItem.item;

			var itemobj = item.gameObject.AddComponent<C>();
			itemobj.myEnum = en; // custom Item object with customItemData

			itemobj.Name = name;

			var sprites = GetAllItemSpritesFrom(name); // Get all sprites from its folder (0 is small icon, 1 is big icon)

			actualItem.itemSpriteLarge = sprites[1];
			actualItem.itemSpriteSmall = sprites[0];


			itemobj.GetAudioClips();
			itemobj.GetSprites();
			itemobj.SetupPrefab();
			

			return actualItem;
		}

		public static ItemObject DuplicateItem(this ItemObject item, ItemMetaData data, string nameKey)
		{
			var it = Instantiate(item);
			it.nameKey = nameKey;
			it.name = "ItmObj_" + nameKey;

			var duplicate = item.item.DuplicatePrefab();
			it.item = duplicate;
			data.itemObjects = data.itemObjects.AddToArray(it);
			it.AddMeta(data);

			return it;
		}

		static Sprite[] GetAllItemSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "items", name, "Textures");
			if (!Directory.Exists(path))
			{
				Debug.LogWarning("Failed to grab folder: " + path);
				return [];
			}

			string[] files = Directory.GetFiles(path);
			var sprs = new Sprite[files.Length];
			string[] repeatedOnes = new string[files.Length];

			// Pre found ones
			var text = files.First(x => Path.GetFileName(x).StartsWith(itemBigIconPrefix));
#if CHEAT
			Debug.Log("Item: " + name);
			Debug.Log("Path used for the item sprite selection: " + path);
			Debug.Log("Files found in path: " + files.Length);
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			var tex = AssetLoader.TextureFromFile(text);
			sprs[1] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, 50f);
			repeatedOnes[0] = text;

			text = files.First(x => Path.GetFileName(x).StartsWith(itemSmallIconPrefix));
#if CHEAT
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			tex = AssetLoader.TextureFromFile(text);
			sprs[0] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, 1f);
			repeatedOnes[1] = text;

			return sprs;
		}

		const string itemBigIconPrefix = "bigicon_", itemSmallIconPrefix = "smallicon_";
	}
}
