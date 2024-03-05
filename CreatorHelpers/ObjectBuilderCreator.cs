using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;
using BBTimes.CustomComponents.CustomDatas;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{

		public static O CreateObjectBuilder<O, C>(string obstacleName) where O : ObjectBuilder where C : CustomBaseData
		{
			var obj = new GameObject(obstacleName).AddComponent<O>();
			obj.name = obstacleName;
			obj.obstacle = EnumExtensions.ExtendEnum<Obstacle>(obstacleName);
			DontDestroyOnLoad(obj.gameObject);

			var data = obj.gameObject.AddComponent<C>();
			data.SetupPrefab();

			return obj;
		}
		
	}
}
