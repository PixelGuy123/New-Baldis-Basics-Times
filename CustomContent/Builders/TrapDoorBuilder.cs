using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	// ********* future note: make a Load method for this **************
	// *****************************************************************
	// *****************************************************************
	// *****************************************************************

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
			var i = ec.map.AddIcon(icon, trapdoor.transform, Color.white);
			i.gameObject.SetActive(true);

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
