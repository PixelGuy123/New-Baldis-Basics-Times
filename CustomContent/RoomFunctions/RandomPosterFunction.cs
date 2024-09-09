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
				var cells = room.AllTilesNoGarbage(false, false);
				for (int i = 0; i < cells.Count; i++)
					if (cells[i].shape != TileShape.Single && cells[i].shape != TileShape.Corner)
						cells.RemoveAt(i--);


				if (cells.Count == 0)
					return;

				while (cells.Count != 0)
				{
					int idx = rng.Next(cells.Count);
					var dirs = cells[idx].AllWallDirections;
					if (dirs.Count != 0)
					{
						room.ec.BuildPoster(poster, cells[idx], dirs[rng.Next(dirs.Count)]);
						break;
					}
					cells.RemoveAt(idx);
				}
			}

		}

		[SerializeField]
		internal PosterObject[] posters = [];
	}
}
