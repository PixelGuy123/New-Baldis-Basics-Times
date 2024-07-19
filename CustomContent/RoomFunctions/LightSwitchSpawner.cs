using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class LightSwitchSpawner : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);

			if (builder.ec.standardDarkLevel == Color.white || (1f /(room.cells.Count * 0.46f)) > rng.NextDouble()) // bigger rooms have a higher chance of spawning light switch
				return; // If the level is fully light, then there's no reason for lightSwitches


			var cells = room.AllTilesNoGarbage(false, false);
			for (int i = 0; i < cells.Count; i++)
				if (!cells[i].HasFreeWall || (cells[i].shape != TileShape.Single && cells[i].shape != TileShape.Corner))
					cells.RemoveAt(i--);


			if (cells.Count == 0)
				return;

			var ecData = builder.ec.GetComponent<EnvironmentControllerData>();

			while (cells.Count > 0)
			{
				int idx = rng.Next(cells.Count);
				var dir = cells[idx].RandomUncoveredDirection(rng);
				if (dir != Direction.Null)
				{
					var machineHolder = new GameObject("LightSwitchHolder");
					machineHolder.transform.SetParent(room.transform);
					machineHolder.transform.position = cells[idx].CenterWorldPosition;

					var machine = Instantiate(lightPre, machineHolder.transform);
					cells[idx].HardCover(dir.ToCoverage());
					machine.Ec = builder.Ec;
					machine.Initialize(this);
					machine.transform.localPosition = dir.ToVector3() * 4.99f;
					machine.transform.rotation = dir.ToRotation();
					ecData.LightSwitches.Add(machine);
					break;
				}
				cells.RemoveAt(idx);
			}

		}


		public void TurnRoom(bool on)
		{
			foreach (var cell in room.cells)
				cell.SetPower(on);

			isRoomOn = on;
			UpdatePlayerVisibility();
		}

		public override void OnPlayerEnter(PlayerManager player)
		{
			base.OnPlayerEnter(player);
			players.Add(player);
			if (!isRoomOn)
			{
				player.SetInvisible(true);
				affectedPlayers.Add(player);
			}
		}

		public override void OnPlayerExit(PlayerManager player)
		{
			base.OnPlayerExit(player);
			if (!isRoomOn && player.Invisible && affectedPlayers.Contains(player))
				player.SetInvisible(false);
			players.Remove(player);
			affectedPlayers.Remove(player);
		}

		void UpdatePlayerVisibility()
		{
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].Invisible == isRoomOn && (!isRoomOn || affectedPlayers.Contains(players[i])))
				{
					players[i].SetInvisible(!isRoomOn);
					affectedPlayers.Add(players[i]);
				}
			}
		}

		public bool IsRoomOn => isRoomOn;

		bool isRoomOn = true;

		[SerializeField]
		internal LightSwitch lightPre;

		readonly List<PlayerManager> players = [], affectedPlayers = [];
	}
}
