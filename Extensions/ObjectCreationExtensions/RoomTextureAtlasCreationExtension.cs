using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static partial class ObjectCreationExtension
	{
		public static Texture2D GenerateTextureAtlas(Texture2D ceil, Texture2D wall, Texture2D floor)
		{
			var textureAtlas = new Texture2D(512, 512, TextureFormat.RGBA32, false)
			{
				filterMode = FilterMode.Point
			};
			var array = MaterialModifier.GetColorsForTileTexture(floor, 256);
			textureAtlas.SetPixels(0, 0, 256, 256, array);
			array = MaterialModifier.GetColorsForTileTexture(wall, 256);
			textureAtlas.SetPixels(256, 256, 256, 256, array);
			array = MaterialModifier.GetColorsForTileTexture(ceil, 256);
			textureAtlas.SetPixels(0, 256, 256, 256, array);
			textureAtlas.Apply();
			return textureAtlas;
		}
	}
}
