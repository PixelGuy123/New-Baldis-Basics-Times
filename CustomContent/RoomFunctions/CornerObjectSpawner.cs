using System.Collections.Generic;
using HarmonyLib;
using PixelInternalAPI.Classes;
using UnityEngine;


namespace BBTimes.CustomContent.RoomFunctions
{
	public class CornerObjectSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			if (rng.NextDouble() > randomChance)
				return;

			var cells = room.GetTilesOfShape(TileShapeMask.Corner, true);
			if (randomObjs.Length == 0 || cells.Count == 0)
			{
				if (cells.Count == 0)
					Debug.LogWarning($"BB TIMES: CornerObjectSpawner was unable to find suitable spots for the object in room: {room.name}");

				return;
			}

			selectedTransform = WeightedTransform.ControlledRandomSelection(randomObjs, rng);
			int max = rng.Next(minAmount, maxAmount + 1);
			for (int i = 0; i < max; i++)
			{
				if (cells.Count == 0)
					break;

				int idx = rng.Next(cells.Count);

				Direction dir = stickToWall ? cells[idx].RandomUncoveredDirection(rng) : Direction.Null;

				spawnSpots.Add(new(cells[idx], dir));
				cells.RemoveAt(idx);
			}
		}

		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			if (selectedTransform == null)
				return;

			while (spawnSpots.Count != 0)
			{
				var transform = Instantiate(selectedTransform, room.objectObject.transform);
				var cell = spawnSpots[0].Key;
				transform.transform.position = cell.FloorWorldPosition;

				if (stickToWall)
				{
					Direction dir = spawnSpots[0].Value;
					if (dir == Direction.Null)
					{
						dir = Direction.North;
						Debug.LogWarning("BB TIMES: CornerObjectSpawner spawned a transform with the default direction North");
					}
					transform.transform.position += dir.ToVector3() * (LayerStorage.TileBaseOffset / 2);
					transform.transform.rotation = dir.ToRotation();
					cell.HardCover(dir.ToCoverage());
				}
				else
					cell.HardCover(CellCoverage.Down);

				if (nonSafeEntityCell)
					room.entitySafeCells.Remove(cell.position);

				transform.GetComponent<RendererContainer>()?.renderers.Do(cell.AddRenderer);

				if (lightPower >= 0)
				{
					room.ec.GenerateLight(cell,
					Singleton<BaseGameManager>.Instance.levelObject.standardLightColor,
					lightPower == 0 ? Singleton<BaseGameManager>.Instance.levelObject.standardLightStrength : lightPower);
				}

				spawnSpots.RemoveAt(0);
			}
		}

		[SerializeField]
		internal WeightedTransform[] randomObjs = [];

		[SerializeField]
		internal int minAmount = 1, maxAmount = 1;

		[SerializeField]
		internal bool stickToWall = false;

		[SerializeField]
		internal bool nonSafeEntityCell = false;

		[SerializeField]
		internal float randomChance = 1f;

		readonly List<KeyValuePair<Cell, Direction>> spawnSpots = [];
		Transform selectedTransform = null;

		/// <summary>
		/// Light power of object (if 0, uses the standard strength)
		/// </summary>
		[SerializeField]
		internal int lightPower = -1;
	}
}
