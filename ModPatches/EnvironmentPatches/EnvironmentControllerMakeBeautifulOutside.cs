using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using Rewired.UI.ControlMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.ModPatches.EnvironmentPatches
{
	[HarmonyPatch(typeof(EnvironmentController), "BuildNavMesh")]
	internal class EnvironmentControllerMakeBeautifulOutside
	{
		[HarmonyPostfix]
		private static void CoverEmptyWallsFromOutside(EnvironmentController __instance)
		{

			if ((bool)BBTimesManager.plug.disableOutside.BoxedValue || PostRoomCreation.i == null) // Make sure this only happens in generated maps
				return;

			List<KeyValuePair<IntVector2, Renderer>> availableMeshes = [];
			// Color color = Singleton<BaseGameManager>.Instance.GetComponent<MainGameManagerExtraComponent>()?.outsideLighting ?? Color.white; // Get the lighting

			var plane = Instantiate(BBTimesManager.man.Get<GameObject>("PlaneTemplate"));
			var renderer = plane.GetComponent<MeshRenderer>();
			renderer.material.mainTexture = __instance.mainHall.wallTex;
			//renderer.enabled = false;
			DestroyImmediate(plane.GetComponent<MeshCollider>()); // No collision needed

			if (mats.Length == 0) // If Not initialized
			{
				mats = [ // to not create a new material each generation
					new(renderer.material) { mainTexture =  BBTimesManager.man.Get<Texture2D>("Tex_Grass") },
					new(BBTimesManager.man.Get<GameObject>("TransparentPlaneTemplate").GetComponent<MeshRenderer>().material) { mainTexture = BBTimesManager.man.Get<Texture2D>("Tex_Fence") }
				];
			}

			var planeCover = new GameObject("PlaneCover");
			planeCover.transform.SetParent(__instance.transform);
			planeCover.transform.localPosition = Vector3.zero;

			plane.transform.SetParent(planeCover.transform);

			bool isFirstFloor = BBTimesManager.CurrentFloor == "F1" || BBTimesManager.CurrentFloor == "END";
			bool lastFloor = BBTimesManager.CurrentFloor == "F3";

			foreach (var t in __instance.AllExistentCells())
			{
				if (!t.Null || t.Hidden || t.offLimits) continue;

				List<Direction> dirs = Directions.All();
				for (int i = 0; i < dirs.Count; i++)
				{
					var pos = t.position + dirs[i].ToIntVector2();
					var tile = __instance.CellFromPosition(pos);
					if (tile.Null) // Does not check for windows anymore, to hide what's behind it
						dirs.RemoveAt(i--);
				}


				if (dirs.Count == 0) continue;

				int max = lastFloor ? 6 : 12;
				int start = isFirstFloor ? 0 : -8;
				Singleton<CoreGameManager>.Instance.UpdateLighting(Color.white, t.position);

				foreach (var dir in dirs)
				{
					for (int i = start; i <= max; i++)
					{
						var p = Instantiate(plane, planeCover.transform);
						p.transform.localRotation = dir.ToRotation();
						p.transform.localPosition = t.CenterWorldPosition + (dir.ToVector3() * ((LayerStorage.TileBaseOffset / 2f) - 0.001f)) + (Vector3.up * LayerStorage.TileBaseOffset * i);
						availableMeshes.Add(new(t.position, p.GetComponent<MeshRenderer>()));
					}
				}

			}


			if (!isFirstFloor)
				goto end;

			plane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
			// Make new instance to not mess up walls
			renderer.material = mats[0];
			plane.name = "GrassPlane";
			System.Random rng = new(PostRoomCreation.i.controlledRNG.Next());

			foreach (var t in __instance.AllExistentCells())
			{
				if (!t.Null || t.Hidden || t.offLimits) continue;
				var p = Instantiate(plane, planeCover.transform);
				p.transform.localPosition = t.FloorWorldPosition;
				Singleton<CoreGameManager>.Instance.UpdateLighting(Color.white, t.position);

				if (decorations.Length > 0 && rng.NextDouble() > 0.75d)
				{
					int amount = rng.Next(1, 4);
					for (int i = 0; i < amount; i++)
					{
						var d = Instantiate(decorations[rng.Next(decorations.Length)], planeCover.transform);
						d.transform.localPosition = t.FloorWorldPosition + new Vector3(((float)rng.NextDouble() * 2f) - 1, 0f, ((float)rng.NextDouble() * 2f) - 1);
						availableMeshes.AddRange(d.GetComponent<RendererContainer>().renderers.ConvertAll(x => new KeyValuePair<IntVector2, Renderer>(t.position, x)));
					}
				}

				availableMeshes.Add(new(t.position, p.GetComponent<MeshRenderer>()));
			}

			renderer.material = mats[1];
			plane.name = "FencePlane";
			plane.transform.localRotation = Quaternion.identity;

			foreach (var t in __instance.AllExistentCells())
			{
				if (!t.Null) continue;

				foreach (var dir in Directions.All())
				{
					if (__instance.ContainsCoordinates(t.position + dir.ToIntVector2())) continue;

					var p = Instantiate(plane, planeCover.transform);
					p.transform.localRotation = dir.ToRotation();
					p.transform.localPosition = t.CenterWorldPosition + (dir.ToVector3() * ((LayerStorage.TileBaseOffset / 2f) - 0.01f));
					availableMeshes.Add(new(t.position, p.GetComponent<MeshRenderer>()));
				}
			}



		end:
			Destroy(plane);


			List<KeyValuePair<Cell, Renderer>> visibleRenderers = [];
			var nullCull = __instance.CullingManager.GetComponent<NullCullingManager>(); // Get the NullCullingManager
			try
			{
				PostRoomCreation.spawnedWindows.ForEach(window =>
					BreastFirstSearch(window.aTile, window.bTile.position, window.direction.GetOpposite(),
					window.bTile,
					__instance.CellFromPosition(window.bTile.position + window.direction.PerpendicularList()[0].ToIntVector2()),
					__instance.CellFromPosition(window.bTile.position + window.direction.PerpendicularList()[1].ToIntVector2())
					));
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}


			for (int i = 0; i < availableMeshes.Count; i++)
			{
				bool hasBeenAdded = false;
				for (int z = 0; z < visibleRenderers.Count; z++)
				{
					if (visibleRenderers[z].Value == availableMeshes[i].Value)
					{
						nullCull.AddRendererToCell(visibleRenderers[i].Key, visibleRenderers[i].Value); // Add all the available renders to the corresponding chunks to be properly culled by a secondary Culling Manager
						hasBeenAdded = true;
					}
				}
				if (!hasBeenAdded)
				{
					Destroy(availableMeshes[i].Value);
					availableMeshes.RemoveAt(i--);
				}
			}


			void BreastFirstSearch(Cell ogCell, IntVector2 pos, Direction forbiddenDirection, params Cell[] cellReferences)
			{
				HashSet<IntVector2> accessedTiles = [];
				Queue<IntVector2> tilesToAccess = [];
				tilesToAccess.Enqueue(pos);
				var dirsToFollow = Directions.All();
				dirsToFollow.Remove(forbiddenDirection);

				if (!__instance.CellFromPosition(pos).Null)
					return;

				while (tilesToAccess.Count != 0) // Until the queue is empty
				{
					var curPos = tilesToAccess.Dequeue();
					for (int i = 0; i < dirsToFollow.Count; i++)
					{
						var nextPos = curPos + dirsToFollow[i].ToIntVector2();
						var cell = __instance.CellFromPosition(nextPos);
						var prevCell = __instance.CellFromPosition(curPos);

						if (cell.Null &&
							__instance.ContainsCoordinates(nextPos) &&
							!accessedTiles.Contains(nextPos) &&
							cellReferences.Any(x => Raycast(x, prevCell))) // If cell is null, add the renderers from it
						{
							accessedTiles.Add(nextPos);
							tilesToAccess.Enqueue(nextPos);
						}
					}
					visibleRenderers.AddRange(availableMeshes.Where(x => x.Key == curPos).Select(x => new KeyValuePair<Cell, Renderer>(ogCell, x.Value)));
					accessedTiles.Add(curPos); // Accessed that tile then
				}
			}

			bool Raycast(Cell startCell, Cell targetCell) // Not using Physics.Raycast because collision isn't really trustful
			{
				Vector3 posToFollow = startCell.FloorWorldPosition;
				Vector3 dir = (targetCell.FloorWorldPosition - posToFollow).normalized * 10f;
				Cell cell = startCell;

				while (cell != targetCell)
				{
					posToFollow += dir;
					cell = __instance.CellFromPosition(posToFollow);
					if (!cell.Null)
						return false;
				} 
				return true;
			}
		}

		static Material[] mats = [];

		public static GameObject[] decorations = [];
	}
}
