using BBTimes.CustomComponents;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
// using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace BBTimes.Extensions
{
	public static class GameExtensions
	{

		//static readonly FieldInfo ec_lightMap = AccessTools.Field(typeof(EnvironmentController), "lightMap");
		//static readonly FieldInfo funcContainer_funcs = AccessTools.Field(typeof(RoomFunctionContainer), "functions");

		public static Window ForceBuildWindow(this EnvironmentController ec, Cell tile, Direction dir, WindowObject wObject)
		{
			if (ec.ContainsCoordinates(tile.position + dir.ToIntVector2()))
			{
				var cell = ec.CellFromPosition(tile.position + dir.ToIntVector2());

				if (cell.Null)
					cell.room.wallTex = tile.room.wallTex;

				IntVector2 position = tile.position;
				Window window = Object.Instantiate(wObject.windowPre, tile.room.transform);
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
			LightController[,] lightMap = ec.lightMap; //(LightController[,])ec_lightMap.GetValue(ec);
			tile.hasLight = true;
			tile.lightOn = true;
			tile.lightStrength = 1;
			tile.lightColor = color;

			lightMap[tile.position.x, tile.position.z].AddSource(tile, tile.lightStrength);
			Singleton<CoreGameManager>.Instance.UpdateLighting(color, tile.position);
			lightMap[tile.position.x, tile.position.z].UpdateLighting();

			//ec_lightMap.SetValue(ec, lightMap);
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
				int num = Random.Range(0, lights.Count);
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

		public static void RemoveFunction(this RoomFunctionContainer container, RoomFunction function) =>
			container.functions.Remove(function);
		

		public static BoxCollider AddBoxCollider(this GameObject g, Vector3 center, Vector3 size, bool isTrigger)
		{
			var c = g.AddComponent<BoxCollider>();
			c.center = center;
			c.size = size;
			c.isTrigger = isTrigger;
			return c;
		}
		public static NavMeshObstacle AddNavObstacle(this GameObject g, Vector3 size) =>
			g.AddNavObstacle(Vector3.zero, size);
		public static NavMeshObstacle AddNavObstacle(this GameObject g, Vector3 center, Vector3 size)
		{
			var nav = g.AddComponent<NavMeshObstacle>();
			nav.center = center;
			nav.size = size;
			nav.carving = true;
			return nav;
		}

		public static WeightedTexture2D ToWeightedTexture(this WeightedSelection<Texture2D> t) =>
			new() { selection = t.selection, weight = t.weight };

		public static PlayerAttributesComponent GetAttribute(this PlayerManager pm) =>
			pm.GetComponent<PlayerAttributesComponent>();

		public static GameObject SetAsPrefab(this GameObject obj, bool active)
		{
			obj.ConvertToPrefab(active);
			return obj;
		}

		public static T SafeInstantiate<T>(this T obj) where T : Component
		{
			obj.gameObject.SetActive(false);
			var inst = Object.Instantiate(obj); // Instantiate a deactivated object, so Awake() calls aren't *called*
			obj.gameObject.SetActive(true);

			return inst;
		}

		public static T SafeDuplicatePrefab<T>(this T obj, bool setActive) where T : Component
		{
			obj.gameObject.SetActive(false);

			var inst = obj.DuplicatePrefab();
			inst.gameObject.SetActive(setActive);

			obj.gameObject.SetActive(true);

			return inst;
		}

		public static void RotateSmoothlyToNextPoint(this Transform transform, Vector3 nextPoint, float speed)
		{
			Vector3 vector = Vector3.RotateTowards(transform.forward, (nextPoint - transform.position).normalized, Time.deltaTime * 2f * Mathf.PI * speed, 0f);
			if (vector != Vector3.zero)
				transform.rotation = Quaternion.LookRotation(vector, Vector3.up);
		}

		public static void SendToDetention(this PlayerManager pm, float time, int detentionNoise = defaultDetentionNoise) =>
			SendToDetention(pm, detentionUiPre, time, detentionNoise);

		public static void SendToDetention(this PlayerManager pm, DetentionUi detentionUiPre, float time, int detentionNoise = defaultDetentionNoise)
		{
			if (pm.ec.offices.Count > 0)
			{
				int num = Random.Range(0, pm.ec.offices.Count);
				pm.Teleport(pm.ec.RealRoomMid(pm.ec.offices[num]));
				pm.ClearGuilt();
				pm.ec.offices[num].functionObject.GetComponent<DetentionRoomFunction>().Activate(time, pm.ec);
				var detentionUi = Object.Instantiate(detentionUiPre);
				detentionUi.Initialize(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam, time, pm.ec);

				Baldi baldi = pm.ec.GetBaldi();
				baldi?.ClearSoundLocations();
				pm.ec.MakeNoise(pm.transform.position, detentionNoise);
			}
		}
		public static void BlockAllDirs(this EnvironmentController ec, Vector3 pos, bool block) =>
			ec.BlockAllDirs(IntVector2.GetGridPosition(pos), block);

		public static void BlockAllDirs(this EnvironmentController ec, IntVector2 pos, bool block)
		{
			ec.FreezeNavigationUpdates(true);
			var origin = ec.CellFromPosition(pos);
			for (int i = 0; i < 4; i++)
			{
				var dir = (Direction)i;
				var cell = ec.CellFromPosition(pos + dir.ToIntVector2());
				if (origin.ConstNavigable(dir))
					cell.Block(dir.GetOpposite(), block);
			}
			ec.FreezeNavigationUpdates(false);
		}

		internal static DetentionUi detentionUiPre;

		const int defaultDetentionNoise = 95;
	}
}
