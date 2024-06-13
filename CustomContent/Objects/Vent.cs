using BBTimes.Extensions;
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
			ec.BlockAllDirs(transform.position, block);
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
				animation = StartCoroutine(UnBlockAnimation());
			}	
		}

		private void OnTriggerStay(Collider other)
		{
			var entity = other.GetComponent<Entity>();
			if (entity == null) return;

			entity.AddForce(new((entity.transform.position - transform.position).normalized, 5f, -5f));
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

		const float minCooldown = 25f, maxCooldown = 40f, emissionRate = 75f;
	}
}
