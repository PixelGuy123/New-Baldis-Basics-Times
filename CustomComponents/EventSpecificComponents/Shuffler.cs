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
				ShuffleCall(shuffleTars[Random.Range(0, shuffleTars.Count)]);
			parts.Stop(true, ParticleSystemStopBehavior.StopEmitting);

			while (audMan.AnyAudioIsPlaying) yield return null;

			Destroy(gameObject);

			yield break;
		}

		public abstract void ShuffleCall(T next);

		protected abstract bool ShuffleCheckCondition(T obj);
			
		

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
	}

	public class EntityShuffler : Shuffler<Entity>
	{
		public override void ShuffleCall(Entity next) =>
			StartCoroutine(TeleportDelay(next));
		IEnumerator TeleportDelay(Entity next)
		{
			target.IgnoreEntity(next, true);
			yield return null;
			Vector3 pos = ((MonoBehaviour)next).transform.position;
			next.Teleport(((MonoBehaviour)target).transform.position);
			target.Teleport(pos);

			yield return null;

			target.IgnoreEntity(next, false);

			yield break;
		}

		protected override bool ShuffleCheckCondition(Entity obj) => false;
	}

	public class PickupShuffler : Shuffler<Pickup>
	{
		public override void ShuffleCall(Pickup next)
		{
			Vector3 pos = next.transform.position;
			next.transform.position = target.transform.position;
			next.icon.UpdatePosition(ec.map);
			target.transform.position = pos;
			target.icon.UpdatePosition(ec.map);
		}

		protected override bool ShuffleCheckCondition(Pickup obj) =>
			obj.item.itemType == Items.None || !obj.free;
	}
}
