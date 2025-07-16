using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class DribbleGymFunction : RoomFunction
	{
		// The caller here is the "exception" that kicks everyone in the first place
		public void KickOutEveryoneFromRoom(NPC caller) // Additionally lock the doors to prevent anyone from entering
		{
			lastException = caller;

			while (npcsToKick.Count != 0)
			{
				if (!npcsToKick[0] || npcsToKick[0] == caller)
				{
					npcsToKick.RemoveAt(0);
					continue;
				}

				var chosenKickDoor = room.doors[Random.Range(0, room.doors.Count)];
				npcsToKick[0].Navigator.Entity.Teleport(chosenKickDoor.bTile.FloorWorldPosition);
				npcsToKick.RemoveAt(0);
			}

			lockEverything = true;
		}
		public void UnlockEverything()
		{
			lockEverything = false;
			for (int i = 0; i < room.doors.Count; i++)
			{
				room.doors[i].Unlock();
				room.doors[i].Block(false);
				doorBlockers[i].EnableObstacle(false);
			}
		}
		public void UpdatePoster(int points)
		{
			if (!activePosterObjects.TryGetValue(points, out var poster))
			{
				poster = Instantiate(chalkboardPre);
				poster.name = chalkboardPre.name + "_Temporary_" + points;
				poster.textData[1].textKey = points.ToString();
				activePosterObjects.Add(points, poster);
			}

			room.ec.BuildPoster(poster, posterCell, posterDirection, false);
		}
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			List<Cell> availableCells = [];

			foreach (var cell in room.cells)
			{
				if (cell.WallCount != 0)
					availableCells.Add(cell);
			}

			if (availableCells.Count == 0)
			{
				Debug.LogError("BBTIMES: PlayerRunCornerFunction couldn\'t find a spot for the chalkboard.");
				return;
			}

			// Get spot for poster
			posterCell = availableCells[rng.Next(availableCells.Count)];
			posterDirection = posterCell.RandomConstDirection(rng);
		}
		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();

			UpdatePoster(0); // Get the poster on the wall

			// Useful when locking up the doors
			for (int i = 0; i < room.doors.Count; i++)
				doorBlockers.Add(StandardDoorNavMeshBlocker.AddBlockerToDoor(room.doors[i])); // Add blocker to door

			// Initializing stuff
			var runLines = room.objectObject.GetComponentsInChildren<RunLineMarker>();
			if (runLines.Length == 0)
			{
				Debug.LogError("BBTIMES: PlayerRunCornerFunction has been placed in a room with no Run Lines.");
				return;
			}

			var directions = Directions.All();
			int currentDirectionIndex = Random.Range(0, directions.Count);
			Direction dir;
			IntVector2 currentPosition;

			// Getting a few actions done
			var hasRunLineInThere = new System.Func<IntVector2, int>((position) =>
			{
				for (int i = 0; i < runLines.Length; i++)
				{
					if (IntVector2.GetGridPosition(runLines[i].transform.position) == position)
						return i;
				}
				return -1;
			});

			// For a better randomized selection, change the first runLine to a random one in the array :D
			if (runLines.Length != 1)
			{
				int randomIndex = Random.Range(1, runLines.Length);
				var randomLine = runLines[randomIndex]; // Swap here
				runLines[randomIndex] = runLines[0];
				runLines[0] = randomLine;
			}


			for (int lineIdx = 0; lineIdx < runLines.Length - 1; lineIdx++)
			{
				bool success = false;
				currentPosition = room.ec.CellFromPosition(runLines[lineIdx].transform.position).position;
				// Get an adjacent line and move it to the adjacent index in this array
				for (int i = 0; i < directions.Count; i++)
				{
					dir = directions[currentDirectionIndex];
					int foundLineIndex = hasRunLineInThere(currentPosition + dir.ToIntVector2());
					// Swap the found index with the next index (making sure every single runLine is adjacent in the right direction)
					if (foundLineIndex != -1)
					{
						var nextRunLine = runLines[lineIdx + 1]; // Get the currently adjacent run line in the array
						runLines[lineIdx + 1] = runLines[foundLineIndex]; // Swap both
						runLines[foundLineIndex] = nextRunLine;

						success = true;
						break;
					}
					currentDirectionIndex = ++currentDirectionIndex % directions.Count;
				}

				if (!success)
				{
					Debug.LogError("BBTIMES: PlayerRunCornerFunction has been placed in a room with disconnected Run Lines.");
					return; // Stop script from here
				}
			}

			for (int i = 0; i < runLines.Length; i++)
				spotsToGo.Add(room.ec.CellFromPosition(runLines[i].transform.position).CenterWorldPosition);

			// ***** Getting basketball hoops with marker ******
			// Not the best workaround, but enough for this purpose
			foreach (var child in room.objectObject.transform.AllChilds())
			{
				if (child.name != "HoopBase(Clone)") continue;
				var hoop = child.gameObject.AddComponent<BasketballHoopMarker>();
				hoop.audMan = child.gameObject.CreatePropagatedAudioManager(55f, 125f); // Add an audio manager
				hoop.audMan.disableSubtitles = true;
				hoop.audGoal = audGoal;
				hoop.localHoopPosition = new(0f, 8f, 2.5f);
			}
		}

		public override void OnNpcEnter(NPC npc)
		{
			base.OnNpcEnter(npc);
			if (!npcsToKick.Contains(npc))
				npcsToKick.Add(npc);
		}
		public override void OnNpcExit(NPC npc)
		{
			base.OnNpcExit(npc);
			if (npcsToKick.Contains(npc))
				npcsToKick.Remove(npc);
		}

		public void MakePlayerRunAround(PlayerManager player, float staminaThreshold = 0f)
		{
			if (spotsToGo.Count != 0)
				StartCoroutine(Runner(player, staminaThreshold));
		}

		IEnumerator Runner(PlayerManager player, float staminaThreshold = 0f)
		{
			// Teleport player to the first corner and initialize variables
			player.Teleport(spotsToGo[0]);
			Vector3 pos = spotsToGo[0];
			activeRunners++;
			LayerMask layer = player.gameObject.layer;

			// Freeze player and disable interaction
			player.plm.Entity.SetFrozen(true);
			player.plm.Entity.SetInteractionState(false);

			// Restore player layer (workaround)
			player.gameObject.layer = layer;

			float timer = 30f; // Set a maximum time limit
			int i = 0; // Start at the first corner

			while (true)
			{
				// Exit if player reference is lost
				if (!player)
				{
					activeRunners--;
					yield break;
				}

				// Decrease timer based on environment time scale
				timer -= player.ec.EnvironmentTimeScale * Time.deltaTime;

				// Calculate vector to next corner
				Vector3 difference = spotsToGo[i] - pos;

				// Move towards the current target corner
				while (difference.magnitude > 0.5f)
				{
					// Exit if player reference is lost
					if (!player)
					{
						activeRunners--;
						yield break;
					}

					// Calculate movement vector for this frame
					Vector3 mov = difference.normalized * player.plm.runSpeed * player.PlayerTimeScale * Time.deltaTime;
					pos += mov.magnitude < difference.magnitude ? mov : difference;
					difference = spotsToGo[i] - pos;

					// If player is too far from the intended position, unfreeze and exit
					if ((player.transform.position - pos).magnitude > player.plm.runSpeed)
					{
						activeRunners--;
						player.plm.Entity.SetFrozen(false);
						player.plm.Entity.SetInteractionState(true);
						yield break;
					}

					// Teleport player to new position and drain stamina
					player.Teleport(pos); // Sums up staminaRise to force a drop
					player.plm.stamina = Mathf.Max(
						player.plm.stamina - (player.plm.staminaDrop + player.plm.staminaRise) * Time.deltaTime * player.PlayerTimeScale,
						0f
					);

					// Update position in case teleport changed it
					pos = player.transform.position;

					// If stamina runs out or timer expires, unfreeze and exit
					if (player.plm.stamina <= staminaThreshold || timer <= 0f)
					{
						activeRunners--;
						player.plm.Entity.SetFrozen(false);
						player.plm.Entity.SetInteractionState(true);
						yield break;
					}

					yield return null; // Wait for next frame
				}

				// Move to the next spot (loop around if at the end)
				i = ++i % spotsToGo.Count;
				yield return null;
			}
		}

		void OnDestroy() // Destroys the PosterObject to free up memory
		{
			if (activePosterObjects.Count != 0)
			{
				foreach (var poster in activePosterObjects.Values)
				{
					if (poster)
						Destroy(poster);
				}
			}
		}

		void Update()
		{
			if (lockEverything)
			{
				for (int i = 0; i < room.doors.Count; i++)
				{
					if (!room.doors[i].locked)
					{
						room.doors[i].Lock(true); // lock doors from outside + block them to prevent anyone from entering

						if (room.doors[i].IsOpen)
							room.doors[i].Shut();

						room.doors[i].Block(true);
						doorBlockers[i].EnableObstacle(true);
					}
				}

				KickOutEveryoneFromRoom(lastException);
			}
		}

		int activeRunners = 0;
		bool lockEverything = false;

		public bool IsActive => activeRunners > 0;

		readonly List<Vector3> spotsToGo = [];
		readonly List<NPC> npcsToKick = [];
		readonly List<StandardDoorNavMeshBlocker> doorBlockers = [];
		NPC lastException = null;

		[SerializeField]
		internal int MakePlayerRunLineAttempts = 3;
		[SerializeField]
		internal PosterObject chalkboardPre;
		[SerializeField]
		internal SoundObject audGoal;

		readonly Dictionary<int, PosterObject> activePosterObjects = []; // System to handle multiple poster objects without contanstly regenerating a new texture2D for each point.
		Cell posterCell;
		Direction posterDirection;
	}
}
