using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.ModPatches.EnvironmentPatches
{
	[HarmonyPatch(typeof(EnvironmentController), "BuildNavMesh")]
	internal class EnvironmentControllerFixWallsOutside
	{
		[HarmonyPostfix]
		private static void CoverEmptyWallsFromOutside(EnvironmentController __instance)
		{
			var grassTex = BBTimesManager.man.Get<Texture2D>("Tex_Grass");
			var plane = Instantiate(BBTimesManager.man.Get<GameObject>("PlaneTemplate"));
			var renderer = plane.GetComponent<MeshRenderer>();
			renderer.material.mainTexture = __instance.mainHall.wallTex;
			DestroyImmediate(plane.GetComponent<MeshCollider>()); // No collision needed

			var planeCover = new GameObject("PlaneCover");
			planeCover.transform.SetParent(__instance.transform);
			planeCover.transform.localPosition = Vector3.zero;

			plane.transform.SetParent(planeCover.transform);

			foreach (var t in __instance.AllExistentCells())
			{
				if (!t.Null || t.Hidden || t.offLimits) continue;

				List<Direction> dirs = Directions.All();
				for (int i = 0; i < dirs.Count; i++)
				{
					var pos = t.position + dirs[i].ToIntVector2();
					var tile = __instance.CellFromPosition(pos);
					if (tile.Null || WindowOutsidePatch.artificallySpawnedWindows.Contains(new KeyValuePair<IntVector2, Direction>(pos, dirs[i].GetOpposite())))
					{
						dirs.RemoveAt(i);
						i--;
					}
				}


				__instance.ForceAddPermanentLighting(t, Color.white);

				if (dirs.Count == 0) continue;

				foreach (var dir in dirs)
				{
					var p = Instantiate(plane, planeCover.transform);
					var q = dir.GetOpposite().ToUiRotation();
					p.transform.localRotation = Quaternion.Euler(90f, q.eulerAngles.y, q.eulerAngles.z);
					p.transform.localPosition = t.CenterWorldPosition + (dir.ToVector3() * 5f);
					//__instance.CellFromPosition(t.position + dir.ToIntVector2()).AddRenderer(p.GetComponent<MeshRenderer>()); // Should keep this on. Because the render is messed up outside school
					p.SetActive(true);
				}
				
			}

			if (BBTimesManager.CurrentFloor != "F1" && BBTimesManager.CurrentFloor != "END")
				goto end;

			plane.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			renderer.material = new(renderer.material)
			{
				mainTexture = grassTex
			}; // Make new instance to not mess up walls
			plane.name = "GrassPlane";

			foreach (var t in __instance.AllExistentCells())
			{
				if (!t.Null || t.Hidden || t.offLimits) continue;
				var p = Instantiate(plane, planeCover.transform);
				p.transform.localPosition = t.FloorWorldPosition;
				__instance.ForceAddPermanentLighting(t, Color.white);
				
				if (decorations.Length > 0)
				{

				}

				p.SetActive(true);
			}

			end:
			Destroy(plane); // Not needed anymore

		}

		public static GameObject[] decorations = [];
	}
}
