using BBTimes.Extensions;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class Duct : MonoBehaviour
	{
		private void Start()
		{
			normalVentAudioMan.maintainLoop = true;
			gasLeakVentAudioMan.maintainLoop = true;
			if (!Enabled)
			{
				normalVentAudioMan.SetLoop(true);
				normalVentAudioMan.QueueAudio(ventAudios[0]);
			}
		}

		public void BlockMe() => animation = StartCoroutine(BlockAnimation());

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
			gasLeakVentAudioMan.SetLoop(true);
			gasLeakVentAudioMan.QueueAudio(ventAudios[1]);
			gasLeakVentAudioMan.QueueAudio(ventAudios[2]);

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

			normalVentAudioMan.SetLoop(true);
			normalVentAudioMan.QueueAudio(ventAudios[0]);
			
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
			var nVent = nextVents[Random.Range(0, nextVents.Count)];
			float ventBlockCooldown = Vector3.Distance(transform.position, nVent.transform.position) / 6f;

			while (ventBlockCooldown > 0f)
			{
				ventBlockCooldown -= Time.deltaTime * ec.EnvironmentTimeScale * 2f;
				yield return null;
			}

			nVent.BlockMe();
			

			yield break;
		}

		private void BlockAllDirections(bool block)
		{
			if (block)
				cooldown = Random.Range(minCooldown, maxCooldown);
			Enabled = block;
			gasLeakVentAudioMan.SetLoop(true);
			ec.BlockAllDirs(transform.position, block);
			colliders.Do(x => x.enabled = block);

		}

		private void Update()
		{
			if (!Enabled || nextVents.Count == 0) return;

			cooldown -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (cooldown <= 0f)
			{
				BlockAllDirections(false);
				animation = StartCoroutine(UnBlockAnimation());
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			var entity = other.GetComponent<Entity>();
			if (entity == null || touchedEntity.Contains(entity)) return;

			if (Enabled)
				entity.AddForce(new((entity.transform.position - transform.position).normalized, leakPushForce, leakAccelerationForce));
			touchedEntity.Add(entity);
		}

		private void OnTriggerExit(Collider other)
		{
			var entity = other.GetComponent<Entity>();
			if (entity == null) return;
			touchedEntity.Remove(entity);
		}

		public void DisableVent(bool disable)
		{
			if (disable)
			{
				normalVentAudioMan.FlushQueue(true);
				gasLeakVentAudioMan.FlushQueue(true);
				if (Enabled)
					BlockAllDirections(false);
				if (animation != null)
					StopCoroutine(animation);
				renderer.material.mainTexture = ventTexs[0];
				var e = particle.emission;
				e.rateOverTimeMultiplier = 0f;
			}
			else
			{
				normalVentAudioMan.QueueAudio(ventAudios[0]);
				normalVentAudioMan.SetLoop(true);
				normalVentAudioMan.maintainLoop = true;
			}
		}

		bool Enabled = false;

		float cooldown;

		Coroutine animation;

		readonly List<Entity> touchedEntity = [];

		internal List<Duct> nextVents = [];

		internal EnvironmentController ec;

		[SerializeField]
		internal BoxCollider[] colliders;

		[SerializeField]
		internal Texture2D[] ventTexs = [];

		[SerializeField]
		internal SoundObject[] ventAudios = [];

		[SerializeField]
		internal MeshRenderer renderer;

		[SerializeField]
		internal ParticleSystem particle;

		[SerializeField]
		internal PropagatedAudioManager normalVentAudioMan, gasLeakVentAudioMan;

		[SerializeField]
		internal float leakPushForce = 31f, leakAccelerationForce = -12.5f;

		const float minCooldown = 10f, maxCooldown = 25f, emissionRate = 75f;
	}
}
