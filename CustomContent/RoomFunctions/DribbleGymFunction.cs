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

				KickNPC(npcsToKick[0]);
				npcsToKick.RemoveAt(0);
			}

			lockEverything = true;
		}
		void KickNPC(NPC npc)
		{
			var chosenKickDoor = room.doors[Random.Range(0, room.doors.Count)];
			npc.Entity.Teleport(chosenKickDoor.bTile.FloorWorldPosition);
		}
		public void UnlockEverything()
		{
			lockEverything = false;
			for (int i = 0; i < room.doors.Count; i++)
			{
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
			if (lockEverything && npc != lastException)
			{
				KickNPC(npc); // Insta kick
				return;
			}

			if (!npcsToKick.Contains(npc))
				npcsToKick.Add(npc);
		}
		public override void OnNpcExit(NPC npc)
		{
			base.OnNpcExit(npc);
			if (npcsToKick.Contains(npc))
				npcsToKick.Remove(npc);
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
						room.doors[i].Block(true);
						doorBlockers[i].EnableObstacle(true);
					}
				}

				KickOutEveryoneFromRoom(lastException);
			}
		}
		bool lockEverything = false;
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
