using HarmonyLib;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	internal class EnvironmentObjectSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			for (int i = 0; i < room.TileCount; i++)
			{
				var pos = room.TileAtIndex(i).CenterWorldPosition;
				int max = rng.Next(minAmountPerTile, maxAmountPerTile + 1);
				for (int y = 0; y < max; y++)
				{
					var ts = Instantiate(randomTransforms[rng.Next(randomTransforms.Length)], room.transform);
					ts.transform.position = pos + new Vector3(-offset + (2f * offset * (float)rng.NextDouble()), -offset + (2f * offset * (float)rng.NextDouble()), -offset + (2f * offset * (float)rng.NextDouble()));
					var cell = builder.ec.CellFromPosition(ts.transform.position);
					ts.GetComponentsInChildren<Renderer>().Do(cell.AddRenderer);
				}
			}
		}

		[SerializeField]
		internal float offset = 4f;

		[SerializeField]
		internal int minAmountPerTile = 2, maxAmountPerTile = 4;

		[SerializeField]
		internal Transform[] randomTransforms;
	}
}
