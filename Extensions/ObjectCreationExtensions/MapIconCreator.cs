using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static partial class ObjectCreationExtension
	{
		public static T CreateMapIcon<T>(Texture2D tex, string name = "MapIconPrefab") where T : MapIcon
		{
			var icon = new GameObject(name).AddComponent<T>();
			icon.spriteRenderer = icon.gameObject.AddComponent<SpriteRenderer>();
			icon.spriteRenderer.material = new(mapMaterial);
			icon.spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(tex, defaultMapIconPixelsPerUnit);

			icon.gameObject.ConvertToPrefab(true);
			icon.gameObject.layer = LayerStorage.map;
			return icon;
		}

		internal const float defaultMapIconPixelsPerUnit = 16f;
	}
}
