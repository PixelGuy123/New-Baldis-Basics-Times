using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using PixelInternalAPI.Classes;
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
			
			if ((bool)BBTimesManager.plug.disableOutside.BoxedValue || PostRoomCreation.i == null) // Make sure this only happens in generated maps
				return;

			List<Renderer> availableMeshes = [];
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
						availableMeshes.Add(p.GetComponent<MeshRenderer>());
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
						availableMeshes.AddRange(d.GetComponent<RendererContainer>().renderers);
					}
				}

				availableMeshes.Add(p.GetComponent<MeshRenderer>());
			}
			try
			{

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
						availableMeshes.Add(p.GetComponent<MeshRenderer>());
						//p.SetActive(true);
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}


		end:
			Destroy(plane);

			// Failed trying to add proper culling for outside
			//Ray ray = new();
			//foreach (var w in PostRoomCreation.spawnedWindows)
			//{
			//	foreach (var rend in availableMeshes)
			//	{
			//		ray.origin = rend.transform.position.ZeroOutY();
			//		ray.direction = (w.transform.position - ray.origin).normalized;

			//		if (Physics.Raycast(ray, out var hit) && hit.transform)
			//			w.aTile.AddRenderer(rend);
			//	}
			//}
		}

		static Material[] mats = [];

		public static GameObject[] decorations = [];
	}
}
