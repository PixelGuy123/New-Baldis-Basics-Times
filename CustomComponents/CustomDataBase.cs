using UnityEngine;
using BBTimes.Plugin;
using MTM101BaldAPI.AssetTools;
using System.IO;
using MTM101BaldAPI;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomBaseData : MonoBehaviour // A basic "mutable" class just for the sole purpose of storing extra info for items
	{
			
		

        public Sprite[] storedSprites = [];

        public SoundObject[] soundObjects = [];

        public void GetAudioClips() => soundObjects = GenerateSoundObjects();

		protected virtual SoundObject[] GenerateSoundObjects() =>
			[];

		public void GetSprites() =>
			storedSprites = GenerateSpriteOrder();


		protected virtual Sprite[] GenerateSpriteOrder() =>
			[];

		protected Texture2D GetTexture(string texName) =>
			AssetLoader.TextureFromFile(Path.Combine(TexturePath, texName));

		protected Sprite GetSprite(float pixelsPerUnit, string texName) =>
			AssetLoader.SpriteFromTexture2D(GetTexture(texName), pixelsPerUnit);

		protected Sprite GetSprite(float pixelsPerUnit, Vector2 center, string texName) =>
			AssetLoader.SpriteFromTexture2D(GetTexture(texName), center, pixelsPerUnit);

		protected SoundObject GetSound(string audioName, string subtitle, SoundType soundType, Color color) =>
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, audioName)), subtitle, soundType, color);

		protected SoundObject GetSoundNoSub(string audioName, SoundType soundType)
		{
			var s = GetSound(audioName, string.Empty, soundType, Color.white);
			s.subtitle = false;
			return s;
		}

		public virtual void SetupPrefab()
		{
			BasePlugin._cstData.Add(this);
		}

		public void PostPrefabSetup()
		{
			SetupPrefabPost();
			storedSprites = null;
			soundObjects = null; // Should be cleaned up by GC
		}
		protected virtual void SetupPrefabPost() 
		{
		} // This one is triggered when every mod initialization is completed (to grab stuff like items for example)

		public string Name { get; internal set; } = string.Empty;

		protected virtual string SoundPath => string.Empty;

		protected virtual string TexturePath => string.Empty;
	}
}
