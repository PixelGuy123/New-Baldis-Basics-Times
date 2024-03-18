using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.ModPatches.EnvironmentPatches;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class VentBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			List<Cell> halls = room.GetTilesOfShape([TileShape.Corner, TileShape.Single], false);
			if (halls.Count == 0) return;

			amounts.Rng = cRng;
			int ventAmount = amounts.RandomVal;

			var selectedWebTile = halls[cRng.Next(halls.Count)];
			var web = ec.FindNearbyTiles(selectedWebTile.position - new IntVector2(builder.levelSize.x / 5, builder.levelSize.z / 5),
				selectedWebTile.position + new IntVector2(builder.levelSize.x / 5, builder.levelSize.z / 5),
				(builder.levelSize.x + builder.levelSize.z) / 6);

			List<Vent> vents = [];

			foreach (var cell in web)
			{
				if (!ec.TrapCheck(cell) && !cell.HasAnyHardCoverage && !cell.open && !cell.doorHere && (cell.shape == TileShape.Corner || cell.shape == TileShape.Single))
				{
					var vent = Instantiate(ventPrefab, room.transform);
					vent.transform.position = cell.FloorWorldPosition;
					vent.SetActive(true);
					var v = vent.GetComponent<Vent>();
					v.ec = ec;
					cell.HardCoverEntirely();
					vents.Add(v);
				}
				if (vents.Count >= ventAmount)
					break;
			}

			if (vents.Count == 0) return;

			Dictionary<IntVector2, GameObject> connectionpos = [];

			foreach (var vent in vents)
			{
				Cell center = ec.CellFromPosition(vent.transform.position);
				for (int i = 0; i < vents.Count; i++)
				{
					if (vents[i] == vent) continue; // Not make a path to itself of course
					EnvironmentControllerPatch.data = new([TileShape.Closed], [RoomType.Hall], true); // Limit to only hallways
					ec.FindPath(center, ec.CellFromPosition(vents[i].transform.position), PathType.Const, out var path, out bool success);
					EnvironmentControllerPatch.ResetData();
					if (!success) continue;
					foreach (var t in path)
					{
						if (connectionpos.ContainsKey(t.position)) continue;
						var c = Instantiate(ventConnectionPrefab);
						t.HardCover(CellCoverage.Up);
						c.transform.SetParent(t.TileTransform);

						t.AddRenderer(c.GetComponent<MeshRenderer>());

						c.transform.localPosition = Vector3.up * 9.5f;
						c.SetActive(true);
						connectionpos.Add(t.position, c);

						List<Cell> neighbors = [];
						ec.GetNavNeighbors(t, neighbors, PathType.Const);
						foreach (var n in neighbors)
						{
							if (connectionpos.TryGetValue(n.position, out var c2))
							{
								var dir = Directions.DirFromVector3(c2.transform.position - c.transform.position, 45f); // 90° angle
								var child = c.transform.Find("VentPrefab_Connection_" + dir);
								child?.gameObject.SetActive(true);

								child = c2.transform.Find("VentPrefab_Connection_" + dir.GetOpposite());
								child?.gameObject.SetActive(true);
							}
						}

						foreach (var c2 in c.transform.AllChilds())
							t.AddRenderer(c2.GetComponent<MeshRenderer>());
					}
				}
				var v = vent.GetComponent<Vent>();
				v.nextVents = new(vents);
				v.nextVents.Remove(vent); // nextVents, excluding itself
			}

			vents[0].BlockMe();


		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir) // In case I modify premade assets (like Endless medium)
		{
			base.Load(ec, pos, dir);
			List<Vent> vents = [];

			foreach (var p in pos)
			{
				var cell = ec.CellFromPosition(p);
				var vent = Instantiate(ventPrefab, cell.room.transform);
				vent.transform.position = cell.FloorWorldPosition;
				vent.SetActive(true);
				var v = vent.GetComponent<Vent>();
				v.ec = ec;
				cell.HardCoverEntirely();
				vents.Add(v);
			}

			if (vents.Count == 0) return;

			Dictionary<IntVector2, GameObject> connectionpos = [];

			foreach (var vent in vents)
			{
				Cell center = ec.CellFromPosition(vent.transform.position);
				for (int i = 0; i < vents.Count; i++)
				{
					if (vents[i] == vent) continue; // Not make a path to itself of course
					EnvironmentControllerPatch.data = new([TileShape.Closed], [RoomType.Hall], true); // Limit to only hallways
					ec.FindPath(center, ec.CellFromPosition(vents[i].transform.position), PathType.Const, out var path, out bool success);
					EnvironmentControllerPatch.ResetData();
					if (!success) continue;
					foreach (var t in path)
					{
						if (connectionpos.ContainsKey(t.position)) continue;
						var c = Instantiate(ventConnectionPrefab);
						t.HardCover(CellCoverage.Up);
						c.transform.SetParent(t.TileTransform);

						t.AddRenderer(c.GetComponent<MeshRenderer>());

						c.transform.localPosition = Vector3.up * 9.5f;
						c.SetActive(true);
						connectionpos.Add(t.position, c);

						List<Cell> neighbors = [];
						ec.GetNavNeighbors(t, neighbors, PathType.Const);
						foreach (var n in neighbors)
						{
							if (connectionpos.TryGetValue(n.position, out var c2))
							{
								var d = Directions.DirFromVector3(c2.transform.position - c.transform.position, 45f); // 90° angle
								var child = c.transform.Find("VentPrefab_Connection_" + d);
								child?.gameObject.SetActive(true);

								child = c2.transform.Find("VentPrefab_Connection_" + d.GetOpposite());
								child?.gameObject.SetActive(true);
							}
						}

						foreach (var c2 in c.transform.AllChilds())
							t.AddRenderer(c2.GetComponent<MeshRenderer>());
					}
				}
				var v = vent.GetComponent<Vent>();
				v.nextVents = new(vents);
				v.nextVents.Remove(vent); // nextVents, excluding itself
			}

			vents[0].BlockMe();
		}

		[SerializeField]
		public GameObject ventPrefab;

		[SerializeField]
		public GameObject ventConnectionPrefab;


		MinMax amounts = new(6, 10);
	}
}
