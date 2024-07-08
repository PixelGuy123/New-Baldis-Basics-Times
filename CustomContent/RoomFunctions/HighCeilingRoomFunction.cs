using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BBTimes.CustomContent.RoomFunctions
{
	internal class HighCeilingRoomFunction : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			base.Build(builder, rng);
			if (ceilingHeight < 1)
				return;

			if (rng.NextDouble() > chanceToHappen)
				return;

			room.ceilTex = ObjectCreationExtension.transparentTex;
			room.GenerateTextureAtlas();

			if (customLight != null)
				room.lightPre = customLight;
			else
				room.lightPre = BBTimesManager.EmptyGameObject.transform; // Literally an empty gameObject

			
			var planeHolder = new GameObject("PlaneCover");
			planeHolder.transform.SetParent(room.transform);
			planeHolder.transform.localPosition = Vector3.zero;

			var fullTex = TextureExtensions.GenerateTextureAtlas(ObjectCreationExtension.transparentTex, usesSingleCustomWall ? customWallProximityToCeil[0] : room.wallTex, ObjectCreationExtension.transparentTex);

			int offset = 0;

			for (int i = 1; i <= ceilingHeight; i++)
			{
				if (i > ceilingHeight - customWallProximityToCeil.Length)
					fullTex = TextureExtensions.GenerateTextureAtlas(ObjectCreationExtension.transparentTex, customWallProximityToCeil[offset++], ObjectCreationExtension.transparentTex);



				//CreateWalls(room.position, Direction.South, 1, 0, room.size.x);
				//CreateWalls(room.position, Direction.West, 0, 1, room.size.z);
				//CreateWalls(room.position + room.size - new IntVector2(1, 1), Direction.North, -1, 0, room.size.x);
				//CreateWalls(room.position + room.size - new IntVector2(1, 1), Direction.East, 0, -1, room.size.z);

				foreach (var cell in ogCellBins)
						AddWalls(cell.Key, cell.Value);

				void AddWalls(Cell c, int ogbin)
				{
					if (ogbin == 0) return;

					int bin = c.ConstBin; // Save it to reset it

					room.ec.SwapCell(c.position, c.room, ogbin);
					var tile = Instantiate(c.Tile);
					tile.transform.SetParent(planeHolder.transform);
					tile.transform.position = c.FloorWorldPosition + (Vector3.up * (LayerStorage.TileBaseOffset * i));
					tile.MeshRenderer.material.mainTexture = fullTex;
					c.AddRenderer(tile.MeshRenderer);

					room.ec.SwapCell(c.position, c.room, bin);

				}
			}
			if (!string.IsNullOrEmpty(targetTransformNamePrefix) && targetTransformOffset > 0f)
			{
				var objects = room.transform.Find("RoomObjects");

				foreach (var obj in objects.AllChilds())
				{
					if (!obj.name.StartsWith(targetTransformNamePrefix))
						continue;
					for (int i = 1; i <= ceilingHeight; i++)
					{
						var clone = Instantiate(obj, objects);
						clone.name = obj.name;
						clone.transform.position = obj.transform.position + (Vector3.up * (i * targetTransformOffset));
						clone.transform.rotation = obj.transform.rotation;

						var collider = clone.GetComponent<Collider>();
						if (collider != null)
							Destroy(collider);

						var nav = clone.GetComponent<NavMeshObstacle>();
						if (nav != null)
							Destroy(nav);
					}
				}
			}

			if (!hasCeiling)
				return;

			fullTex = TextureExtensions.GenerateTextureAtlas(customCeiling ?? originalCeilTex, ObjectCreationExtension.transparentTex, ObjectCreationExtension.transparentTex);

			foreach (var c in room.cells)
			{
				var tile = Instantiate(c.Tile, planeHolder.transform);
				tile.transform.position = c.FloorWorldPosition + (Vector3.up * (ceilingHeight * LayerStorage.TileBaseOffset));
				tile.MeshRenderer.material.mainTexture = fullTex;
				c.AddRenderer(tile.MeshRenderer);
			}

		}

		public override void Initialize(RoomController room)
		{
			base.Initialize(room);
			originalCeilTex = room.ceilTex;
			foreach (var c in room.cells)
				ogCellBins.Add(c, c.ConstBin);
		}

		Texture2D originalCeilTex;

		[SerializeField]
		public string targetTransformNamePrefix = string.Empty;

		[SerializeField]
		public float targetTransformOffset = 1f;

		[SerializeField]
		public int ceilingHeight = 1;

		[SerializeField]
		public bool hasCeiling = true, usesSingleCustomWall = false;

		[SerializeField]
		public Transform customLight = null;

		[SerializeField]
		public float chanceToHappen = 1f;

		[SerializeField]
		public Texture2D customCeiling = null;

		[SerializeField]
		public Texture2D[] customWallProximityToCeil = [];

		readonly Dictionary<Cell, int> ogCellBins = [];
	}
}
