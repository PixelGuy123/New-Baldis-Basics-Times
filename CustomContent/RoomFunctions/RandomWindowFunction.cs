using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomWindowFunction : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			if (window == null)
				return;

			base.Build(builder, rng);
			List<Cell> tilesOfShape = room.GetTilesOfShape([TileShape.Single, TileShape.Corner], true);
			for (int i = 0; i < tilesOfShape.Count; i++)
			{
				if (!tilesOfShape[i].HasFreeWall)
				{
					tilesOfShape.RemoveAt(i);
					i--;
				}
			}
			int amount = rng.Next(room.size.x + room.size.z / 6, room.size.x + room.size.z / 3 + 1);
			for (int i = 0; i < amount; i++)
			{
				if (tilesOfShape.Count == 0) break;

				int idx = rng.Next(0, tilesOfShape.Count);
				Cell cell = tilesOfShape[idx];
				var dir = cell.RandomUncoveredDirection(rng);
				if (builder.Ec.CellFromPosition(cell.position + dir.ToIntVector2()).WallHardCovered(dir.GetOpposite())) // If it is hard covered at the other side, no window should spawn in there then
				{
					tilesOfShape.RemoveAt(idx);
					i--;
					continue;
				}
				room.ec.BuildWindow(cell, dir, window);
				tilesOfShape.RemoveAt(idx);
			}
		}

		[SerializeField]
		public WindowObject window;
	}
}
