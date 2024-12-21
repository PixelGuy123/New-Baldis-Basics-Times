using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using NewPlusDecorations.Components;
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
			proof = builder;

			if (changed || ceilingHeight < 1 || rng.NextDouble() > chanceToHappen)
				return;

			AddAllWalls();
		}

		public override void Initialize(RoomController room)
		{

			base.Initialize(room);
			originalCeilTex = room.ceilTex;

			ogCellBins.Clear();
			room.cells.ForEach(c => ogCellBins.Add(c, c.ConstBin));

			if (tilePrefabRef)
				DestroyImmediate(tilePrefabRef.gameObject);

			tilePrefabRef = new GameObject("TileRef").AddComponent<MeshFilter>();
			var renderer = tilePrefabRef.gameObject.AddComponent<MeshRenderer>();
			renderer.materials = room.ec.tilePre.MeshRenderer.materials;

		}

		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			
			if (!BBTimesManager.plug.disableHighCeilings.Value && (!proof || proof is LevelLoader))
				AddAllWalls(true); // If proof isn't assigned, it means this must be LevelLoader

			if (changed)
				foreach (var c in room.cells)
					c.SetBase(c.Tile.MeshRenderer.material.name.StartsWith(room.defaultPosterMat.name) ? room.posterMat : room.baseMat); // base mat should be alpha now

			Destroy(tilePrefabRef.gameObject);
		}

		void AddAllWalls(bool levelLoader = false)
		{
			changed = true;

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



				foreach (var cell in ogCellBins)
					AddWalls(cell.Key, cell.Value);

				void AddWalls(Cell c, int ogbin)
				{

					if (levelLoader)
					{
						var dirs = Directions.All();
						for (int i = 0; i < dirs.Count; i++)
							if (!ogbin.IsBitSet(dirs[i].BitPosition()) && (c.doorDirs.Contains(dirs[i]) || !c.NavNavigable(dirs[i]))) // Should account for windows and doors
								ogbin = ogbin.ToggleBit(dirs[i].BitPosition()); // For pre-loaded rooms, the tiles will always have a door placement that will open them, so it's better closing them by toggling their bit.
					}
					if (ogbin == 0) return;

					var tile = Instantiate(tilePrefabRef, planeHolder.transform);

					tile.sharedMesh = room.ec.TileMesh(ogbin);

					tile.transform.position = c.FloorWorldPosition + (Vector3.up * (LayerStorage.TileBaseOffset * i));
					var render = tile.GetComponent<MeshRenderer>();
					render.material = room.defaultAlphaMat;
					render.material.mainTexture = fullTex;
					c.AddRenderer(render);

				}
			}
			var objects = room.objectObject;
			if (!string.IsNullOrEmpty(targetTransformNamePrefix) && targetTransformOffset > 0f)
				SearchChildsBasedOnCriteria(objects.transform, obj => obj.name.StartsWith(targetTransformNamePrefix), null, targetTransformOffset);

			SearchChildsBasedOnCriteria(objects.transform, obj => obj.GetComponent<Column>(), obj =>
			{
				if (proof)
					EnvironmentObject.GiveController(obj, room.ec, proof.environmentObjects);
			}, LayerStorage.TileBaseOffset);

			if (!hasCeiling)
				return;

			fullTex = TextureExtensions.GenerateTextureAtlas(customCeiling ?? originalCeilTex, ObjectCreationExtension.transparentTex, ObjectCreationExtension.transparentTex);

			foreach (var c in room.cells)
			{
				var tile = Instantiate(tilePrefabRef, planeHolder.transform);
				tile.sharedMesh = room.ec.TileMesh(0); // open tile

				tile.transform.position = c.FloorWorldPosition + (Vector3.up * (ceilingHeight * LayerStorage.TileBaseOffset));
				var rend = tile.GetComponent<MeshRenderer>();
				rend.material = room.defaultAlphaMat;
				rend.material.mainTexture = fullTex;
				c.AddRenderer(rend);
			}
		}

		void SearchChildsBasedOnCriteria(Transform objects, System.Predicate<Transform> predicate, System.Action<Transform> onInstantiate, float offset)
		{
			foreach (var obj in objects.AllChilds())
			{
				if (!predicate(obj))
					continue;
				for (int i = 1; i <= ceilingHeight; i++)
				{
					var clone = Instantiate(obj, objects);
					clone.name = obj.name;
					clone.position = obj.transform.position + (Vector3.up * (i * offset));
					clone.rotation = obj.transform.rotation;
					onInstantiate?.Invoke(clone);

					var collider = clone.GetComponent<Collider>();
					if (collider != null)
						Destroy(collider);

					var nav = clone.GetComponent<NavMeshObstacle>();
					if (nav != null)
						Destroy(nav);
				}
			}
		}

		LevelBuilder proof;
		Texture2D originalCeilTex;
		MeshFilter tilePrefabRef;
		bool changed = false;

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
