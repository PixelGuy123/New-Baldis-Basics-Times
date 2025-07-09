using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomContent.Events;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public abstract class Plant : EnvironmentObject, IItemAcceptor
	{

		public void Initialize(NatureEvent ev, EnvironmentController ec, Cell cell, Vector3 spawnPos)
		{
			if (despawned || initialized) return;

			natEv = ev;
			this.ec = ec;
			spawn = cell;
			StartCoroutine(SpawnAnimation(spawnPos));
		}

		public void Despawn(bool replaceWithOther, bool actualDestroy = true)
		{
			if (!initialized)
				return;

			natEv.RemoveFlower(this);
			if (actualDestroy)
			{
				OnDespawn();
			}

			StartCoroutine(DespawnAnimation(actualDestroy));
			despawned = true;

			if (replaceWithOther)
				natEv.SpawnRandomFlower(spawn);
		}

		public virtual void PrefabSetup(NatureEvent setup) { }

		protected virtual void OnDespawn() { } // For plants to clean up stuff when despawning from here (IT SHOULDN'T DO ANYTHING TO INTERRUPT THE ANIMATION, but to clean up any effect left over)

		void OnTriggerEnter(Collider other)
		{
			if (!initialized) return;

			if (other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm)
					TriggerEnterPlayer(pm);
				return;
			}
			var npc = other.GetComponent<NPC>();
			if (npc)
				TriggerEnterNPC(npc);
		}
		void OnTriggerExit(Collider other)
		{
			if (!initialized) return;

			if (other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm)
					TriggerExitPlayer(pm);
				return;
			}
			var npc = other.GetComponent<NPC>();
			if (npc)
				TriggerExitNPC(npc);
		}

		protected virtual void TriggerEnterPlayer(PlayerManager pm) { }
		protected virtual void TriggerExitPlayer(PlayerManager pm) { }
		protected virtual void TriggerEnterNPC(NPC npc) { }
		protected virtual void TriggerExitNPC(NPC npc) { }

		IEnumerator SpawnAnimation(Vector3 expectedPos)
		{
			Vector3 curPos = renderer.transform.position;
			float t = 0f;
			while (true)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * 3.5f;
				if (t >= 1f)
				{
					renderer.transform.position = expectedPos;
					initialized = true;
					yield break;
				}
				renderer.transform.position = Vector3.Lerp(curPos, expectedPos, t);

				yield return null;
			}
		}

		IEnumerator DespawnAnimation(bool actuallyDestroy)
		{
			if (despawned)
			{
				if (actuallyDestroy)
					Destroy(gameObject);
				yield break;
			}

			Vector3 curPos = renderer.transform.position;
			Vector3 expectedPos = curPos + Vector3.down * 10f;

			float t = 0f;
			while (true)
			{
				t += ec.EnvironmentTimeScale * Time.deltaTime * 3.5f;
				if (t >= 1f)
				{
					renderer.transform.position = expectedPos;
					if (!audMan.AnyAudioIsPlaying)
					{
						if (actuallyDestroy)
							Destroy(gameObject);
						yield break;
					}
					yield return null;
					continue;
				}
				renderer.transform.position = Vector3.Lerp(curPos, expectedPos, t);

				yield return null;
			}
		}

		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{

		}

		public bool ItemFits(Items item) =>
			!despawned && initialized && cuttableItems.Contains(item);

		NatureEvent natEv;
		public NatureEvent NatureEv => natEv;

		Cell spawn;
		public Cell Spawn => spawn;

		[SerializeField]
		internal SpriteRenderer renderer;

		bool initialized = false, despawned = false;

		public bool Initialized => initialized;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audSpawn, audDespawn;

		[SerializeField]
		internal BoxCollider collider;

		public static HashSet<Items> cuttableItems = [Items.Scissors];
	}
}
