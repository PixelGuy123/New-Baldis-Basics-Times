using UnityEngine;
using System.Collections.Generic;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class FreezingRoomFunction : RoomFunction
	{
		public override void OnEntityEnter(Entity entity)
		{
			base.OnEntityEnter(entity);
			if (!actMods.Contains(entity.ExternalActivity))
			{
				entity.ExternalActivity.moveMods.Add(moveMod);
				actMods.Add(entity.ExternalActivity);
			}
		}

		public override void OnEntityExit(Entity entity)
		{
			base.OnEntityExit(entity);
			actMods.Remove(entity.ExternalActivity);
			entity.ExternalActivity.moveMods.Remove(moveMod);
		}

		void OnDestroy()
		{
			while (actMods.Count != 0)
			{
				actMods[0]?.moveMods.Remove(moveMod);
				actMods.RemoveAt(0);
			}
		}


		readonly List<ActivityModifier> actMods = [];
		readonly MovementModifier moveMod = new(Vector3.zero, 0.25f);
	}
}
