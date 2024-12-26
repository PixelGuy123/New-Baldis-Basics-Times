using MTM101BaldAPI.Registers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public abstract class Shuffler<T> : MonoBehaviour where T : Component
	{
		public void Initialize(T target, EnvironmentController ec, float delay, params T[] shuffleEntities)
		{
			if (initialized) return;
			initialized = true;
			this.target = target;
			this.ec = ec;
			shuffleTars.AddRange(shuffleEntities);
			StartCoroutine(Teleport(delay));
		}

		IEnumerator Teleport(float del)
		{
			audMan.QueueAudio(audPrep);
			audMan.SetLoop(true);
			while (del > 0f)
			{
				del -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			for (int i = 0; i < shuffleTars.Count; i++)
			{
				if (!shuffleTars[i] || shuffleTars[i] == target || ShuffleCheckCondition(shuffleTars[i]))
					shuffleTars.RemoveAt(i--);
			}
			audMan.FlushQueue(true);
			audMan.PlaySingle(audTel);
			if (shuffleTars.Count != 0)
			{
				next = shuffleTars[Random.Range(0, shuffleTars.Count)];
				ShuffleCall(next);
			}
			parts.Stop(true, ParticleSystemStopBehavior.StopEmitting);

			PostTeleportProcedure();
			while (audMan.AnyAudioIsPlaying) yield return null;

			
			yield break;
		}

		public abstract void ShuffleCall(T next);

		protected abstract bool ShuffleCheckCondition(T obj);

		protected virtual void PostTeleportProcedure() => Destroy(gameObject);



		void Update()
		{
			if (!initialized) return;
			if (!target)
				Destroy(gameObject);
			else
				transform.position = target.transform.position;
		}

		[SerializeField]
		internal ParticleSystem parts;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audPrep, audTel;

		readonly List<T> shuffleTars = [];
		bool initialized = false;
		protected EnvironmentController ec;
		protected T target;
		protected T next;
	}

	public class EntityShuffler : Shuffler<Entity>
	{
		public override void ShuffleCall(Entity next) =>
			StartCoroutine(TeleportDelay(next));
		IEnumerator TeleportDelay(Entity next)
		{
			target.SetInteractionState(false);
			next.SetInteractionState(false);

			yield return null;
			Vector3 pos = ((MonoBehaviour)next).transform.position;
			next.Teleport(((MonoBehaviour)target).transform.position);
			target.Teleport(pos);

			yield return null;

			StartCoroutine(ImmunityTimer());
			target.SetInteractionState(true);
			next.SetInteractionState(true);

			yield break;
		}

		protected override void PostTeleportProcedure() { }

		IEnumerator ImmunityTimer()
		{
			target.ExternalActivity.moveMods.Add(slowMod);
			next.ExternalActivity.moveMods.Add(slowMod);
			PushEveryoneAround(next);
			PushEveryoneAround(target);

			ec.Npcs.ForEach(x => x.Navigator.Entity?.IgnoreEntity(target, true));

			float timer = 5f;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			target.ExternalActivity.moveMods.Remove(slowMod);
			next.ExternalActivity.moveMods.Remove(slowMod);

			ec.Npcs.ForEach(x => x.Navigator.Entity?.IgnoreEntity(target, false));

			base.PostTeleportProcedure();
		}

		void PushEveryoneAround(Entity ent)
		{
			foreach (var npc in ec.Npcs)
			{
				if (npc.Navigator.enabled && npc.Navigator.Entity != ent && npc.GetMeta().flags.HasFlag(NPCFlags.Standard))
				{
					float force = 45f - (Vector3.Distance(npc.transform.position, ent.transform.position) * 0.25f);
					if (force > 2f)
						npc.Navigator.Entity.AddForce(new((npc.transform.position - ent.transform.position).normalized, force, -force * 0.75f));
				}
			}
		}

		protected override bool ShuffleCheckCondition(Entity obj) => ec.CellFromPosition(obj.transform.position).Null;

		readonly MovementModifier slowMod = new(Vector3.zero, 0.85f);
	}

	public class PickupShuffler : Shuffler<Pickup>
	{
		public override void ShuffleCall(Pickup next)
		{
			Vector3 pos = next.transform.position;
			next.transform.position = target.transform.position;
			next.icon?.UpdatePosition(ec.map);
			target.transform.position = pos;
			target.icon?.UpdatePosition(ec.map);
		}

		protected override bool ShuffleCheckCondition(Pickup obj) =>
			obj.item.itemType == Items.None || !obj.free;
	}
}
