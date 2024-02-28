using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		public static ItemObject CreateItem<T, C>(string name, string nameKey, string descKey, int price, int genCost) where T : Item where C : CustomItemData
		{

			var en = EnumExtensions.ExtendEnum<Items>(name); // Make enum


			var itemobj = new GameObject(name + itemSufix, typeof(T)).AddComponent<C>();

			DontDestroyOnLoad(itemobj.gameObject);
			itemobj.gameObject.SetActive(false);
			itemobj.myEnum = en; // custom Item object with customItemData
			var sprites = GetAllItemSpritesFrom(name); // Get all sprites from its folder (0 is small icon, 1 is big icon)
			if (sprites.Length > 2)
				itemobj.storedSprites = [.. sprites.Skip(2)]; // Make sure to not skip and leave an empty array... if that returns an error I guess
			itemobj.GetAudioClips();
			itemobj.SetupPrefab();

			var item = ObjectCreators.CreateItemObject(nameKey, descKey, sprites[0], sprites[1], en, price, genCost);
			item.name = name;
			item.item = itemobj.GetComponent<T>();
			return item;
		}

		static Sprite[] GetAllItemSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "items", name, "Textures");
			if (!Directory.Exists(path))
				return [];

			var files = Directory.GetFiles(path);
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
			sprs[1] = AssetLoader.SpriteFromTexture2D(tex, new((float)tex.width / 2 / tex.width, (float)tex.height / 2 / tex.height), 50f);
			repeatedOnes[0] = text;

			text = files.First(x => Path.GetFileName(x).StartsWith(itemSmallIconPrefix));
#if CHEAT
			Debug.Log("current file: " + Path.GetFileNameWithoutExtension(text));
#endif
			tex = AssetLoader.TextureFromFile(text);
			sprs[0] = AssetLoader.SpriteFromTexture2D(tex, new((float)tex.width / 2 /tex.width, (float)tex.height / 2 / tex.height), 1f);
			repeatedOnes[1] = text;

			// The rest (which also follows up a pattern)
			int z = 2;

			for (int i = 0; i < files.Length; i++)
			{
				if (repeatedOnes.Contains(files[i])) continue; // Skip repeated ones

				var ar = Path.GetFileNameWithoutExtension(files[i]).Split('_');
				tex = AssetLoader.TextureFromFile(files[i]);
				sprs[z] = AssetLoader.SpriteFromTexture2D(tex, new((float)tex.width / 2 / tex.width, (float)tex.height / 2 / tex.height), float.Parse(ar[1]));
				sprs[z].name = ar[0];
				repeatedOnes[z] = files[i];
				z++; // Increment by 1
			}

			return sprs;
		}

		internal const string itemSufix = "_CustomItemInstance";

		const string itemBigIconPrefix = "bigicon_", itemSmallIconPrefix = "smallicon_";
	}
}
