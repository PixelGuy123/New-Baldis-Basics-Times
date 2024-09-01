using BBTimes.Extensions;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class Vent : MonoBehaviour
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
				while (touchedEntity.Count != 0)
				{
					touchedEntity[0].ExternalActivity.moveMods.Remove(moveMod);
					touchedEntity.RemoveAt(0);
				}
				BlockAllDirections(false);
				animation = StartCoroutine(UnBlockAnimation());
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			var entity = other.GetComponent<Entity>();
			if (entity == null) return;

			entity.AddForce(new((entity.transform.position - transform.position).normalized, 19f, -19f));
			touchedEntity.Add(entity);
			entity.ExternalActivity.moveMods.Add(moveMod);
		}

		private void OnTriggerExit(Collider other)
		{
			var entity = other.GetComponent<Entity>();
			if (entity == null || !touchedEntity.Contains(entity)) return;
			touchedEntity.Remove(entity);
			entity.ExternalActivity.moveMods.Remove(moveMod);
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
		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		internal List<Vent> nextVents = [];

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

		const float minCooldown = 10f, maxCooldown = 25f, emissionRate = 75f;
	}
}
