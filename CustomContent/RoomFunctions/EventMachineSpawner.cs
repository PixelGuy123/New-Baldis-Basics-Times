﻿using BBTimes.CustomContent.Objects;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class EventMachineSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);


			var cells = room.AllTilesNoGarbage(false, false);
			for (int i = 0; i < cells.Count; i++)
				if (!cells[i].HasAllFreeWall || (!cells[i].shape.HasFlag(TileShapeMask.Single) && !cells[i].shape.HasFlag(TileShapeMask.Corner)))
					cells.RemoveAt(i--);


			if (cells.Count == 0)
				return;

			while (cells.Count > 0)
			{
				int idx = rng.Next(cells.Count);
				var dir = cells[idx].RandomUncoveredDirection(rng);
				if (dir != Direction.Null)
				{
					var machineHolder = new GameObject("EventMachineHolder");
					machineHolder.transform.SetParent(room.transform);
					machineHolder.transform.position = cells[idx].CenterWorldPosition;

					var machine = Instantiate(machinePre, machineHolder.transform);
					machine.Ec = builder.Ec;
					machine.transform.localPosition = dir.ToVector3() * 4.99f;
					machine.transform.rotation = dir.ToRotation();
					cells[idx].AddRenderer(machine.Renderer);
					cells[idx].HardCover(dir.ToCoverage());

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
