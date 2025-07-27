using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomItemSpawnFunction : RoomFunction
	{
		[SerializeField]
		internal bool excludeCenterTile = true;

		[SerializeField]
		internal ItemObject itemToSpawn;

		Vector3 positionToSpawnItem = default;

		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			var cells = Room.AllEntitySafeCellsNoGarbage();
			if (excludeCenterTile)
				cells.Remove(room.ec.CellFromPosition(room.ec.RealRoomMid(room))); // Removes middle tile


			if (cells.Count != 0)
				positionToSpawnItem = cells[rng.Next(cells.Count)].FloorWorldPosition;
		}

		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			if (positionToSpawnItem != default)
			{
				room.ec.CreateItem(room, itemToSpawn, new(positionToSpawnItem.x, positionToSpawnItem.z));

				var pickup = room.ec.items[room.ec.items.Count - 1];
				pickup.icon = room.ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
				pickup.AssignItem(itemToSpawn); // Intentionally calls this 'cuz EnvironmentController doesn't seem to do that for some reason
				pickup.Hide(false);
				return;
			}

			Debug.LogWarning("RandomItemSpawnFunction: Position to spawn item was default (no position available found!)");
		}
	}
}
