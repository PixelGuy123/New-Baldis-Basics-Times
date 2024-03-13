using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static partial class ObjectCreationExtension
	{

		public static Cubemap CubemapFromTexture2D(Texture2D texture)
		{
			int cubemapWidth = texture.height / 6;
			Cubemap cubemap = new(cubemapWidth, TextureFormat.ARGB32, false);
			cubemap.SetPixels(texture.GetPixels(0, 0 * cubemapWidth, cubemapWidth, cubemapWidth), CubemapFace.NegativeZ);
			cubemap.SetPixels(texture.GetPixels(0, 1 * cubemapWidth, cubemapWidth, cubemapWidth), CubemapFace.PositiveZ);
			cubemap.SetPixels(texture.GetPixels(0, 2 * cubemapWidth, cubemapWidth, cubemapWidth), CubemapFace.PositiveY);
			cubemap.SetPixels(texture.GetPixels(0, 3 * cubemapWidth, cubemapWidth, cubemapWidth), CubemapFace.NegativeY);
			cubemap.SetPixels(texture.GetPixels(0, 4 * cubemapWidth, cubemapWidth, cubemapWidth), CubemapFace.NegativeX);
			cubemap.SetPixels(texture.GetPixels(0, 5 * cubemapWidth, cubemapWidth, cubemapWidth), CubemapFace.PositiveX);
			cubemap.Apply();
			return cubemap;
		}
	}
}
