using System.Collections.Generic;
using System.Linq;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.ModPatches.EnvironmentPatches
{
	[HarmonyPatch(typeof(EnvironmentController), "BuildNavMesh")]
	internal class EnvironmentControllerMakeBeautifulOutside
	{
		public static WindowObject window;

		[HarmonyPostfix]
		private static void CoverEmptyWallsFromOutside(EnvironmentController __instance)
		{
			try
			{
				var lg = LevelGeneratorInstanceGrabber.i;

				if (BBTimesManager.plug.disableOutside.Value || lg == null) // Make sure this only happens in generated maps
					return;

				// Current way to deal with level objects I guess lol
				var ld = Singleton<BaseGameManager>.Instance.levelObject;
				if (ld is CustomLevelGenerationParameters cld)
				{
					var modVal = cld.GetCustomModValue(BBTimesManager.plug.Info, "Times_GenConfig_DisableOutside");
					if (modVal == null || ((bool)modVal)) // If the mod val is null or it is actually true: disable outside
						return;
				}


				Debug.Log("TIMES: Creating windows for outside...");

				List<Window> spawnedWindows = [];
				Dictionary<Cell, Direction[]> tiles = [];
				var cells = __instance.mainHall.GetNewTileList();

				for (int i = 0; i < __instance.rooms.Count; i++)
					cells.AddRange(__instance.rooms[i].GetNewTileList());

				foreach (var t in cells)
				{
					if (t.Hidden || t.offLimits || !t.HasAllFreeWall) // No elevator tiles or invalid tiles
						continue;
					// A quick fix for the walls


					var dirs = Directions.All();
					dirs.RemoveAll(x => !__instance.CellFromPosition(t.position + x.ToIntVector2()).Null || t.WallAnyCovered(x));

					if (dirs.Count > 0)
						tiles.Add(t, [.. dirs]);
					lg.FrameShouldEnd(); // fail safe to not crash for no f reason
				}

				if (tiles.Count == 0)
					return;



				foreach (var tile in tiles)
				{
					if (lg.controlledRNG.NextDouble() >= 0.95f)
					{
						var dir = tile.Value[lg.controlledRNG.Next(tile.Value.Length)];
						var w = __instance.ForceBuildWindow(tile.Key, dir, window);
						if (w != null)
						{
							w.aTile.AddRenderer(w.windows[0]); // A small optimization
							spawnedWindows.Add(w);
						}
					}
					lg.FrameShouldEnd();
				}


				Debug.Log("TIMES: Creating outside...");


				List<KeyValuePair<IntVector2, KeyValuePair<Direction, Renderer>>> availableMeshes = []; // Has direction as well to avoid renderers that are adjacent to a visible window, which still makes them impossible to be seen
																										// Color color = Singleton<BaseGameManager>.Instance.GetComponent<MainGameManagerExtraComponent>()?.outsideLighting ?? Color.white; // Get the lighting

				var plane = Instantiate(BBTimesManager.man.Get<GameObject>("PlaneTemplate"));
				var renderer = plane.GetComponent<MeshRenderer>();
				renderer.enabled = false;
				renderer.material.mainTexture = __instance.mainHall.wallTex;

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
							availableMeshes.Add(new(t.position, new(dir, p.GetComponentInChildren<Renderer>())));
						}
					}

				}


				if (!isFirstFloor)
					goto end;

				plane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				// Make new instance to not mess up walls
				renderer.material = mats[0];
				plane.name = "GrassPlane";
				System.Random rng = new(lg.controlledRNG.Next());

				foreach (var t in __instance.AllExistentCells())
				{
					if (!t.Null || t.Hidden || t.offLimits) continue;
					var p = Instantiate(plane, planeCover.transform);
					p.transform.localPosition = t.FloorWorldPosition;
					Singleton<CoreGameManager>.Instance.UpdateLighting(Color.white, t.position);
					//AddDebugPosText(t, p.transform);

					availableMeshes.Add(new(t.position, new(Direction.Null, p.GetComponentInChildren<Renderer>())));

					if (decorations.Length > 0 && rng.NextDouble() > 0.75d)
					{
						int amount = rng.Next(1, 4);
						for (int i = 0; i < amount; i++)
						{
							var d = Instantiate(decorations[rng.Next(decorations.Length)], planeCover.transform);
							d.transform.localPosition = t.FloorWorldPosition + new Vector3(((float)rng.NextDouble() * 2f) - 1, 0f, ((float)rng.NextDouble() * 2f) - 1);
							availableMeshes.Add(new(t.position, new(Direction.Null, d.GetComponentInChildren<Renderer>())));
						}
					}


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
						availableMeshes.Add(new(t.position, new(dir, p.GetComponentInChildren<Renderer>())));
					}
				}



			end:
				Destroy(plane);


				List<KeyValuePair<Cell, Renderer>> visibleRenderers = [];
				var nullCull = __instance.CullingManager.GetComponent<NullCullingManager>(); // Get the NullCullingManager

				spawnedWindows.ForEach(window =>
				{
					Cell normCell = window.aTile.Null ? window.bTile : window.aTile;
					Cell oppoCell = !window.aTile.Null ? window.bTile : window.aTile;

					BreastFirstSearch(normCell, oppoCell.position, window.direction.GetOpposite(),
					oppoCell, __instance.CellFromPosition(oppoCell.position + window.direction.ToIntVector2()));
				});


				for (int i = 0; i < availableMeshes.Count; i++)
				{
					bool hasBeenAdded = false;
					for (int z = 0; z < visibleRenderers.Count; z++)
					{
						if (visibleRenderers[z].Value == availableMeshes[i].Value.Value)
						{
							nullCull.AddRendererToCell(visibleRenderers[z].Key, visibleRenderers[z].Value); // Add all the available renders to the corresponding chunks to be properly culled by a secondary Culling Manager
							hasBeenAdded = true;
						}
					}
					if (!hasBeenAdded)
					{
						Destroy(availableMeshes[i].Value.Value);
						availableMeshes.RemoveAt(i--);
					}
				}

				Debug.Log("TIMES: Outside created successfully!");


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
								cellReferences.Any(cellReference => Raycast(cellReference.FloorWorldPosition, prevCell.FloorWorldPosition))) // If cell is null, add the renderers from it
							{
								accessedTiles.Add(nextPos);
								tilesToAccess.Enqueue(nextPos);
							}
						}
						visibleRenderers.AddRange(availableMeshes.Where(x => x.Key == cellReferences[0].position || (x.Key == curPos &&
						x.Value.Key != forbiddenDirection))
							.Select(x => new KeyValuePair<Cell, Renderer>(ogCell, x.Value.Value)));

						accessedTiles.Add(curPos); // Accessed that tile then
					}

				}

				bool Raycast(Vector3 startCell, Vector3 targetCell) // Not using Physics.Raycast because collision isn't really trustful
				{
					Vector3 posToFollow = startCell;
					Vector3 dir = (targetCell - posToFollow).normalized * rayCastDistance;

					Cell cell = __instance.CellFromPosition(startCell);
					var tarCell = __instance.CellFromPosition(targetCell);

					while (cell != tarCell)
					{
						posToFollow += dir;
						cell = __instance.CellFromPosition(posToFollow);
						if (!cell.Null)
							return false;
					}
					return true;
				}

				//void AddDebugPosText(Cell t, Transform reference)
				//{
				//	var text = new GameObject("DebugPositionText_(" + t.position.ToString() + ')').AddComponent<TextMeshPro>();
				//	text.gameObject.layer = LayerStorage.billboardLayer;

				//	text.transform.SetParent(reference);
				//	text.transform.localPosition = Vector3.up * 0.1f;
				//	text.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
				//	text.alignment = TextAlignmentOptions.Center;
				//	text.rectTransform.offsetMin = new(-4f, -3.99f);
				//	text.rectTransform.offsetMax = new(4f, 4.01f);
				//	text.text = t.position.ToString();
				//}
			}
			catch (System.Exception e)
			{
				Debug.LogError("TIMES: Failed to create outside!");
				Debug.LogException(e);
				throw e; // Rethrow the exception to not break the game
			}
		}

		const float rayCastDistance = LayerStorage.TileBaseOffset;

		static Material[] mats = [];

		public static GameObject[] decorations = [];
	}
}
