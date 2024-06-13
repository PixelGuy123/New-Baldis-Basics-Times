using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{

	public class TrapDoorBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			var t = room.GetTilesOfShape([TileShape.Corner, TileShape.End], CellCoverage.Down, false);
			int max = cRng.Next(minAmount, maxAmount + 1);
			if (t.Count == 0)
			{
#if CHEAT
				Debug.LogWarning("No initial spots found for the TrapdoorBuilder");
#endif
				return;
			}

			for (int i = 0; i < max; i++)
			{
				if (t.Count == 0)
					break;
				int idx = cRng.Next(t.Count);
				var trap = CreateTrapDoor(t[idx], ec, ecData);
				t.RemoveAt(idx);

				if (t.Count > 0 && max - i > 1 && cRng.NextDouble() >= 0.55) // Linked trapdoor
				{
					idx = cRng.Next(t.Count);
					var strap = CreateTrapDoor(t[idx], ec, ecData);
					trap.SetLinkedTrapDoor(strap);
					strap.SetLinkedTrapDoor(trap);
					t.RemoveAt(idx);

					trap.renderer.sprite = openSprites[1];
					strap.renderer.sprite = openSprites[1];
					trap.sprites = [closedSprites[1], openSprites[1]];
					strap.sprites = [closedSprites[1], openSprites[1]];
					continue;
				}

				trap.renderer.sprite = openSprites[0]; // Random trapdoor
				trap.sprites = [closedSprites[0], openSprites[0]];
			}

		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			base.Load(ec, pos, dir);
			var ecData = ec.GetComponent<EnvironmentControllerData>();

			for (int i = 0; i < pos.Count; i++)
			{
				var cell = ec.CellFromPosition(pos[i]);

				var trap = CreateTrapDoor(cell, ec, ecData);
				if (dir[i] != Direction.Null) // Means it is a linked trapdoor
				{
					if (++i >= pos.Count)
						throw new System.ArgumentException("A linked trapdoor was flagged with no link available on the next item of the pos collection");

					cell = ec.CellFromPosition(pos[i]); // Updates cell for the next position

					var strap = CreateTrapDoor(cell, ec, ecData); // Linked trapdoor setup
					trap.SetLinkedTrapDoor(strap);
					strap.SetLinkedTrapDoor(trap);

					trap.renderer.sprite = openSprites[1];
					strap.renderer.sprite = openSprites[1];
					trap.sprites = [closedSprites[1], openSprites[1]];
					strap.sprites = [closedSprites[1], openSprites[1]];

					continue;
				} 

				trap.renderer.sprite = openSprites[0]; // Random trapdoor
				trap.sprites = [closedSprites[0], openSprites[0]];
			}
		}

		private Trapdoor CreateTrapDoor(Cell pos, EnvironmentController ec, EnvironmentControllerData dat)
		{
			var trapdoor = Instantiate(trapDoorpre);
			trapdoor.transform.SetParent(pos.TileTransform);
			trapdoor.transform.position = pos.FloorWorldPosition;
			trapdoor.gameObject.SetActive(true);
			trapdoor.SetEC(ec);
			pos.HardCover(CellCoverage.Down | CellCoverage.Center | CellCoverage.East | CellCoverage.North | CellCoverage.South | CellCoverage.West);
			pos.AddRenderer(trapdoor.renderer);
			pos.AddRenderer(trapdoor.text.GetComponent<MeshRenderer>());
			ec.map.AddIcon(icon, trapdoor.transform, Color.white);

			dat.Trapdoors.Add(trapdoor);

			return trapdoor;
		}

		[SerializeField]
		public Trapdoor trapDoorpre;

		[SerializeField]
		public int minAmount = 1, maxAmount = 2;

		[SerializeField]
		public Sprite[] closedSprites;

		[SerializeField]
		public Sprite[] openSprites;

		internal static MapIcon icon;
	}
}
