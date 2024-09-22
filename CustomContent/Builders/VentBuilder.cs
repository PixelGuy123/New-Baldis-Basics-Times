using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using BBTimes.ModPatches.EnvironmentPatches;
using System.Collections.Generic;
using UnityEngine;
using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using PixelInternalAPI.Components;
using PixelInternalAPI.Classes;


namespace BBTimes.CustomContent.Builders
{
    public class VentBuilder : ObjectBuilder, IObjectPrefab
	{
		public void SetupPrefab()
		{
			// Making of the main vent
			var vent = new GameObject("VentPrefab", typeof(Vent)) { layer = LayerStorage.ignoreRaycast};
			vent.AddBoxCollider(Vector3.zero, new(9.99f, 10f, 9.99f), true);

			var blockObj = new GameObject("VentPrefab_RaycastBlock");
			blockObj.transform.SetParent(vent.transform);
			blockObj.transform.localPosition = Vector3.zero;
			blockObj.transform.localScale = new(1.2f, 10f, 1.2f);

			var box2 = blockObj.AddBoxCollider(Vector3.zero, Vector3.one * 10f, true);
			box2.enabled = false;

			blockObj.layer = LayerMask.NameToLayer("Block Raycast");

			Texture2D[] texs = [
				this.GetTexture("vent.png"),
				this.GetTexture("vent_1.png"),
				this.GetTexture("vent_2.png"),
				this.GetTexture("vent_3.png")
				];

			var visual = ObjectCreationExtension.CreateCube(texs[0]);
			Destroy(visual.GetComponent<BoxCollider>()); // Removes the collider

			vent.ConvertToPrefab(true);

			var v = vent.GetComponent<Vent>();
			v.renderer = visual.GetComponent<MeshRenderer>();
			v.ventTexs = texs;
			v.normalVentAudioMan = vent.CreatePropagatedAudioManager(2f, 25f); // Two propagated audio managers
			v.gasLeakVentAudioMan = vent.CreatePropagatedAudioManager(2f, 25f);
			v.ventAudios = [this.GetSoundNoSub("vent_normal.wav", SoundType.Voice), 
				this.GetSound("vent_gasleak_start.wav", "Vfx_VentGasLeak", SoundType.Voice, Color.white),
				this.GetSound("vent_gasleak_loop.wav", "Vfx_VentGasLeak", SoundType.Voice, Color.white),
				this.GetSound("vent_gasleak_end.wav", "Vfx_VentGasLeak", SoundType.Voice, Color.white)];
			v.colliders = [box2];

			visual.transform.SetParent(vent.transform);
			visual.transform.localPosition = new Vector3(-4.95f, 9f, -4.95f);
			visual.transform.localScale = new Vector3(9.9f, 1f, 9.9f);

			var builder = GetComponent<VentBuilder>();
			builder.ventPrefab = vent;

			// Making of particles

			var particle = new GameObject("VentPrefab_ParticleEmitter", typeof(BillboardRotator)).AddComponent<ParticleSystem>();
			particle.transform.SetParent(vent.transform);
			particle.transform.localPosition = Vector3.up * 10f;
			particle.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);


			var m = particle.main;
			m.gravityModifier = -4f;
			m.cullingMode = ParticleSystemCullingMode.Automatic;
			m.startLifetimeMultiplier = 2.1f;
			m.startSize = 3f;


			var e = particle.emission;
			e.enabled = true;
			e.rateOverTime = 0f;

			var vp = particle.velocityOverLifetime;
			vp.enabled = true;
			vp.zMultiplier = -4f;
			vp.yMultiplier = -24f;
			vp.radialMultiplier = 1.5f;

			var vs = particle.rotationBySpeed;
			vs.enabled = true;
			vs.z = 1.5f;
			vs.range = new(0f, 5f);

			v.particle = particle;

			particle.GetComponent<ParticleSystemRenderer>().material = ObjectCreationExtension.defaultDustMaterial;

			// Making of vent connections

			var connection = ObjectCreationExtension.CreateCube(this.GetTexture("ventConnection.png"), false);
			Destroy(connection.GetComponent<BoxCollider>());
			connection.name = "VentPrefab_Connection";
			connection.transform.localScale = new(connectionSize, 0.6f, connectionSize);


			builder.ventConnectionPrefab = connection;



			var connection2 = Instantiate(connection);

			for (int i = 0; i < 4; i++)
			{
				var dir = Directions.All()[i];
				var con = Instantiate(connection2, connection.transform);
				con.transform.localPosition = dir.ToVector3() * 1.5f;
				con.transform.rotation = dir.ToRotation();
				con.transform.localScale = new Vector3(1f, 1f, 2.01f);
				con.name = "VentPrefab_Connection_" + dir;
				con.SetActive(false);
				Destroy(con.GetComponent<BoxCollider>());
			}

			Destroy(connection2);
			connection.ConvertToPrefab(true);
		}

		public void SetupPrefabPost() { }

		const float connectionSize = 2f;

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");



		// ^^
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			List<Cell> halls = room.GetTilesOfShape([TileShape.Corner, TileShape.Single], false);
			if (halls.Count == 0) return;

			int ventAmount = cRng.Next(minAmount, maxAmount + 1);

			var selectedWebTile = halls[cRng.Next(halls.Count)];
			var web = ec.FindNearbyTiles(selectedWebTile.position - new IntVector2(builder.levelSize.x / 5, builder.levelSize.z / 5),
				selectedWebTile.position + new IntVector2(builder.levelSize.x / 5, builder.levelSize.z / 5),
				(builder.levelSize.x + builder.levelSize.z) / 6);

			List<Vent> vents = [];

			foreach (var cell in web)
			{
				if (cell.TileMatches(room) && !cell.HasAnyHardCoverage && !cell.open && !cell.doorHere && (cell.shape == TileShape.Corner || cell.shape == TileShape.Single) && !ec.TrapCheck(cell))
				{
					var vent = Instantiate(ventPrefab, room.transform);
					vent.transform.position = cell.FloorWorldPosition;
					vent.SetActive(true);
					var v = vent.GetComponent<Vent>();
					v.ec = ec;
					cell.HardCoverEntirely();
					cell.AddRenderer(v.renderer);
					cell.AddRenderer(v.particle.GetComponent<ParticleSystemRenderer>());
					vents.Add(v);
				}
				if (vents.Count >= ventAmount)
					break;
			}

			if (vents.Count == 0) return;

			Dictionary<IntVector2, GameObject> connectionpos = [];

			foreach (var vent in vents)
			{
				Cell center = ec.CellFromPosition(vent.transform.position);
				for (int i = 0; i < vents.Count; i++)
				{
					if (vents[i] == vent) continue; // Not make a path to itself of course
					EnvironmentControllerPatch.SetNewData([TileShape.Closed], [RoomType.Hall], true); // Limit to only hallways
					ec.FindPath(center, ec.CellFromPosition(vents[i].transform.position), PathType.Const, out var path, out bool success);
					EnvironmentControllerPatch.ResetData();
					if (!success) continue;
					foreach (var t in path)
					{
						if (connectionpos.ContainsKey(t.position)) continue;
						var c = Instantiate(ventConnectionPrefab);
						t.HardCover(CellCoverage.Up);
						c.transform.SetParent(t.TileTransform);

						t.AddRenderer(c.GetComponent<MeshRenderer>());

						c.transform.localPosition = Vector3.up * 9.5f;
						c.SetActive(true);
						connectionpos.Add(t.position, c);

						List<Cell> neighbors = [];
						ec.GetNavNeighbors(t, neighbors, PathType.Const);
						foreach (var n in neighbors)
						{
							if (connectionpos.TryGetValue(n.position, out var c2))
							{
								var dir = Directions.DirFromVector3(c2.transform.position - c.transform.position, 45f); // 90° angle
								var child = c.transform.Find("VentPrefab_Connection_" + dir);
								child?.gameObject.SetActive(true);

								child = c2.transform.Find("VentPrefab_Connection_" + dir.GetOpposite());
								child?.gameObject.SetActive(true);
							}
						}

						foreach (var c2 in c.transform.AllChilds())
							t.AddRenderer(c2.GetComponent<MeshRenderer>());
					}
				}
				var v = vent.GetComponent<Vent>();
				v.nextVents = new(vents);
				v.nextVents.Remove(vent); // nextVents, excluding itself
			}

			vents[0].BlockMe();

			ec.GetComponent<EnvironmentControllerData>().Vents.AddRange(vents);


		}

		public override void Load(EnvironmentController ec, List<IntVector2> pos, List<Direction> dir) // In case I modify premade assets (like Endless medium)
		{
			base.Load(ec, pos, dir);
			List<Vent> vents = [];

			foreach (var p in pos)
			{
				var cell = ec.CellFromPosition(p);
				var vent = Instantiate(ventPrefab, cell.room.transform);
				vent.transform.position = cell.FloorWorldPosition;
				vent.SetActive(true);
				var v = vent.GetComponent<Vent>();
				v.ec = ec;
				cell.HardCoverEntirely();
				vents.Add(v);
			}

			if (vents.Count == 0) return;

			Dictionary<IntVector2, GameObject> connectionpos = [];

			foreach (var vent in vents)
			{
				Cell center = ec.CellFromPosition(vent.transform.position);
				for (int i = 0; i < vents.Count; i++)
				{
					if (vents[i] == vent) continue; // Not make a path to itself of course
					EnvironmentControllerPatch.SetNewData([TileShape.Closed], [RoomType.Hall], true); // Limit to only hallways
					ec.FindPath(center, ec.CellFromPosition(vents[i].transform.position), PathType.Const, out var path, out bool success);
					EnvironmentControllerPatch.ResetData();
					if (!success) continue;
					foreach (var t in path)
					{
						if (connectionpos.ContainsKey(t.position)) continue;
						var c = Instantiate(ventConnectionPrefab);
						t.HardCover(CellCoverage.Up);
						c.transform.SetParent(t.TileTransform);

						t.AddRenderer(c.GetComponent<MeshRenderer>());

						c.transform.localPosition = Vector3.up * 9.5f;
						c.SetActive(true);
						connectionpos.Add(t.position, c);

						List<Cell> neighbors = [];
						ec.GetNavNeighbors(t, neighbors, PathType.Const);
						foreach (var n in neighbors)
						{
							if (connectionpos.TryGetValue(n.position, out var c2))
							{
								var d = Directions.DirFromVector3(c2.transform.position - c.transform.position, 45f); // 90° angle
								var child = c.transform.Find("VentPrefab_Connection_" + d);
								child?.gameObject.SetActive(true);

								child = c2.transform.Find("VentPrefab_Connection_" + d.GetOpposite());
								child?.gameObject.SetActive(true);
							}
						}

						foreach (var c2 in c.transform.AllChilds())
							t.AddRenderer(c2.GetComponent<MeshRenderer>());
					}
				}
				var v = vent.GetComponent<Vent>();
				v.nextVents = new(vents);
				v.nextVents.Remove(vent); // nextVents, excluding itself
			}

			vents[0].BlockMe();

			ec.GetComponent<EnvironmentControllerData>().Vents.AddRange(vents);
		}

		[SerializeField]
		public GameObject ventPrefab;

		[SerializeField]
		public GameObject ventConnectionPrefab;

		[SerializeField]
		public int minAmount = 6, maxAmount = 10;
	}
}
