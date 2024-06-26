﻿using BBTimes.CustomContent.Builders;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class VentBuilderCustomData : CustomObjectPrefabData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("vent_normal.wav", SoundType.Voice),
			GetSound("vent_gasleak_start.wav", "Vfx_VentGasLeak", SoundType.Voice, Color.white),
			GetSound("vent_gasleak_loop.wav", "Vfx_VentGasLeak", SoundType.Voice, Color.white),
			GetSound("vent_gasleak_end.wav", "Vfx_VentGasLeak", SoundType.Voice, Color.white)];

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
				GetTexture("vent.png"),
				GetTexture("vent_1.png"),
				GetTexture("vent_2.png"),
				GetTexture("vent_3.png")
				];

			var visual = ObjectCreationExtension.CreateCube(texs[0]);
			Destroy(visual.GetComponent<BoxCollider>()); // Removes the collider

			vent.ConvertToPrefab(true);

			var v = vent.GetComponent<Vent>();
			v.renderer = visual.GetComponent<MeshRenderer>();
			v.ventTexs = texs;
			v.normalVentAudioMan = vent.CreatePropagatedAudioManager(2f, 25f); // Two propagated audio managers
			v.gasLeakVentAudioMan = vent.CreatePropagatedAudioManager(2f, 25f);
			v.ventAudios = [.. soundObjects];
			v.colliders = [box, box2, box3];

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

			var connection = ObjectCreationExtension.CreateCube(GetTexture("ventConnection.png"), false);
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

		const float connectionSize = 2f;
	}
}
