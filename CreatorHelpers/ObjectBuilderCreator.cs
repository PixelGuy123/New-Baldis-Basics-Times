using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;
using BBTimes.CustomComponents.CustomDatas;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		public static O CreateObjectBuilder<O, C>(string obstacleName = null) where O : ObjectBuilder where C : CustomBaseData
		{
			var obj = new GameObject(obstacleName).AddComponent<O>();

			if (obstacleName != null)
				obj.obstacle = EnumExtensions.ExtendEnum<Obstacle>(obstacleName);
			else
				obj.obstacle = Obstacle.Null;

			DontDestroyOnLoad(obj.gameObject);

			var data = obj.gameObject.AddComponent<C>();
			data.SetupPrefab();

			return obj;
		}

		public static O CreateObjectBuilder<O>(string obstacleName = null) where O : ObjectBuilder
		{
			var obj = new GameObject(obstacleName).AddComponent<O>();

			if (obstacleName != null)
				obj.obstacle = EnumExtensions.ExtendEnum<Obstacle>(obstacleName);
			else
				obj.obstacle = Obstacle.Null;
			DontDestroyOnLoad(obj.gameObject);

			return obj;
		}

	}
}
