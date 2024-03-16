using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	internal class HighCeilingRoomFunction : RoomFunction
	{
		public override void Build(LevelBuilder builder, System.Random rng)
		{
			if (height < 1)
				throw new System.InvalidOperationException("Height set is less than 2");

			if (rng.NextDouble() > chanceToHappen)
				return;

			room.ceilTex = ObjectCreationExtension.transparentTex;
			room.GenerateTextureAtlas();
			if (customLight != null)
				room.lightPre = customLight;

			base.Build(builder, rng);
			var planeHolder = new GameObject("PlaneCover");
			planeHolder.transform.SetParent(room.transform);
			planeHolder.transform.localPosition = Vector3.zero;

			var plane = Instantiate(BBTimesManager.man.Get<GameObject>("PlaneTemplate"), planeHolder.transform);
			var renderer = plane.GetComponent<MeshRenderer>();
			renderer.material.mainTexture = room.wallTex;
			plane.transform.localScale = Vector3.one * 10f;
			DestroyImmediate(plane.GetComponent<MeshCollider>());
			int offset = 0;

			for (int i = 1; i <= height; i++)
			{
				if (i > height - customWallProximityToCeil.Length)
				{
					renderer.material = new(renderer.material)
					{
						mainTexture = customWallProximityToCeil[offset++],
						name = renderer.material.name
					};
				}

				CreateWalls(room.position, Direction.South, 1, 0, room.size.x);
				CreateWalls(room.position, Direction.West, 0, 1, room.size.z);
				CreateWalls(room.position + room.size - new IntVector2(1, 1), Direction.North, -1, 0, room.size.x);
				CreateWalls(room.position + room.size - new IntVector2(1, 1), Direction.East, 0, -1, room.size.z);

				void CreateWalls(IntVector2 v, Direction dir, sbyte xAddend, sbyte zAddend, int max)
				{
					for (int x = 0; x < max; x++)
					{
						var c = builder.Ec.CellFromPosition(v);
						var wall = Instantiate(plane, planeHolder.transform);
						wall.transform.position = c.CenterWorldPosition + dir.ToVector3() * (BBTimesManager.TileBaseOffset / 2f - 0.01f) + Vector3.up * (i * BBTimesManager.TileBaseOffset);
						wall.transform.rotation = dir.ToRotation();
						v.x += xAddend;
						v.z += zAddend;
						c.AddRenderer(wall.GetComponent<MeshRenderer>());
					}
				}
			}



			if (!hasCeiling)
				goto end;

			renderer.material = new(renderer.material)
			{
				mainTexture = customCeiling ?? originalCeilTex
			};

			foreach (var c in room.cells)
			{
				var wall = Instantiate(plane, planeHolder.transform);
				wall.transform.position = c.CenterWorldPosition + Vector3.up * (height * BBTimesManager.TileBaseOffset + 5f);
				wall.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
				c.AddRenderer(wall.GetComponent<MeshRenderer>());
			}

		end:
			Destroy(plane);

		}

		public override void Initialize(RoomController room)
		{
			base.Initialize(room);
			originalCeilTex = room.ceilTex;
		}

		Texture2D originalCeilTex;

		[SerializeField]
		public int height = 1;

		[SerializeField]
		public bool hasCeiling = true;

		[SerializeField]
		public Transform bigObjects = null;

		[SerializeField]
		public Transform customLight;

		[SerializeField]
		public float chanceToHappen = 1f;

		[SerializeField]
		public Texture2D customCeiling = null;

		[SerializeField]
		public Texture2D[] customWallProximityToCeil = [];
	}
}
