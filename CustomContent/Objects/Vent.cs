using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class Vent : MonoBehaviour
	{
		private void Start()
		{
			if (!Enabled)
			{
				normalVentAudioMan.QueueAudio(ventAudios[0]);
				normalVentAudioMan.SetLoop(true);
			}
			normalVentAudioMan.maintainLoop = true;

			gasLeakVentAudioMan.maintainLoop = true;
		}

		public void BlockMe() => StartCoroutine(BlockAnimation());

		IEnumerator BlockAnimation()
		{
			yield return null;

			normalVentAudioMan.FlushQueue(true);
			normalVentAudioMan.SetLoop(false);
	
			float sprite = 0f;
			float speed = 0f;
			while (sprite < ventTexs.Length - 1)
			{
				sprite += speed;
				renderer.material.mainTexture = ventTexs[Mathf.FloorToInt(sprite)];
				speed += Time.deltaTime * ec.EnvironmentTimeScale * 0.2f;
				yield return null;
			}

			renderer.material.mainTexture = ventTexs[ventTexs.Length - 1];
			gasLeakVentAudioMan.QueueAudio(ventAudios[1]);
			gasLeakVentAudioMan.QueueAudio(ventAudios[2]);
			gasLeakVentAudioMan.SetLoop(true);

			var e = particle.emission;
			e.rateOverTimeMultiplier = emissionRate;

			BlockAllDirections(true);
			yield break;
		}

		IEnumerator UnBlockAnimation()
		{
			var e = particle.emission;
			e.rateOverTimeMultiplier = 0f;

			gasLeakVentAudioMan.FlushQueue(true);
			gasLeakVentAudioMan.SetLoop(false);
			gasLeakVentAudioMan.QueueAudio(ventAudios[3]);

			normalVentAudioMan.QueueAudio(ventAudios[0]);
			normalVentAudioMan.SetLoop(true);
			float sprite = ventTexs.Length - 1;
			float speed = 0f;
			while (sprite > 1f)
			{
				sprite -= speed;
				renderer.material.mainTexture = ventTexs[Mathf.FloorToInt(sprite)];
				speed += Time.deltaTime * ec.EnvironmentTimeScale * 0.2f;
				yield return null;
			}

			renderer.material.mainTexture = ventTexs[0];

			SwitchVent();
			yield break;
		}

		private void BlockAllDirections(bool block)
		{
			Enabled = block;
			gasLeakVentAudioMan.SetLoop(true);
			foreach (var dir in Directions.All()) 
			{
				var cell = ec.CellFromPosition(transform.position);
				cell.SilentBlock(dir, block);
				cell = ec.CellFromPosition(cell.position + dir.ToIntVector2());
				if (!cell.Null)
					cell.SilentBlock(dir.GetOpposite(), block);
			}
			colliders.Do(x => x.enabled = block);
			if (block)
				cooldown = UnityEngine.Random.Range(minCooldown, maxCooldown);
			
		}

		private void SwitchVent() => nextVents[UnityEngine.Random.Range(0, nextVents.Count)].BlockMe();

		private void Update()
		{
			if (!Enabled || nextVents.Count == 0) return;

			cooldown -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (cooldown <= 0f)
			{
				BlockAllDirections(false);
				StartCoroutine(UnBlockAnimation());
			}	
		}

		private void OnTriggerStay(Collider other)
		{
			var entity = other.GetComponent<Entity>();
			if (entity == null) return;

			entity.AddForce(new((entity.transform.position - transform.position).normalized, 5f, -5f));
		}

		bool Enabled = false;

		float cooldown;

		public List<Vent> nextVents = [];

		public EnvironmentController ec;

		[SerializeField]
		public BoxCollider[] colliders;

		[SerializeField]
		public Texture2D[] ventTexs = [];

		[SerializeField]
		public SoundObject[] ventAudios = [];

		[SerializeField]
		public MeshRenderer renderer;

		[SerializeField]
		public ParticleSystem particle;

		[SerializeField]
		public PropagatedAudioManager normalVentAudioMan;

		[SerializeField]
		public PropagatedAudioManager gasLeakVentAudioMan;

		const float minCooldown = 25f, maxCooldown = 40f, emissionRate = 75f;
	}
}
