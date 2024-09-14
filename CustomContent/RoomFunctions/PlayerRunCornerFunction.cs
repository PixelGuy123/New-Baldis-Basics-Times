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

			foreach (var cell in room.cells)
			{
				if (cell.shape == TileShape.Corner)
				{
					pos = cell.position;
					dirToFollow = cell.AllWallDirections[0].GetOpposite();
					break;
				}
			}
			if (pos == default || dirToFollow == Direction.Null)
				return;
			int attempts = 0;

			while (true)
			{
				var fPos = room.ec.CellFromPosition(pos).CenterWorldPosition;
				if (!cornersToGo.Contains(fPos))
				{
					cornersToGo.Add(fPos);
					attempts = 0;
				}
				else if (++attempts >= MakePlayerRunLineAttempts) return;

				
				for(;;)
				{
					pos += dirToFollow.ToIntVector2();
					var nextPos = pos + dirToFollow.ToIntVector2();
					var nextCell = room.ec.CellFromPosition(nextPos);
					var curCell = room.ec.CellFromPosition(pos);
					if (curCell.shape == TileShape.End) // There can't be dead ends
					{
						cornersToGo.Clear();
						Debug.LogWarning("The PlayerRunCornerFunction has been used in a room with an invalid shape! Room: " + room.name);
						return;
					}
					if (nextCell.Null || !room.ec.ContainsCoordinates(nextPos) || !nextCell.TileMatches(room) || curCell.shape == TileShape.Open)
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

		public void MakePlayerRunAround(PlayerManager player)
		{
			if (cornersToGo.Count != 0)
				StartCoroutine(Runner(player));
		}

		IEnumerator Runner(PlayerManager player)
		{
			player.Teleport(cornersToGo[0]);
			Vector3 pos = cornersToGo[0];
			activeRunners++;
			LayerMask layer = player.gameObject.layer;
			player.plm.Entity.SetFrozen(true);
			player.plm.Entity.SetInteractionState(false);
			player.gameObject.layer = layer; // Workaround lol
			float timer = 30f;
			int i = 0;
			while (true)
			{
				if (!player)
				{
					activeRunners--;
					yield break;
				}
				timer -= player.ec.EnvironmentTimeScale * Time.deltaTime;
				Vector3 difference = cornersToGo[i] - pos;
				while (difference.magnitude > 0.5f)
				{
					if (!player)
					{
						activeRunners--;
						yield break;
					}
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

					
					if (player.plm.stamina <= 0f || timer <= 0f)
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

		[SerializeField]
		internal int MakePlayerRunLineAttempts = 3;
	}
}
