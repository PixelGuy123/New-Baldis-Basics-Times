using UnityEngine;
using BBTimes.Plugin;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomBaseData : MonoBehaviour // A basic "mutable" class just for the sole purpose of storing extra info for items
	{
			
		

        public Sprite[] storedSprites = [];

        public SoundObject[] soundObjects = [];

        public void GetAudioClips() => soundObjects = GenerateSoundObjects();

        protected virtual SoundObject[] GenerateSoundObjects()
        {
            return [];
        }

		public virtual void SetupPrefab()
		{
			BasePlugin._cstData.Add(this);
		}

		public virtual void SetupPrefabPost() { } // This one is triggered when every mod initialization is completed (to grab stuff like items for example)

		public string Name { get; internal set; } = string.Empty;
    }
}
