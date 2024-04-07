using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class RandomPosterFunction : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			foreach (var poster in posters)
			{
				var cells = room.GetTilesOfShape([TileShape.Single, TileShape.Corner], true);
				for (int i = 0; i < cells.Count; i++)
				{
					if (!cells[i].HasFreeWall)
					{
						cells.RemoveAt(i);
						i--;
					}
				}
				if (cells.Count == 0)
					break;
				var cell = cells[rng.Next(cells.Count)];
				room.ec.BuildPoster(poster, cell, cell.RandomUncoveredDirection(rng));
			}
		}

		[SerializeField]
		public PosterObject[] posters = [];
	}
}
