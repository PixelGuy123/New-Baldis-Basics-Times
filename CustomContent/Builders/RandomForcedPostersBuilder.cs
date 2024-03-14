using UnityEngine;
using System.Collections.Generic;

namespace BBTimes.CustomContent.Builders
{
    public class RandomForcedPostersBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			if (posters.Length == 0 || allowedShapes.Count == 0 || chance == 0f)
				return;

			foreach (var c in room.GetTilesOfShape(allowedShapes, false))
				if (c.HasFreeWall && cRng.NextDouble() <= chance)
					ec.BuildPoster(WeightedPosterObject.ControlledRandomSelection(posters, cRng), c, c.RandomUncoveredDirection(cRng));
			

		


		}


		[SerializeField]
		public List<TileShape> allowedShapes = [];

		[SerializeField]
		public WeightedPosterObject[] posters = [];

		[SerializeField]
		public float chance = 0f;
	}
}
