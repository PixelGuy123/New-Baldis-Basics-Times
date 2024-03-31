using UnityEngine;
using BBTimes.CustomContent.Builders;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI.AssetTools;
using BBTimes.CustomContent.Objects;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class VentBuilderCustomData : CustomObjectPrefabData
	{
		public override void SetupPrefab()
		{
			base.SetupPrefab();

			// Making of the main vent
			var vent = new GameObject("VentPrefab", typeof(Vent));
			var box = vent.AddComponent<BoxCollider>();
			box.size = new Vector3(9.99f, 10f, 9.99f);
			box.enabled = false;
			box.isTrigger = true;

			var box3 = vent.AddComponent<BoxCollider>();
			box3.size = new Vector3(8.9f, 10f, 8.9f);
			box3.enabled = false;

			var blockObj = new GameObject("VentPrefab_RaycastBlock");
			blockObj.transform.SetParent(vent.transform);
			blockObj.transform.localPosition = Vector3.zero;
			blockObj.transform.localScale = new(1.2f, 10f, 1.2f);

			var box2 = blockObj.AddComponent<BoxCollider>();
			box2.size = new Vector3(10f, 10f, 10f);
			box2.enabled = false;
			
			blockObj.layer = LayerMask.NameToLayer("Block Raycast");

			Texture2D[] texs = [
				AssetLoader.TextureFromFile(System.IO.Path.Combine(TexturePath, "vent.png")),
				AssetLoader.TextureFromFile(System.IO.Path.Combine(TexturePath, "vent_1.png")),
				AssetLoader.TextureFromFile(System.IO.Path.Combine(TexturePath, "vent_2.png")),
				AssetLoader.TextureFromFile(System.IO.Path.Combine(TexturePath, "vent_3.png"))
				];
			SoundObject[] audios = [
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "vent_normal.wav")), "", SoundType.Voice, Color.white),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "vent_gasleak_start.wav")), "Vfx_VentGasLeak", SoundType.Voice, Color.white),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "vent_gasleak_loop.wav")), "Vfx_VentGasLeak", SoundType.Voice, Color.white),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(System.IO.Path.Combine(SoundPath, "vent_gasleak_end.wav")), "Vfx_VentGasLeak", SoundType.Voice, Color.white)
				];

			audios[0].subtitle = false;

			var visual = ObjectCreationExtension.CreateCube(texs[0]);
			Destroy(visual.GetComponent<BoxCollider>()); // Removes the collider

			var v = vent.GetComponent<Vent>();
			v.renderer = visual.GetComponent<MeshRenderer>();
			v.ventTexs = texs;
			v.normalVentAudioMan = vent.CreateAudioManager(2f, 25f, true); // Two propagated audio managers
			v.gasLeakVentAudioMan = vent.CreateAudioManager(2f, 25f, true);
			v.ventAudios = audios;
			v.colliders = [box, box2, box3];

			visual.transform.SetParent(vent.transform);
			visual.transform.localPosition = new Vector3(-4.95f, 9f, -4.95f);
			visual.transform.localScale = new Vector3(9.9f, 1f, 9.9f);

			vent.SetActive(false);
			DontDestroyOnLoad(vent);

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

			var connection = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(System.IO.Path.Combine(TexturePath, "ventConnection.png")), false);
			Destroy(connection.GetComponent<BoxCollider>());
			connection.name = "VentPrefab_Connection";
			connection.transform.localScale = new(connectionSize, 0.6f, connectionSize);
			connection.SetActive(false);
			DontDestroyOnLoad(connection);

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
				Destroy(con.GetComponent<BoxCollider>());
			}

			Destroy(connection2);

		}

		const float connectionSize = 2f;
	}
}
