using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class PlayerRunCornerFunction : RoomFunction
	{
		public override void Initialize(RoomController room)
		{
			base.Initialize(room);
			IntVector2 pos = default;
			Direction dirToFollow = Direction.Null;
			int cornerCount = 0;
			foreach (var cell in room.cells)
			{
				if (cell.shape == TileShape.Corner)
				{
					pos = cell.position;
					dirToFollow = cell.AllWallDirections[0].GetOpposite();
					cornerCount++;
				}
			}
			if (pos == default || dirToFollow == Direction.Null)
				return;

			while (cornersToGo.Count < cornerCount)
			{
				cornersToGo.Add(room.ec.CellFromPosition(pos).CenterWorldPosition);
				for(;;)
				{
					pos += dirToFollow.ToIntVector2();
					var nextCell = room.ec.CellFromPosition(pos + dirToFollow.ToIntVector2());
					var curCell = room.ec.CellFromPosition(pos);
					if (curCell.shape == TileShape.End) // There can't be dead ends
					{
						cornersToGo.Clear();
						Debug.LogWarning("The PlayerRunCornerFunction has been used in a room with an invalid shape! Room: " + room.name);
						return;
					}
					if (!nextCell.TileMatches(room) || curCell.shape == TileShape.Open)
					{
						var prevDir = dirToFollow;
						dirToFollow = dirToFollow.PerpendicularList()[0];
						nextCell = room.ec.CellFromPosition(pos + dirToFollow.ToIntVector2());
						if (prevDir.GetOpposite() == dirToFollow || !nextCell.TileMatches(room) || nextCell.shape == TileShape.Open)
							dirToFollow = prevDir.PerpendicularList()[1];
						break;
					}
				}

			}


		}

		public void MakePlayerRunAround(PlayerManager player) =>
			StartCoroutine(Runner(player));

		IEnumerator Runner(PlayerManager player)
		{
			player.Teleport(cornersToGo[0]);
			Vector3 pos = cornersToGo[0];
			activeRunners++;
			LayerMask layer = player.gameObject.layer;
			player.plm.Entity.SetFrozen(true);
			player.plm.Entity.SetInteractionState(false);
			player.gameObject.layer = layer; // Workaround lol
			int i = 0;
			while (true)
			{
				Vector3 difference = cornersToGo[i] - pos;
				while (difference.magnitude > 0.5f)
				{
					Vector3 mov = difference.normalized * player.plm.runSpeed * player.PlayerTimeScale * Time.deltaTime;
					pos += mov.magnitude < difference.magnitude ? mov : difference;
					difference = cornersToGo[i] - pos;
					
					if ((player.transform.position - pos).magnitude > player.plm.runSpeed)
					{
						activeRunners--;
						player.plm.Entity.SetFrozen(false);
						player.plm.Entity.SetInteractionState(true);
						yield break;
					}

					player.Teleport(pos); // Sums up staminaRise to force a drop
					player.plm.stamina = Mathf.Max(player.plm.stamina - (player.plm.staminaDrop + player.plm.staminaRise) * Time.deltaTime * player.PlayerTimeScale, 0f);

					pos = player.transform.position;

					
					if (player.plm.stamina <= 0f)
					{
						activeRunners--;
						player.plm.Entity.SetFrozen(false);
						player.plm.Entity.SetInteractionState(true);
						yield break;
					}

					yield return null;
				}
				i++;
				i %= cornersToGo.Count;
				yield return null;
			}
		}

		int activeRunners = 0;

		public bool IsActive => activeRunners > 0;

		readonly List<Vector3> cornersToGo = [];
	}
}
