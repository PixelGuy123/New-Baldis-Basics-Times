﻿using UnityEngine;
using MTM101BaldAPI.AssetTools;
using System.IO;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using BBTimes.CustomComponents;
using System.Linq;
using BBTimes.Manager;

namespace BBTimes.Extensions
{
	public static class CustomDataExtension
	{
		public static bool ReplacesCharacter(this INPCPrefab prefab, Character c) =>
			prefab.GetReplacementNPCs() != null && prefab.GetReplacementNPCs().Contains(c);
		public static string GenerateDataPath(this IPrefab pr, string category, string folder) =>
			Path.Combine(BasePlugin.ModPath, category, pr.Name, folder);
		public static Texture2D GetTexture(this IPrefab pr, string texName) =>
			AssetLoader.TextureFromFile(Path.Combine(pr.TexturePath, BBTimesManager.GetAssetName(texName)));

		public static Sprite GetSprite(this IPrefab pr, float pixelsPerUnit, string texName) =>
			AssetLoader.SpriteFromTexture2D(pr.GetTexture(texName), pixelsPerUnit);

		public static Sprite GetSprite(this IPrefab pr, float pixelsPerUnit, Vector2 center, string texName) =>
			AssetLoader.SpriteFromTexture2D(pr.GetTexture(texName), center, pixelsPerUnit);

		public static Sprite[] GetSpriteSheet(this IPrefab pr, int horizontalTiles, int verticalTiles, float pixelsPerUnit, Vector2 center, string texName) =>
			TextureExtensions.LoadSpriteSheet(horizontalTiles, verticalTiles, pixelsPerUnit, center, pr.TexturePath, BBTimesManager.GetAssetName(texName));

		public static Sprite[] GetSpriteSheet(this IPrefab pr, int horizontalTiles, int verticalTiles, float pixelsPerUnit, string texName) =>
			pr.GetSpriteSheet(horizontalTiles, verticalTiles, pixelsPerUnit, new Vector2(0.5f, 0.5f), texName);

		public static Sprite[] ExcludeNumOfSpritesFromSheet(this Sprite[] array, int spritesToRemove, bool fromEnd = true)
		{
			if (spritesToRemove >= array.Length)
				throw new System.ArgumentException($"spritesToRemove ({spritesToRemove}) is higher than the array length ({array.Length})");

			Sprite[] newAr = new Sprite[array.Length - spritesToRemove];
			int z = fromEnd ? 0 : spritesToRemove; // If fromEnd = false, it should ignore the first sprites, so starting from Start
			for (int i = 0; i < newAr.Length; i++)
				newAr[i] = array[z++];

			return newAr;
		}

		public static Sprite[] MirrorSprites(this Sprite[] array)
		{
			if (array.Length <= 2)
				throw new System.ArgumentException($"Array is too small to be mirrored (size: {array.Length})");

			Sprite[] newAr = new Sprite[array.Length * 2 - 2];
			int i = 0;
			for (; i < array.Length; i++)
				newAr[i] = array[i];
			int z = 1;
			for (; i < newAr.Length; i++)
			{
				newAr[i] = array[array.Length - (1 + z)]; // Mirror sprites!!!
				z++;
			}

			return newAr;
		}

		public static SoundObject GetSound(this IPrefab pr, string audioName, string subtitle, SoundType soundType, Color color) =>
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(pr.SoundPath, BBTimesManager.GetAssetName(audioName))), subtitle, soundType, color);

		public static SoundObject GetSoundNoSub(this IPrefab pr, string audioName, SoundType soundType)
		{
			var s = pr.GetSound(audioName, string.Empty, soundType, Color.white);
			s.subtitle = false;
			return s;
		}
	}
}
