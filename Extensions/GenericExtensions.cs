﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BBTimes.Extensions
{
	public static class GenericExtensions
	{

		static readonly FieldInfo ec_lightMap = AccessTools.Field(typeof(EnvironmentController), "lightMap");
		public static void ReplaceAt<T>(this IList<T> list, int index, T replacement)
		{
			list.RemoveAt(index);
			list.Insert(index, replacement);
		}

		public static bool Replace<T>(this IList<T> list, Predicate<T> predicate, T replacement)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicate(list[i]))
				{
					ReplaceAt(list, i, replacement);
					return true;
				}
			}
			return false;
		}

		public static List<Transform> AllChilds(this Transform transform)
		{
			List<Transform> cs = [];
			int count = transform.childCount;
			for (int i = 0; i < count; i++)
				cs.Add(transform.GetChild(i));
			return cs;
		}

		public static void ForceBuildWindow(this EnvironmentController ec, Cell tile, Direction dir, WindowObject wObject)
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
			}
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
	}
}
