using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class Structure_SmallDoor : StructureBuilder, IBuilderPrefab
	{

		public StructureWithParameters SetupBuilderPrefabs()
		{
			var templateDoor = Instantiate(GenericExtensions.FindResourceObject<StandardDoor>());
			templateDoor.gameObject.ConvertToPrefab(true);
			templateDoor.name = "SmallDoor";

			doorPre = templateDoor.gameObject.AddComponent<SmallDoor>();
			doorPre.audMan = templateDoor.audMan;
			doorPre.audOpen = this.GetSound("smallDoorOpen.wav", "Sfx_Doors_StandardOpen", SoundType.Voice, Color.white);
			doorPre.audClose = this.GetSound("smallDoorShut.wav", "Sfx_Doors_StandardShut", SoundType.Voice, Color.white);

			doorPre.colliders = templateDoor.colliders;
			Texture2D activeTex = this.GetTexture("smallDoorMask.png");


			doorPre.mask = templateDoor.mask;
			var matTemplate = new Material(doorPre.mask[0])
			{
				name = "SmallDoorMask"
			};
			matTemplate.SetTexture("_Mask", activeTex);

			for (int i = 0; i < doorPre.mask.Length; i++)
				doorPre.mask[i] = matTemplate;
			

			activeTex = this.GetTexture("smallDoorClosed.png");

			doorPre.doors = templateDoor.doors;
			
			doorPre.overlayShut = templateDoor.overlayShut;
			matTemplate = new(doorPre.overlayShut[0])
			{
				name = "SmallDoor_OverlayShut"
			};

			matTemplate.SetMainTexture(activeTex);


			for (int i = 0; i < doorPre.overlayShut.Length; i++)
				doorPre.overlayShut[i] = matTemplate;
			

			activeTex = this.GetTexture("smallDoorOpen.png");

			doorPre.overlayOpen = templateDoor.overlayOpen;
			matTemplate = new(doorPre.overlayOpen[0])
			{
				name = "SmallDoor_OverlayOpen"
			};
			matTemplate.SetMainTexture(activeTex);

			for (int i = 0; i < doorPre.overlayOpen.Length; i++)
				doorPre.overlayOpen[i] = matTemplate;
			

			doorPre.bg = templateDoor.bg;

			Destroy(templateDoor); // Destroys StandardDoor

			doorPre.gameObject.AddComponent<RendererContainer>().renderers = doorPre.doors;
			doorPre.doorIcon = this.GetSprite(ObjectCreationExtension.defaultMapIconPixelsPerUnit, "smallDoorIcon.png");

			return new() { prefab = this, parameters = new() { chance = [0.55f] } }; // Chance = factor to multiply with the amount of rooms in a level (determines the amount of small doors)
		}
		public void SetupPrefab() { }
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");


		// Prefab stuff above ^^
		public override void Generate(LevelGenerator lg, System.Random rng)
		{
			base.Generate(lg, rng);

			var room = lg.Ec.mainHall;
			var cells = room.AllTilesNoGarbage(false, false);
			List<KeyValuePair<Cell, Direction>> availableCells = [];
			for (int i = 0; i < cells.Count; i++)
			{
				if (!cells[i].HasAllFreeWall)
				{
					cells.RemoveAt(i--);
					continue;
				}

				foreach (var dir in cells[i].AllWallDirections)
				{
					var nextCell = ec.CellFromPosition(cells[i].position + dir.ToIntVector2());
					if (!nextCell.Null &&
						!cells[i].WallHardCovered(dir) && 
						nextCell.room.potentialDoorPositions.Contains(nextCell.position))
					{
						availableCells.Add(new(cells[i], dir));
					}
						
				}
			}

			int max = Mathf.FloorToInt(ec.rooms.Count * parameters.chance[0]);

			for (int i = 0; i < max; i++)
			{
				if (availableCells.Count == 0) // Reminder that the cells here are all from the mainHall, the direction is the one that tells what room it is aiming to
					return;

				int index = rng.Next(availableCells.Count);
				var cellPair = availableCells[index];
				BuildDoor(ec, cellPair.Key, cellPair.Value);
				var nextRoom = ec.CellFromPosition(cellPair.Key.position + cellPair.Value.ToIntVector2()).room;

				availableCells.RemoveAll(cell => ec.CellFromPosition(
					cell.Key.position + cell.Value.ToIntVector2()
					).TileMatches(nextRoom));
			}

			Finished();
		}

		public override void Load(List<StructureData> data)
		{
			base.Load(data);
			for (int i = 0; i < data.Count; i++)
				BuildDoor(ec, ec.CellFromPosition(data[i].position), data[i].direction);

			Finished();
		}

		void BuildDoor(EnvironmentController ec, Cell cell, Direction dir)
		{
			var door = Instantiate(doorPre, cell.ObjectBase);
			door.ec = ec;
			door.position = cell.position;
			door.direction = dir;

			ec.ConnectCells(cell.position, dir);

			cell.HardCoverWall(dir, true); // Cell
			ec.CellFromPosition(cell.position + dir.ToIntVector2())
				.HardCoverWall(dir.GetOpposite(), true); // The other cell next

			door.transform.position = cell.FloorWorldPosition;
			door.transform.rotation = dir.ToRotation();
			return;
		}

		[SerializeField]
		internal SmallDoor doorPre;

		[SerializeField]
		[Range(0f, 1f)]
		internal float roomWithSmallDoorFactor = 0.5f;
	}
}
