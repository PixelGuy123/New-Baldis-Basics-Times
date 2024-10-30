using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomObjectSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			var cells = room.GetTilesOfShape(shapes, coverage, includeOpen);
			if (cells.Count == 0)
			{
				Debug.LogWarning("BBTimes: RandomObjectSpawner failed to find any spot for room: " + room.name);
				return;
			}

			int max = (int)((room.size.x + room.size.z) * transformsPerSizeFactor);

			for (int i = 0; i < max; i++)
			{
				if (cells.Count == 0)
					return;

				int idx = rng.Next(cells.Count);
				var cell = cells[idx];
				cells.RemoveAt(idx);

				var transform = Instantiate(WeightedTransform.ControlledRandomSelection(transformsPre, rng), this.transform);
				transform.position = cell.FloorWorldPosition + 
					new Vector3(-horizontalOffset + (2f * horizontalOffset * (float)rng.NextDouble()), -verticalOffset + (2f * verticalOffset * (float)rng.NextDouble()), -horizontalOffset + (2f * horizontalOffset * (float)rng.NextDouble()));

			}
		}

		[SerializeField]
		internal List<TileShape> shapes = [TileShape.Open, TileShape.Corner, TileShape.Single];

		[SerializeField]
		internal bool includeOpen = true;

		[SerializeField]
		internal CellCoverage coverage = CellCoverage.None;

		[SerializeField]
		internal WeightedTransform[] transformsPre;

		[SerializeField]
		internal float horizontalOffset = 0f, verticalOffset = 0f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float transformsPerSizeFactor = 0.25f;

	}
}
