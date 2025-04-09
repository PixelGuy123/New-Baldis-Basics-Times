using BBTimes.CustomContent.Misc;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
    public class IceWaterFunction : RoomFunction
    {
		[SerializeField]
		internal IceRinkWater waterPre;

		[SerializeField]
		internal int minWaterCount = 3, maxWaterCount = 7;

		readonly List<IceRinkWater> createdWaterObjs = [], potentialWaterObjs = [];

		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			var safeCells = room.AllEntitySafeCellsNoGarbage();
			if (safeCells.Count == 0)
			{
				Debug.LogWarning("IceWaterFunction: failed to find any good spots for IceRinkWater");
				return;
			}

			int count = Random.Range(minWaterCount, maxWaterCount + 1);
			for (int i = 0; i < count; i++)
			{
				if (safeCells.Count == 0)
					return;

				int num = Random.Range(0, safeCells.Count);

				var water = Instantiate(waterPre, room.objectObject.transform);
				water.transform.position = safeCells[num].FloorWorldPosition;
				water.Initialize(this, room.ec);
				createdWaterObjs.Add(water);

				room.entitySafeCells.Remove(safeCells[num].position);
				room.eventSafeCells.Remove(safeCells[num].position);
				safeCells.RemoveAt(num);
			}
		}

		public IceRinkWater GetPotentialSpot(IceRinkWater water)
		{
			potentialWaterObjs.Clear();
			for (int i = 0; i < createdWaterObjs.Count; i++)
				if (createdWaterObjs[i] != water)
					potentialWaterObjs.Add(createdWaterObjs[i]);
			return potentialWaterObjs.Count != 0 ? potentialWaterObjs[Random.Range(0, potentialWaterObjs.Count)] : null;
		}

	}
}
