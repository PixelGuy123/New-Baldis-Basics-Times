using MTM101BaldAPI;
using UnityEngine;
using BBTimes.Plugin;
using BBTimes.CustomComponents;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{
		public static O CreateObjectBuilder<O>(string name, string obstacleName = null) where O : ObjectBuilder
		{
			var obj = new GameObject(name).AddComponent<O>();

			if (obstacleName != null)
				obj.obstacle = EnumExtensions.ExtendEnum<Obstacle>(obstacleName);
			else
				obj.obstacle = Obstacle.Null;

			obj.gameObject.ConvertToPrefab(true);
			var data = obj.GetComponent<IObjectPrefab>();

			data.Name = obstacleName;
			data.SetupPrefab();
			BasePlugin._cstData.Add(data);

			return obj;
		}

		public static O CreateObjectBuilder<O>(string obstacleName = null) where O : ObjectBuilder
		{
			var obj = new GameObject(obstacleName ?? typeof(O).Name).AddComponent<O>();

			if (obstacleName != null)
				obj.obstacle = EnumExtensions.ExtendEnum<Obstacle>(obstacleName);
			else
				obj.obstacle = Obstacle.Null;
			obj.gameObject.ConvertToPrefab(true);

			return obj;
		}

	}
}
