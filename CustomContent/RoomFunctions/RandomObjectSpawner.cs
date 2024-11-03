using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomObjectSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			objectPlacer.Build(builder, room, rng);
		}

		[SerializeField]
		internal ObjectPlacer objectPlacer;

	}
}
