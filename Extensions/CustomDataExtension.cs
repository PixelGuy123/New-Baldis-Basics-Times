using System.IO;
using System.Linq;
using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.OBJImporter;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.Extensions
{
	public static class CustomDataExtension
	{
		const string audioFolderName = "Audios", textureFolderName = "Textures", modelsFolderName = "Models";

		public static bool ReplacesCharacter(this INPCPrefab prefab, Character c) =>
			prefab.GetReplacementNPCs() != null && prefab.GetReplacementNPCs().Contains(c);
		public static Texture2D GetTexture(this IPrefab pr, string texName) =>
			AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, pr.Category, pr.Name, textureFolderName, BBTimesManager.GetAssetName(texName)));

		public static Sprite GetSprite(this IPrefab pr, float pixelsPerUnit, string texName) =>
			AssetLoader.SpriteFromTexture2D(pr.GetTexture(texName), pixelsPerUnit);

		public static Sprite GetSprite(this IPrefab pr, float pixelsPerUnit, Vector2 center, string texName) =>
			AssetLoader.SpriteFromTexture2D(pr.GetTexture(texName), center, pixelsPerUnit);

		public static Sprite[] GetSpriteSheet(this IPrefab pr, int horizontalTiles, int verticalTiles, float pixelsPerUnit, Vector2 center, string texName) =>
			TextureExtensions.LoadSpriteSheet(horizontalTiles, verticalTiles, pixelsPerUnit, center, BasePlugin.ModPath, pr.Category, pr.Name, textureFolderName, BBTimesManager.GetAssetName(texName));

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
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, pr.Category, pr.Name, audioFolderName, BBTimesManager.GetAssetName(audioName))), subtitle, soundType, color);

		public static SoundObject GetSoundNoSub(this IPrefab pr, string audioName, SoundType soundType)
		{
			var s = pr.GetSound(audioName, string.Empty, soundType, Color.white);
			s.subtitle = false;
			return s;
		}
		public static GameObject GetModel(this IPrefab pr, string modelName, bool includeRendererContainer, bool includeMeshCollider, out Transform rendererHolder, Material mat = null) =>
			pr.GetModel(modelName, includeRendererContainer, includeMeshCollider, Vector3.one, out rendererHolder, mat);

		public static GameObject GetModel(this IPrefab pr, string modelName, bool includeRendererContainer, bool includeMeshCollider, Vector3 scale, out Transform rendererHolder, Material mat = null)
		{
			string normalPath = Path.Combine(BasePlugin.ModPath, pr.Category, pr.Name, modelsFolderName, BBTimesManager.GetAssetName(modelName));

			var obj = new OBJLoader().Load(normalPath + ".obj", normalPath + ".mtl", mat ?? ObjectCreationExtension.defaultMaterial);

			obj.transform.localScale = Vector3.one;
			if (includeRendererContainer)
				obj.AddComponent<RendererContainer>().renderers = obj.GetComponentsInChildren<Renderer>();

			if (includeMeshCollider)
				obj.AddComponent<MeshCollider>();

			var childObject = new GameObject("RenderHolder");
			childObject.transform.SetParent(obj.transform);
			childObject.transform.localPosition = Vector3.zero;

			foreach (var child in obj.GetComponentsInChildren<MeshRenderer>())
				child.transform.SetParent(childObject.transform, false);

			childObject.transform.localScale = scale;
			rendererHolder = childObject.transform;

			return obj;
		}
	}
}
