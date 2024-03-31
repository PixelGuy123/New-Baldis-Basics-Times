using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BBTimes.Extensions
{
	public static class GameExtensions
	{

		static readonly FieldInfo ec_lightMap = AccessTools.Field(typeof(EnvironmentController), "lightMap");
		static readonly FieldInfo funcContainer_funcs = AccessTools.Field(typeof(RoomFunctionContainer), "functions");

		public static Window ForceBuildWindow(this EnvironmentController ec, Cell tile, Direction dir, WindowObject wObject)
		{
			if (ec.ContainsCoordinates(tile.position + dir.ToIntVector2()))
			{
				var cell = ec.CellFromPosition(tile.position + dir.ToIntVector2());

				if (cell.Null)
					cell.room.wallTex = tile.room.wallTex;

				IntVector2 position = tile.position;
				Window window = UnityEngine.Object.Instantiate(wObject.windowPre, tile.room.transform);
				ec.ConnectCells(tile.position, dir);
				Cell cell2 = ec.CellFromPosition(position);
				window.Initialize(ec, tile.position, dir, wObject);
				cell2.HardCoverWall(dir, true);
				cell = ec.CellFromPosition(tile.position + dir.ToIntVector2());
				cell.HardCoverWall(dir.GetOpposite(), true);
				window.transform.position = tile.FloorWorldPosition;
				window.transform.rotation = dir.ToRotation();
				return window;
			}
			return null;
		}

		public static List<Cell> AllExistentCells(this EnvironmentController ec)
		{
			List<Cell> list = [];
			for (int i = 0; i < ec.levelSize.x; i++)
			{
				for (int j = 0; j < ec.levelSize.z; j++)
				{
					Cell cell = ec.CellFromPosition(i, j);
					list.Add(cell);
				}
			}
			return list;
		}

		public static void ForceAddPermanentLighting(this EnvironmentController ec, Cell tile, Color color)
		{
			tile.permanentLight = true;
			LightController[,] lightMap = ( LightController[,])ec_lightMap.GetValue(ec);
			tile.hasLight = true;
			tile.lightOn = true;
			tile.lightStrength = 1;
			tile.lightColor = color;

			lightMap[tile.position.x, tile.position.z].AddSource(tile, tile.lightStrength);
			Singleton<CoreGameManager>.Instance.UpdateLighting(color, tile.position);
			lightMap[tile.position.x, tile.position.z].UpdateLighting();

			ec_lightMap.SetValue(ec, lightMap);
		}

		public static IEnumerator LightChanger(this EnvironmentController ec, List<Cell> lights, bool on, float delay)
		{
			float time = delay;
			while (lights.Count > 0)
			{
				while (time > 0f)
				{
					time -= Time.deltaTime;
					yield return null;
				}
				time = delay;
				int num = UnityEngine.Random.Range(0, lights.Count);
				lights[num].lightColor = Color.red;
				ec.SetLight(on, lights[num]);
				lights.RemoveAt(num);
			}
			yield break;
		}

		public static IEnumerator InfiniteAnger(Baldi b, float increaser)
		{
			if (increaser <= 0f)
				yield break;

			while (true)
			{
				b.GetAngry(increaser * b.TimeScale);
				yield return null;
			}
		}

		public static void RemoveFunction(this RoomFunctionContainer container, RoomFunction function)
		{
			var list = (List<RoomFunction>)funcContainer_funcs.GetValue(container);
			list.Remove(function);
			funcContainer_funcs.SetValue(container, list);
		}

		public static BoxCollider AddBoxCollider(this GameObject g, Vector3 center, Vector3 size, bool isTrigger)
		{
			var c = g.AddComponent<BoxCollider>();
			c.center = center;
			c.size = size;
			c.isTrigger = isTrigger;
			return c;
		}

		public static WeightedTexture2D ToWeightedTexture(this WeightedSelection<Texture2D> t) =>
			new() { selection = t.selection, weight = t.weight };
	}
}
