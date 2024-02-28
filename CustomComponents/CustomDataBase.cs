using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomBaseData<E> : MonoBehaviour where E : System.Enum // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
        public Sprite[] storedSprites = [];

        public SoundObject[] soundObjects = [];

        public E myEnum;

        public void GetAudioClips() => soundObjects = GenerateSoundObjects();

		public void SetupPrefab() => SetupPrefabObject();

        protected virtual SoundObject[] GenerateSoundObjects()
        {
            return [];
        }

		protected virtual void SetupPrefabObject() { }
    }
}
