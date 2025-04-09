using BBTimes.CustomComponents;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class SlipperyMaterialFunction : RoomFunction
	{
		[SerializeField]
		internal SlippingMaterial slipMatPre;
		readonly List<SlippingMaterial> slips = [];
		public List<SlippingMaterial> GeneratedSlips => slips;

		[SerializeField]
		internal IntVector2 minMax = new(3, 8);


		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);

			var cells = room.AllEntitySafeCellsNoGarbage();
			if (cells.Count == 0)
			{
				Debug.LogWarning("SlipperyMaterialFunction failed to find good spots for slippery materials.");
				return;
			}

			int amount = Mathf.Min(cells.Count, rng.Next(minMax.x, minMax.z));


			for (int i = 0; i < amount; i++)
			{
				if (cells.Count == 0)
					return;

				int idx = rng.Next(cells.Count);
				SpawnSlipper(cells[idx]);
				cells.RemoveAt(idx);
			}
		}

		void SpawnSlipper(Cell cell)
		{
			var slip = Instantiate(slipMatPre);
			slip.transform.position = cell.FloorWorldPosition;
			slip.GetComponentsInChildren<Renderer>().Do(cell.AddRenderer);
			slips.Add(slip);
		}
	}
}
