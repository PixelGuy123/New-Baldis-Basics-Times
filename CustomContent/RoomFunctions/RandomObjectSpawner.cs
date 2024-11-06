using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomObjectSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			objectPlacer.Build(builder, room, rng);
			objectPlacer.ObjectsPlaced.ForEach(obj => obj.transform.SetParent(room.transform, false));
		}

		[SerializeField]
		internal ObjectPlacer objectPlacer;

	}
}
