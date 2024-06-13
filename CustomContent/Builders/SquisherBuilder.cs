using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class SquisherBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			var spots = room.GetTilesOfShape([TileShape.Corner, TileShape.Single, TileShape.Straight], false);
			for (int i = 0; i < spots.Count; i++)
				if (!spots[i].HardCoverageFits(CellCoverage.Up))
					spots.RemoveAt(i--);

			if (spots.Count == 0)
			{
				Debug.LogWarning("SquisherBuilder has failed to find a good spot for the Squishers.");
				return;
			}
			int am = cRng.Next(minAmount, maxAmount + 1);
			for (int i = 0; i < am; i++)
			{
				if (spots.Count == 0)
					break;

				int idx = cRng.Next(spots.Count);

				var squ = Instantiate(squisherPre, spots[idx].ObjectBase);
				squ.transform.localPosition = Vector3.up * 8.5f;
				squ.Ec = ec;
				squ.Setup(cRng.Next(5, 9));
				squ.GetComponentsInChildren<Renderer>().Do(spots[idx].AddRenderer);
				ecData.Squishers.Add(squ);
				if (cRng.NextDouble() > 0.7f)
					GameButton.BuildInArea(ec, spots[idx].position, spots[idx].position, cRng.Next(4, 7), squ.gameObject, buttonPre, cRng);

				spots.RemoveAt(idx);
			}
		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			base.Load(ec, pos, dir);
			for (int i = 0; i < pos.Count; i++)
			{
				var cell = ec.CellFromPosition(pos[i]);
				var squ = Instantiate(squisherPre, cell.ObjectBase);
				squ.transform.localPosition = Vector3.up * 9f;
				squ.Ec = ec;
				squ.Setup((int)dir[i] + 1);
				squ.GetComponentsInChildren<Renderer>().Do(cell.AddRenderer);
				ecData.Squishers.Add(squ);
			}
		}

		[SerializeField]
		internal int minAmount = 2, maxAmount = 4;

		[SerializeField]
		internal Squisher squisherPre;

		[SerializeField]
		internal GameButton buttonPre;
	}
}
