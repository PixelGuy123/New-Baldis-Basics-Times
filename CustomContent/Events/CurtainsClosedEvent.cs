
using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class CurtainsClosedEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{

			eventIntro = this.GetSound("Bal_curtainsClosed.wav", "Event_CurtClosed0", SoundType.Voice, Color.green);
			eventIntro.additionalKeys = [new() { time = 6.155f, key = "Event_CurtClosed1" }, new() { time = 9.861f, key = "Event_CurtClosed2" }];

			var curt = new GameObject("Curtain").AddComponent<Curtains>();
			curt.gameObject.ConvertToPrefab(true);
			curt.audClose = this.GetSound("curtainClose.wav", "Vfx_Curtain_Slide", SoundType.Effect, Color.white);
			curt.audOpen = this.GetSound("curtainOpen.wav", "Vfx_Curtain_Slide", SoundType.Effect, Color.white);

			var storedSprites = this.GetSpriteSheet(2, 1, 25.25f, "curtains.png");
			curt.sprClosed = storedSprites[0];
			curt.sprOpen = storedSprites[1];
			curt.audMan = curt.gameObject.CreatePropagatedAudioManager(65, 85);

			var curtFace = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1], false);
			curtFace.transform.SetParent(curt.transform);
			curtFace.transform.localPosition = Vector3.up * 5.14f;
			curtFace.name = "CurtainFace";

			curt.renderer = curtFace;

			var colliderObj = new GameObject("CurtainBlockRaycast")
			{
				layer = LayerStorage.blockRaycast
			};
			colliderObj.transform.SetParent(curt.transform);
			colliderObj.transform.localPosition = Vector3.up * 5f;

			curt.collider = colliderObj.gameObject.AddBoxCollider(Vector3.zero, new(10f, 10f, 1f), true);
			curt.collider.enabled = false;

			curtPre = curt;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "events";

		// ---------------------------------------------------
		public override void AfterUpdateSetup(System.Random rng)
		{
			base.AfterUpdateSetup(rng);

			// Based on Structure_Hall thing; should fix the issue with curtains spawning over swinging doors
			int targetCurtains = rng.Next(minCurtains, maxCurtains + 1);
			List<List<Cell>> allHallways = ec.FindHallways();
			int placedCurtains = 0;
			int hallwayIndex = 0;

			while (hallwayIndex < allHallways.Count && placedCurtains < targetCurtains)
			{
				List<Cell> currentHallway = allHallways[hallwayIndex];
				List<Cell> straightCells = [];
				foreach (Cell cell in currentHallway) // Get all straight cells to be used
				{
					if (cell.shape.HasFlag(TileShapeMask.Straight))
					{
						straightCells.Add(cell);
					}
				}

				if (straightCells.Count == 0) // If no straight cells found, move to the next hallway
				{
					hallwayIndex++;
					continue;
				}


				Cell selectedCell = straightCells[rng.Next(0, straightCells.Count)];
				List<Direction> validDirections = Directions.OpenDirectionsFromBin(selectedCell.ConstBin);
				selectedCell.FilterDirectionsThroughHardCoverage(validDirections, false);

				for (int i = 0; i < validDirections.Count; i++)
				{
					Cell neighbor = ec.CellFromPosition(selectedCell.position + validDirections[i].ToIntVector2());
					if (neighbor.WallHardCovered(validDirections[i].GetOpposite()))
					{
						validDirections.RemoveAt(i);
						i--;
					}
				}

				if (validDirections.Count != 0 && !selectedCell.HasAnyHardCoverage)
				{
					Direction placementDirection = validDirections[rng.Next(0, validDirections.Count)];

					var curt = Instantiate(curtPre, selectedCell.TileTransform);
					curt.transform.localPosition = placementDirection.ToVector3() * 5f;
					curt.transform.rotation = placementDirection.ToRotation();
					curt.AttachToCell(ec, selectedCell, placementDirection);

					selectedCell.AddRenderer(curt.renderer);
					curtains.Add(curt);

					selectedCell.HardCoverWall(placementDirection, true);
					ec.CellFromPosition(selectedCell.position + placementDirection.ToIntVector2()).HardCoverWall(placementDirection.GetOpposite(), true);

					placedCurtains++;
				}


				hallwayIndex++;
			}
		}

		public override void Begin()
		{
			base.Begin();
			StartCoroutine(ClosePerGroup());
		}

		IEnumerator ClosePerGroup()
		{
			yield return null;
			for (int i = 0; i < curtains.Count; i++)
			{
				curtains[i].Close(true);
				if (i % 3 == 0)
					yield return null;
			}
		}

		public override void End()
		{
			base.End();
			curtains.ForEach(x => x.TimedClose(false, crng.Next(1, 15)));
		}

		readonly List<Curtains> curtains = [];

		[SerializeField]
		internal Curtains curtPre;

		[SerializeField]
		internal int minCurtains = 9, maxCurtains = 16;
	}
}
