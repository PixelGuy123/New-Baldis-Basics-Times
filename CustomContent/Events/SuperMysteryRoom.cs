using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class SuperMysteryRoom : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("SuperMysteryRoom.wav", "Event_SuperMystery", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [new() { time = 4.092f, key = "Vfx_BAL_Event_MysteryRoom_2" }];

			mysteryDoorPre = GenericExtensions.FindResourceObject<MysteryDoor>();
		}
		public void SetupPrefabPost() =>
			target = EnumExtensions.GetFromExtendedName<RoomCategory>("SuperMystery");
		
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------
		public override void Begin()
		{
			base.Begin();
			if (room != null)
			{
				foreach (MysteryDoor mysteryDoor in mysteryDoors)
				{
					mysteryDoor.HideDoor(false);
					mysteryDoor.Door.Unlock();
				}
			}
		}
		public override void End()
		{
			base.End();
			if (room != null)
			{
				foreach (MysteryDoor mysteryDoor in mysteryDoors)
				{
					mysteryDoor.HideDoor(true);
					mysteryDoor.Door.Lock(true);
				}
			}
		}
		public override void AfterUpdateSetup()
		{
			if (room != null)
			{
				foreach (Cell cell in room.GetNewTileList())
				{
					cell.HardCoverEntirely();
					for (int i = 0; i < 4; i++)
						if (cell.HasWallInDirection((Direction)i))
							cell.HardCoverWall((Direction)i, true);
				}

				UpdateDoor();
			}
		}

		void UpdateDoor()
		{
			if (room != null)
			{
				foreach (Door door in room.doors)
				{
					mysteryDoors.Add(door.GetComponent<MysteryDoor>());

					MaterialModifier.SetBase(mysteryDoors[mysteryDoors.Count - 1].Cover, door.bTile.room.wallTex);
					mysteryDoors[mysteryDoors.Count - 1].HideDoor(true);
					door.bTile.Block(door.direction.GetOpposite(), true);
				}
			}
		}

		public override void AssignRoom(RoomController room)
		{
			base.AssignRoom(room);
			room.doorPre = mysteryDoorPre.Door;
			room.potentialDoorPositions.Clear();
			room.forcedDoorPositions.Clear();
		}

		public override void PremadeSetup()
		{
			foreach (RoomController roomController in this.ec.rooms)
			{
				if (roomController.category == target)
				{
					room = roomController;
					break;
				}
			}
			UpdateDoor();
		}

		[SerializeField]
		internal List<MysteryDoor> mysteryDoors = [];

		[SerializeField]
		internal RoomCategory target;

		[SerializeField]
		internal MysteryDoor mysteryDoorPre;

	}
}
