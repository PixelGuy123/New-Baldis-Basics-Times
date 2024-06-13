using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class CameraBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			var spots = room.GetTilesOfShape([TileShape.Corner, TileShape.Single], false);
			for (int i = 0; i < spots.Count; i++)
				if (!spots[i].HardCoverageFits(CellCoverage.Up))
					spots.RemoveAt(i--);

			if (spots.Count == 0)
			{
				Debug.LogWarning("CameraBuilder has failed to find a good spot for the Security Camera.");
				return;
			}

			int amount = cRng.Next(minAmount, maxAmount + 1);
			for (int i = 0; i < amount; i++)
			{
				if (spots.Count == 0)
					break;
				int s = cRng.Next(spots.Count);
				var cam = Instantiate(camPre, spots[s].ObjectBase).GetComponentInChildren<SecurityCamera>();
				cam.Ec = ec;
				cam.GetComponentsInChildren<SpriteRenderer>().Do(spots[s].AddRenderer);
				cam.Setup(spots[s].AllOpenNavDirections, cRng.Next(2, 4));
				ecData.Cameras.Add(cam);

				spots[s].HardCover(CellCoverage.Up);
				spots.RemoveAt(s);
			}

		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			base.Load(ec, pos, dir);
			var ecData = ec.GetComponent<EnvironmentControllerData>();
			for (int i = 0; i < pos.Count; i++)
			{
				var spot = ec.CellFromPosition(pos[i]);
				var cam = Instantiate(camPre, spot.ObjectBase).GetComponentInChildren<SecurityCamera>();
				cam.Ec = ec;
				cam.GetComponentsInChildren<Renderer>().Do(spot.AddRenderer);
				cam.Setup(spot.AllOpenNavDirections, (int)dir[i]);
				ecData.Cameras.Add(cam);

				spot.HardCover(CellCoverage.Up);
			}
		}

		[SerializeField]
		internal Transform camPre;

		[SerializeField]
		internal int minAmount = 2, maxAmount = 4;
	}
}
