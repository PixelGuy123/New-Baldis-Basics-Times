using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
    public class RandomItemSpawnFunction : RoomFunction
    {
		[SerializeField]
		internal bool excludeCenterTile = true;

		[SerializeField]
		internal ItemObject itemToSpawn;

		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			var cells = Room.AllEntitySafeCellsNoGarbage();
			if (excludeCenterTile)
			{
				var middleCell = room.ec.CellFromPosition(room.ec.RealRoomMid(room));
				cells.Remove(middleCell);
			}

			if (cells.Count != 0)
			{
				var pos = cells[rng.Next(cells.Count)].FloorWorldPosition;
				var anchorPos = room.ec.RealRoomMin(room);
				builder.CreateItem(room, itemToSpawn, new(pos.x - anchorPos.x, pos.z - anchorPos.z));
				return;
			}

			Debug.LogWarning("RandomItemSpawnFunction: Failed to find good spot to spawn item.");

		}
	}
}
