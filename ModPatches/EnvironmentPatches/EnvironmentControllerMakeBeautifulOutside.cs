using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using System.Collections.Generic;
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
			if (PostRoomCreation.i == null) // Make sure this only happens in non-generated maps
				return;

			Color color = Singleton<BaseGameManager>.Instance.GetComponent<MainGameManagerExtraComponent>()?.outsideLighting ?? Color.white; // Get the lighting

			var grassTex = BBTimesManager.man.Get<Texture2D>("Tex_Grass");
			var fenceTex = BBTimesManager.man.Get<Texture2D>("Tex_Fence");
			var plane = Instantiate(BBTimesManager.man.Get<GameObject>("PlaneTemplate"));
			var renderer = plane.GetComponent<MeshRenderer>();
			renderer.material.mainTexture = __instance.mainHall.wallTex;
			DestroyImmediate(plane.GetComponent<MeshCollider>()); // No collision needed

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
					{
						dirs.RemoveAt(i);
						i--;
					}
				}


				__instance.ForceAddPermanentLighting(t, color);

				if (dirs.Count == 0) continue;

				int max = lastFloor ? 2 : 10;

				foreach (var dir in dirs)
				{
					for (int i = isFirstFloor ? 0 : -10; i <= max; i++)
					{
						var p = Instantiate(plane, planeCover.transform);
						p.transform.localRotation = dir.ToRotation();
						p.transform.localPosition = t.CenterWorldPosition + (dir.ToVector3() * 4.99f) + (Vector3.up * t.CenterWorldPosition.y * i);
						// t.AddRenderer(p.GetComponent<MeshRenderer>()); // Should keep this on. Because the render is messed up outside school
						p.SetActive(true);
					}
				}

			}


			if (!isFirstFloor)
				goto end;

			plane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
			renderer.material = new(renderer.material)
			{
				mainTexture = grassTex
			}; // Make new instance to not mess up walls
			plane.name = "GrassPlane";
			System.Random rng = new(PostRoomCreation.i.controlledRNG.Next());

			foreach (var t in __instance.AllExistentCells())
			{
				if (!t.Null || t.Hidden || t.offLimits) continue;
				var p = Instantiate(plane, planeCover.transform);
				p.transform.localPosition = t.FloorWorldPosition;
				__instance.ForceAddPermanentLighting(t, color);

				if (decorations.Length > 0 && rng.NextDouble() > 0.75d)
				{
					int amount = rng.Next(1, 4);
					for (int i = 0; i < amount; i++)
					{
						var d = Instantiate(decorations[rng.Next(decorations.Length)], planeCover.transform);
						d.SetActive(true);
						d.transform.localPosition = t.FloorWorldPosition + new Vector3((float)rng.NextDouble() * 2f - 1, 0f, (float)rng.NextDouble() * 2f - 1);
						d.GetComponent<RendererContainer>().renderers.Do(t.AddRenderer); // I didn't know this was valid syntax, thanks compiler!
					}
				}

				p.SetActive(true);

				t.AddRenderer(p.GetComponent<MeshRenderer>());
			}
			try
			{

				renderer.material = new(renderer.material)
				{
					mainTexture = fenceTex
				};
				plane.name = "FencePlane";
				plane.transform.localRotation = Quaternion.identity;

				foreach (var t in __instance.AllExistentCells())
				{
					foreach (var dir in Directions.All())
					{
						if (__instance.ContainsCoordinates(t.position + dir.ToIntVector2())) continue;

						var p = Instantiate(plane, planeCover.transform);
						p.transform.localRotation = dir.ToRotation();
						p.transform.localPosition = t.CenterWorldPosition + (dir.ToVector3() * 5f);
						t.AddRenderer(p.GetComponent<MeshRenderer>());
						p.SetActive(true);
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}


			end:
			Destroy(plane); // Not needed anymore


		}

		public static GameObject[] decorations = [];
	}
}
