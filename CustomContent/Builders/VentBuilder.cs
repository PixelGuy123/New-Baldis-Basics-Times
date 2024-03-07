using System;
using UnityEngine;
using BBTimes.CustomContent.Objects;
using System.Collections.Generic;

namespace BBTimes.CustomContent.Builders
{
	public class VentBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			List<Cell> halls = room.GetTilesOfShape([TileShape.Corner, TileShape.Single], false);
			if (halls.Count == 0) return;

			int ventAmount = cRng.Next(minAmount, maxAmount + 1);

			var selectedWebTile = halls[cRng.Next(halls.Count)];
			var web = ec.FindNearbyTiles(selectedWebTile.position - new IntVector2(builder.levelSize.x / 5, builder.levelSize.z / 5),
				selectedWebTile.position + new IntVector2(builder.levelSize.x / 5, builder.levelSize.z / 5),
				(builder.levelSize.x + builder.levelSize.z) / 6);

			List<Vent> vents = [];

			foreach (var cell in web)
			{
				if (!cell.HasAnyHardCoverage && !cell.open && !cell.doorHere && (cell.shape == TileShape.Corner || cell.shape == TileShape.Single))
				{
					var vent = Instantiate(ventPrefab, room.transform);
					vent.transform.position = cell.FloorWorldPosition;
					vent.SetActive(true);
					var v = vent.GetComponent<Vent>();
					v.ec = ec;
					vents.Add(v);
				}
				if (vents.Count >= ventAmount)
					break;
			}

			List<IntVector2> connectionpos = [];

			foreach (var vent in vents)
			{
				Cell center = ec.CellFromPosition(vent.transform.position);
				for (int i = 0; i < vents.Count; i++)
				{
					if (vents[i] == vent) continue; // Not make a path to itself of course
					ec.FindPath(center, ec.CellFromPosition(vents[i].transform.position), PathType.Const, out var path, out bool success);
					if (!success) continue;
					foreach (var t in path)
					{
						if (connectionpos.Contains(t.position)) continue;
						connectionpos.Add(t.position);
						var c = Instantiate(ventConnectionPrefab);
						c.transform.SetParent(t.TileTransform);
						c.transform.localPosition = Vector3.up * 9.5f;
						c.SetActive(true);
					}
				}
				var v = vent.GetComponent<Vent>();
				v.nextVents = new(vents);
				v.nextVents.Remove(vent); // nextVents, excluding itself
			}

			vents[0].BlockMe();


		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			base.Load(ec, pos, dir);
		}

		[SerializeField]
		public GameObject ventPrefab;

		[SerializeField]
		public GameObject ventConnectionPrefab;


		const int minAmount = 6, maxAmount = 10;
	}
}
