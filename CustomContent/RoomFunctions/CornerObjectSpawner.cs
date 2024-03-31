using BBTimes.Manager;
using PixelInternalAPI.Classes;
using UnityEngine;


namespace BBTimes.CustomContent.RoomFunctions
{
	public class CornerObjectSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			if (rng.NextDouble() > randomChance)
				return;

			base.Build(builder, rng);
			var cells = room.GetTilesOfShape([TileShape.Corner], true);
			if (randomObjs.Length == 0 || cells.Count == 0) 
			{
				if (cells.Count == 0)
					Debug.LogWarning("BB TIMES: CornerObjectSpawner was unable to find suitable spots for the object");
				return;
			}

			var obj = WeightedTransform.ControlledRandomSelection(randomObjs, rng);
			objAmount.Rng = rng;
			int max = objAmount.RandomVal;
			for (int i = 0; i < max; i++)
			{
				if (cells.Count == 0)
					break;

				var transform = Instantiate(obj, room.transform);
				var cell = cells[rng.Next(cells.Count)];
				transform.transform.position = cell.FloorWorldPosition;
				if (stickToWall)
				{
					Direction dir = cell.RandomUncoveredDirection(rng);
					if (dir == Direction.Null)
					{
						dir = Direction.North;
						Debug.LogWarning("BB TIMES: CornerObjectSpawner spawned a transform with the default direction North");
					}
					transform.transform.position += dir.ToVector3() * (BBTimesManager.TileBaseOffset / 2);
					transform.transform.rotation = dir.ToRotation();
				}

				//transform.GetComponent<RendererContainer>().renderers.Do(cell.AddRenderer);

				if (lightPower >= 0)
					builder.Ec.GenerateLight(cell, builder.ld.standardLightColor, lightPower == 0 ? builder.ld.standardLightStrength : lightPower);
			}
		}

		[SerializeField]
		public WeightedTransform[] randomObjs = [];

		[SerializeField]
		public MinMax objAmount = new(1, 1);

		[SerializeField]
		public bool stickToWall = false;

		[SerializeField]
		public float randomChance = 1f;

		/// <summary>
		/// Light power of object (if 0, uses the standard strength)
		/// </summary>
		[SerializeField]
		public int lightPower = -1;
	}
}
