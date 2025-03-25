using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_ChillyChilli : Item, IItemPrefab
	{
		[SerializeField]
		internal MovementModifier moveMod = new(Vector3.zero, 0f);

		[SerializeField]
		internal float lifeTime = 15f, coldEffectCooldown = 10f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audEat, audBreathing;

		[SerializeField]
		internal ParticleSystem breezeParticles;

		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.ignoreRaycast;

			var collider = gameObject.AddComponent<BoxCollider>();
			collider.size = new Vector3(1.75f, 2f, 14f);
			collider.center = new Vector3(0f, 0f, 14f);
			collider.isTrigger = true;

			audMan = gameObject.CreateAudioManager(55f, 75f).MakeAudioManagerNonPositional();
			audEat = GenericExtensions.FindResourceObjectByName<SoundObject>("ChipCrunch");
			audBreathing = this.GetSoundNoSub("breathing.wav", SoundType.Effect);

			breezeParticles = new GameObject("BreezeParticles").AddComponent<ParticleSystem>();
			breezeParticles.transform.SetParent(transform);
			breezeParticles.transform.localPosition = Vector3.forward * 0.05f;
			breezeParticles.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = this.GetTexture("breath.png") };

			var main = breezeParticles.main;
			main.gravityModifierMultiplier = 0.05f;
			main.startLifetimeMultiplier = 1.15f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(0.5f, 1f);

			var emission = breezeParticles.emission;
			emission.rateOverTimeMultiplier = 90f;
			emission.enabled = false;

			var vel = breezeParticles.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.Local;
			vel.x = new(-35f, 35f);
			vel.y = new(-4.5f, 4.5f);
			vel.z = new(25f, 65f);

			var col = breezeParticles.collision;
			col.enabled = true;
			col.type = ParticleSystemCollisionType.World;
			col.enableDynamicColliders = false;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			ec = pm.ec;
			audMan.QueueAudio(audEat);
			StartCoroutine(LifetimeCountdown());
			return true;
		}

		IEnumerator LifetimeCountdown()
		{
			while (audMan.QueuedAudioIsPlaying)
				yield return null;
			isBreathing = true;
			
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audBreathing);

			var emission = breezeParticles.emission;
			emission.enabled = true;

			float timer = lifeTime;
			while (timer > 0 || activeCoroutines > 0)
			{
				if (Time.timeScale == 0f)
				{
					yield return null;
					continue;
				}
				if (timer > 0f)
				{
					timer -= ec.EnvironmentTimeScale * Time.deltaTime;
					if (timer <= 0f) // Should be triggered once
					{
						isBreathing = false;
						audMan.FlushQueue(true);

						emission = breezeParticles.emission;
						emission.enabled = false;
					}
				}
				transform.position = pm.transform.position;
				transform.rotation = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.rotation;
				yield return null;
			}
			Destroy(gameObject);
		}

		void OnTriggerStay(Collider other)
		{
			if (!isBreathing || !other.isTrigger || !other.CompareTag("NPC")) return;

			var npc = other.GetComponent<NPC>();
			if (!npc || touchedNPCs.Contains(npc)) return;

			if (!Physics.Raycast(pm.transform.position, (npc.transform.position - pm.transform.position).normalized, out var hit, 999f, LayerStorage.principalLookerMask, QueryTriggerInteraction.Ignore))
				return;

			if (hit.transform == npc.transform)
			{
				touchedNPCs.Add(npc);
				StartCoroutine(FreezeNPC(npc));
			}
		}

		IEnumerator FreezeNPC(NPC npc)
		{
			// Make npcs blue lol
			activeCoroutines++;
			npc.Navigator.Am.moveMods.Add(moveMod);
			npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			Color ogColor = npc.spriteRenderer[0].color;
			npc.spriteRenderer[0].color = Color.blue;

			float cooldown = coldEffectCooldown;
			while (cooldown > 0f && npc)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			if (npc)
			{
				touchedNPCs.Remove(npc);
				npc.Navigator.Am.moveMods.Remove(moveMod);
				npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
				npc.spriteRenderer[0].color = ogColor;
			}

			activeCoroutines--;
		}
		readonly HashSet<NPC> touchedNPCs = [];
		int activeCoroutines;
		EnvironmentController ec;
		bool isBreathing = false;
	}
}