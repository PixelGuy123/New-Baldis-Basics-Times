using BBTimes.CustomContent.Objects;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class CameraBuilder : ObjectBuilder
	{
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
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

				spots[s].HardCover(CellCoverage.Up);
				spots.RemoveAt(s);
			}

		}

		[SerializeField]
		internal Transform camPre;

		[SerializeField]
		internal int minAmount = 2, maxAmount = 4;
	}
}
