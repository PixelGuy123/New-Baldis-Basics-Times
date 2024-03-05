using UnityEngine;

namespace BBTimes.Extensions.ObjectCreationExtensions
{
	public static partial class ObjectCreationExtension
	{
		public static GameObject CreateCube(Texture2D tex, bool useUVMap = true)
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

			/*
			 * Mesh script made thanks for Ilkinulas (http://ilkinulas.github.io/development/unity/2016/05/06/uv-mapping.html)
			 */
			if (useUVMap)
			{
				Mesh mesh = cube.GetComponent<MeshFilter>().mesh; // Setup Mesh
				mesh.Clear();
				mesh.vertices = vertices;
				mesh.triangles = triangles;
				mesh.uv = uvs;
				mesh.Optimize();
				mesh.RecalculateNormals();
			}

			var renderer = cube.GetComponent<MeshRenderer>(); // Put right material
			renderer.material = defaultMaterial;	
			renderer.material.mainTexture = tex;

			return cube;
		}

		public static bool TryCreateCube(Texture2D tex, out GameObject cube)
		{
			try
			{
				cube = CreateCube(tex);
				return true;
			}
			catch
			{
				cube = null;
			}
			return false;
		}

		static readonly Vector3[] vertices = [
				new (0, 1f, 0),
			new (0, 0, 0),
			new (1f, 1f, 0),
			new (1f, 0, 0),

			new (0, 0, 1f),
			new (1f, 0, 1f),
			new (0, 1f, 1f),
			new (1f, 1f, 1f),

			new (0, 1f, 0),
			new (1f, 1f, 0),

			new (0, 1f, 0),
			new(0, 1f, 1f),

			new(1f, 1f, 0),
			new(1f, 1f, 1f),
		];

		static readonly int[] triangles = [
				0, 2, 1, // front
			1, 2, 3,
			4, 5, 6, // back
			5, 7, 6,
			6, 7, 8, //top
			7, 9 ,8,
			1, 3, 4, //bottom
			3, 5, 4,
			1, 11,10,// left
			1, 4, 11,
			3, 12, 5,//right
			5, 12, 13


			];

		/*

		horizontal:
		0.2475 = 99
		0.4975 = 199
		0.5 = 200
		0.7475 = 299
		0.75 = 300
		0.25 = 100
		0.9975 = 399

		vertical:
		0.66220735 = 198
		0.6655518 = 199
		0.33110367 = 99
		0.996655518 = 298


		*/

		static readonly Vector2[] uvs = [
				new(0, 0.66f),
			new(0.25f, 0.66f),
			new(0, 0.33f),
			new(0.25f, 0.34f),

			new(0.5f, 0.66f),
			new(0.5f, 0.34f),
			new(0.75f, 0.65f),
			new(0.75f, 0.34f),

			new(1, 0.66f),
			new(1, 0.34f),

			new(0.25f, 1),
			new(0.5f, 1),

			new(0.25f, 0),
			new(0.5f, 0),
		];

	}
}
