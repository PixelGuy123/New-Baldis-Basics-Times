using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;
using System.Reflection;
using HarmonyLib;
using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Plugin;
using MTM101BaldAPI.AssetTools;
using System.IO;
using System.Linq;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		// Fields
		readonly static FieldInfo _ev_randomEventType = AccessTools.Field(typeof(RandomEvent), "eventType");
		readonly static FieldInfo _ev_minEventTime = AccessTools.Field(typeof(RandomEvent), "minEventTime");
		readonly static FieldInfo _ev_maxEventTime = AccessTools.Field(typeof(RandomEvent), "maxEventTime");
		readonly static FieldInfo _ev_potentialRoomAssets = AccessTools.Field(typeof(RandomEvent), "potentialRoomAssets");
		readonly static FieldInfo _ev_eventDescKey = AccessTools.Field(typeof(RandomEvent), "eventDescKey");

		public static E CreateEvent<E, C>(string name, string eventDescriptionKey, float minEventTime, float maxEventTime, WeightedRoomAsset[] rooms) where E : RandomEvent where C : CustomEventData
		{
			// Object setup
			var ev = new GameObject(name).AddComponent<E>();
			ev.gameObject.SetActive(false);
			DontDestroyOnLoad(ev.gameObject);

			// Fields setup
			_ev_randomEventType.SetValue(ev, EnumExtensions.ExtendEnum<RandomEventType>(name)); // Enum
			_ev_minEventTime.SetValue(ev, minEventTime);
			_ev_maxEventTime.SetValue(ev, maxEventTime);
			_ev_potentialRoomAssets.SetValue(ev, rooms);
			_ev_eventDescKey.SetValue(ev, eventDescriptionKey);

			var cus = ev.gameObject.AddComponent<C>();
			cus.storedSprites = GetAllEventSpritesFrom(name);
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


	}
}
