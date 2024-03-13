using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomWindowFunction : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			if (window == null)
				throw new System.ArgumentNullException("Missing window from RandomWindowFunction");

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
				room.ec.BuildWindow(cell, cell.RandomUncoveredDirection(rng), window);
				tilesOfShape.RemoveAt(idx);
			}
		}

		[SerializeField]
		public WindowObject window;
	}
}
