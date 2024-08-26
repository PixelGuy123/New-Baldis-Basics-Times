using UnityEngine;
using BBTimes.Plugin;
using MTM101BaldAPI.AssetTools;
using System.IO;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
    public static class CustomDataExtension
	{
		public static string GenerateDataPath(this GameObject pr, string category, string folder) =>
			Path.Combine(BasePlugin.ModPath, category, pr.name, folder);
		public static Texture2D GetTexture(this IPrefab pr, string texName) =>
			AssetLoader.TextureFromFile(Path.Combine(pr.TexturePath, texName));

		public static Sprite GetSprite(this IPrefab pr, float pixelsPerUnit, string texName) =>
			AssetLoader.SpriteFromTexture2D(pr.GetTexture(texName), pixelsPerUnit);

		public static Sprite GetSprite(this IPrefab pr, float pixelsPerUnit, Vector2 center, string texName) =>
			AssetLoader.SpriteFromTexture2D(pr.GetTexture(texName), center, pixelsPerUnit);

		public static Sprite[] GetSpriteSheet(this IPrefab pr, int horizontalTiles, int verticalTiles, float pixelsPerUnit, Vector2 center, string texName) =>
			TextureExtensions.LoadSpriteSheet(horizontalTiles, verticalTiles, pixelsPerUnit, center, pr.TexturePath, texName);

		public static Sprite[] GetSpriteSheet(this IPrefab pr, int horizontalTiles, int verticalTiles, float pixelsPerUnit, string texName) =>
			pr.GetSpriteSheet(horizontalTiles, verticalTiles, pixelsPerUnit, new Vector2(0.5f, 0.5f), texName);

		public static SoundObject GetSound(this IPrefab pr, string audioName, string subtitle, SoundType soundType, Color color) =>
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(pr.SoundPath, audioName)), subtitle, soundType, color);

		public static SoundObject GetSoundNoSub(this IPrefab pr, string audioName, SoundType soundType)
		{
			var s = pr.GetSound(audioName, string.Empty, soundType, Color.white);
			s.subtitle = false;
			return s;
		}
	}
}
