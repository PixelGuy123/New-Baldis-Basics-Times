using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.Builders
{

    public class TrapDoorBuilder : ObjectBuilder, IObjectPrefab
	{
		public void SetupPrefab()
		{
			var trapdoorholder = new GameObject("TrapDoor").AddComponent<Trapdoor>();
			trapdoorholder.gameObject.ConvertToPrefab(true);


			var text = new GameObject("TrapdoorText").AddComponent<TextMeshPro>();
			text.gameObject.layer = LayerStorage.billboardLayer;
			text.transform.SetParent(trapdoorholder.transform);
			text.transform.localPosition = Vector3.up * 0.02f;
			text.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			text.alignment = TextAlignmentOptions.Center;
			text.rectTransform.offsetMin = new(-4f, -3.99f);
			text.rectTransform.offsetMax = new(4f, 4.01f);
			trapdoorholder.text = text;

			var collider = trapdoorholder.gameObject.AddComponent<BoxCollider>();
			collider.size = Vector3.one * 4.9f;
			collider.isTrigger = true;

			var builder = GetComponent<TrapDoorBuilder>();
			builder.trapDoorpre = trapdoorholder;

			var trapSprites = this.GetSpriteSheet(2, 2, 25f, "traps.png");

			builder.closedSprites = [trapSprites[0], trapSprites[1]];
			builder.openSprites = [trapSprites[2], trapSprites[3]];

			trapdoorholder.aud_shut = this.GetSound("trapDoor_shut.wav", "Sfx_Doors_StandardShut", SoundType.Voice, Color.white);
			trapdoorholder.aud_open = this.GetSound("trapDoor_open.wav", "Sfx_Doors_StandardOpen", SoundType.Voice, Color.white);

			var trapdoor = ObjectCreationExtensions.CreateSpriteBillboard(builder.closedSprites[0], false);
			trapdoor.transform.SetParent(trapdoorholder.transform); // prefab stuf


			trapdoor.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			trapdoor.transform.localPosition = Vector3.up * 0.02f;
			trapdoor.name = "TrapdoorVisual";
			trapdoor.gameObject.layer = 0; // default layer

			trapdoorholder.renderer = trapdoor;
			trapdoorholder.audMan = trapdoorholder.gameObject.CreatePropagatedAudioManager(35f, 45f);

			// Fake trapdoor
			var fake = trapdoor.SafeDuplicatePrefab(true);
			fake.name = "FakeTrapDoor";
			fake.gameObject.CreatePropagatedAudioManager(35f, 45f);
			trapdoorholder.fakeTrapdoorPre = fake.transform;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");




		// setup prefab ^^
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			builtTraps = [];

			map = new(ec, PathType.Const, []);

			var ecData = ec.GetComponent<EnvironmentControllerData>();

			var t = room.AllTilesNoGarbage(false, false);
			for (int i = 0; i < t.Count; i++)
				if ((t[i].shape != TileShape.Corner && t[i].shape != TileShape.End) || t[i].open)
					t.RemoveAt(i--);


			if (t.Count == 0)
			{
				Debug.LogWarning("No initial spots found for the TrapdoorBuilder");
				return;
			}

			List<WeightedSelection<Cell>> intVectors = t.ConvertAll(x => new WeightedSelection<Cell>() { selection = x, weight = 100 });
			int max = cRng.Next(minAmount, maxAmount + 1);

			for (int i = 0; i < max; i++)
			{
				if (t.Count == 0)
					break;
				int idx = WeightedSelection<Cell>.ControlledRandomIndexList(intVectors, cRng);
				var trap = CreateTrapDoor(intVectors[idx].selection, ec, ecData);
				t.Remove(intVectors[idx].selection);
				intVectors.RemoveAt(idx);

				if (t.Count > 0 && max - i > 1 && cRng.NextDouble() >= 0.55) // Linked trapdoor
				{
					idx = WeightedSelection<Cell>.ControlledRandomIndexList(intVectors, cRng);
					t.Remove(intVectors[idx].selection);

					var strap = CreateTrapDoor(intVectors[idx].selection, ec, ecData);
					trap.SetLinkedTrapDoor(strap);
					strap.SetLinkedTrapDoor(trap);


					trap.renderer.sprite = openSprites[1];
					strap.renderer.sprite = openSprites[1];
					trap.sprites = [closedSprites[1], openSprites[1]];
					strap.sprites = [closedSprites[1], openSprites[1]];
				}
				else
				{
					trap.renderer.sprite = openSprites[0]; // Random trapdoor
					trap.sprites = [closedSprites[0], openSprites[0]];
				}

				intVectors.Clear();
				map.Calculate([.. builtTraps.ConvertAll(x => ec.CellFromPosition(x.transform.position).position)]);
				for (int x = 0; x < t.Count; x++)
				{
					int val = map.Value(t[x].position);
					if (val >= minimumDistanceFromATrapDoor)
						intVectors.Add(new WeightedSelection<Cell>() { selection = t[x], weight = val });
				}
			}

		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir)
		{
			base.Load(ec, pos, dir);
			var ecData = ec.GetComponent<EnvironmentControllerData>();

			for (int i = 0; i < pos.Count; i++)
			{
				var cell = ec.CellFromPosition(pos[i]);

				var trap = CreateTrapDoor(cell, ec, ecData);
				if (dir[i] != Direction.Null) // Means it is a linked trapdoor
				{
					if (++i >= pos.Count)
						throw new System.ArgumentException("A linked trapdoor was flagged with no link available on the next item of the pos collection");

					cell = ec.CellFromPosition(pos[i]); // Updates cell for the next position

					var strap = CreateTrapDoor(cell, ec, ecData); // Linked trapdoor setup
					trap.SetLinkedTrapDoor(strap);
					strap.SetLinkedTrapDoor(trap);

					trap.renderer.sprite = openSprites[1];
					strap.renderer.sprite = openSprites[1];
					trap.sprites = [closedSprites[1], openSprites[1]];
					strap.sprites = [closedSprites[1], openSprites[1]];

					continue;
				} 

				trap.renderer.sprite = openSprites[0]; // Random trapdoor
				trap.sprites = [closedSprites[0], openSprites[0]];
			}
		}

		private Trapdoor CreateTrapDoor(Cell pos, EnvironmentController ec, EnvironmentControllerData dat)
		{
			var trapdoor = Instantiate(trapDoorpre);
			trapdoor.transform.SetParent(pos.TileTransform);
			trapdoor.transform.position = pos.FloorWorldPosition;
			trapdoor.gameObject.SetActive(true);
			trapdoor.SetEC(ec);
			pos.HardCover(CellCoverage.Down | CellCoverage.Center | CellCoverage.East | CellCoverage.North | CellCoverage.South | CellCoverage.West);
			pos.AddRenderer(trapdoor.renderer);
			pos.AddRenderer(trapdoor.text.GetComponent<MeshRenderer>());
			ec.map.AddIcon(icon, trapdoor.transform, Color.white);

			dat.Trapdoors.Add(trapdoor);

			builtTraps?.Add(trapdoor);

			return trapdoor;
		}

		[SerializeField]
		public Trapdoor trapDoorpre;

		[SerializeField]
		public int minAmount = 1, maxAmount = 2, minimumDistanceFromATrapDoor = 10;

		[SerializeField]
		public Sprite[] closedSprites;

		[SerializeField]
		public Sprite[] openSprites;

		List<Trapdoor> builtTraps;

		internal static MapIcon icon;
		DijkstraMap map;
	}
}
