using MTM101BaldAPI.Reflection;
using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		public static E CreateEvent<E>(string name, string eventDescriptionKey, float minEventTime, float maxEventTime, WeightedRoomAsset[] rooms) where E : RandomEvent
		{
			// Object setup
			var ev = new GameObject(name + "_CustomEvent").AddComponent<E>();
			ev.gameObject.SetActive(false);
			DontDestroyOnLoad(ev.gameObject);

			// Fields setup
			ev.ReflectionSetVariable("RandomEventType", EnumExtensions.ExtendEnum<EventType>(name)); // Enum
			ev.ReflectionSetVariable("minEventTime", minEventTime);
			ev.ReflectionSetVariable("maxEventTime", maxEventTime);
			ev.ReflectionSetVariable("potentialRoomAssets", rooms);
			ev.ReflectionSetVariable("eventDescKey", eventDescriptionKey);


			return ev;
		}

		
	}
}
