using BBTimes.CustomContent.Objects;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class EventMachineSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);


			var cells = room.AllTilesNoGarbage(false, true);
			for (int i = 0; i < cells.Count; i++)
				if (cells[i].shape != TileShape.Single && cells[i].shape != TileShape.Corner)
					cells.RemoveAt(i--);


			if (cells.Count == 0)
				return;

			while (cells.Count > 0)
			{
				int idx = rng.Next(cells.Count);
				var dirs = cells[idx].AllWallDirections;
				if (dirs.Count != 0)
				{
					var machineHolder = new GameObject("EventMachineHolder");
					machineHolder.transform.SetParent(room.transform);
					machineHolder.transform.position = cells[idx].CenterWorldPosition;

					var machine = Instantiate(machinePre, machineHolder.transform);
					Direction dir = dirs[rng.Next(dirs.Count)];
					machine.Ec = builder.Ec;
					machine.transform.localPosition = dir.ToVector3() * 4.99f;
					machine.transform.rotation = dir.ToRotation();

					var icon = builder.Ec.map.AddIcon(iconPre, machineHolder.transform, Color.white);
					machine.mapIcon = icon;
					break;
				}
				cells.RemoveAt(idx);
			}

		}

		[SerializeField]
		internal EventMachine machinePre;

		internal static MapIcon iconPre;
	}
}
