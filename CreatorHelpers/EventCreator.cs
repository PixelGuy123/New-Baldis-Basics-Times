using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;
using System.Reflection;
using HarmonyLib;

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

		public static E CreateEvent<E>(string name, string eventDescriptionKey, float minEventTime, float maxEventTime, WeightedRoomAsset[] rooms) where E : RandomEvent
		{
			// Object setup
			var ev = new GameObject(name + "_CustomEvent").AddComponent<E>();
			ev.gameObject.SetActive(false);
			DontDestroyOnLoad(ev.gameObject);

			// Fields setup
			_ev_randomEventType.SetValue(ev, EnumExtensions.ExtendEnum<RandomEventType>(name)); // Enum
			_ev_minEventTime.SetValue(ev, minEventTime);
			_ev_maxEventTime.SetValue(ev, maxEventTime);
			_ev_potentialRoomAssets.SetValue(ev, rooms);
			_ev_eventDescKey.SetValue(ev, eventDescriptionKey);


			return ev;
		}

		
	}
}
