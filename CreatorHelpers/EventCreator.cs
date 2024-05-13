using UnityEngine;
using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Plugin;
using MTM101BaldAPI.AssetTools;
using System.IO;
using System.Linq;
using MTM101BaldAPI.ObjectCreation;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{

		public static RandomEvent SetupEvent<C>(this RandomEvent ev) where C : CustomEventData
		{
			var cus = ev.gameObject.AddComponent<C>();
			cus.storedSprites = GetAllEventSpritesFrom(ev.name);
			cus.Name = ev.name;
			cus.GetAudioClips();
			cus.SetupPrefab();
			

			return ev;
		}



		static Sprite[] GetAllEventSpritesFrom(string name)
		{
			var path = Path.Combine(BasePlugin.ModPath, "events", name, "Textures");
			if (!Directory.Exists(path))
				return [];

			string[] files = [.. Directory.GetFiles(path).OrderBy(Path.GetFileNameWithoutExtension)]; // Guarantee the order of the files
			var sprs = new Sprite[files.Length];

			for (int i = 0; i < files.Length; i++)
			{
				var ar = Path.GetFileNameWithoutExtension(files[i]).Split('_');
				var tex = AssetLoader.TextureFromFile(files[i]);
				sprs[i] = AssetLoader.SpriteFromTexture2D(tex, Vector2.one / 2f, float.Parse(ar[1]));
				sprs[i].name = ar[0];
			}


			return sprs;
		}

		public static RandomEventBuilder<T> AddRequiredCharacters<T>(this RandomEventBuilder<T> r, params Character[] chars) where T : RandomEvent
		{
			for (int i = 0; i < chars.Length; i++)
				r.AddRequiredCharacter(chars[i]);
			return r;
		}


	}
}
