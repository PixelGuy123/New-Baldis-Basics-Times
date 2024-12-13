using UnityEngine;
using BBTimes.CustomComponents;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.Builders
{
    public class RandomForcedPostersBuilder : StructureBuilder, IBuilderPrefab
	{
		public StructureWithParameters SetupBuilderPrefabs() =>
			new() { parameters = new() { chance = [0f] }, prefab = this }; // Can be null, right?
		public void SetupPrefabPost() { }
		public void SetupPrefab() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");
		public override void Generate(LevelGenerator lg, System.Random rng)
		{
			base.Generate(lg, rng);
			if (posters.Length == 0)
			{
				Finished();
				return;
			}

			var room = lg.Ec.mainHall;

			foreach (var c in room.GetTilesOfShape(allowedShape, true))
				if (c.HasAllFreeWall && rng.NextDouble() <= parameters.chance[0])
					ec.BuildPoster(WeightedPosterObject.ControlledRandomSelection(posters, rng), c, c.RandomUncoveredDirection(rng));

			Finished();
		}


		[SerializeField]
		public TileShapeMask allowedShape;

		[SerializeField]
		public WeightedPosterObject[] posters = [];

	}
}
