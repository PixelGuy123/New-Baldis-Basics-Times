using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class Bubble : MonoBehaviour
	{
		[SerializeField]
		internal SoundObject audPop;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Canvas bubbleCanvas;

		Vector3 direction;
		Entity target, owner;
		EnvironmentController ec;
		bool hasPopped = false, holding = false, initialized = false;
		float height = 5f, speed = 5f;
		public bool Initialized => initialized;

		public void Spawn(EnvironmentController ec, Entity owner, Vector3 position, Vector3 direction, float speed)
		{
			this.owner = owner;
			this.ec = ec;
			entity.Initialize(ec, position);
			entity.UpdateInternalMovement(Vector3.zero);
			entity.OnEntityMoveInitialCollision += hit => Pop();

			this.direction = direction;
			this.speed = speed;
			
		}

		public void Pop()
		{
			if (hasPopped) return;

			hasPopped = true;
			entity.UpdateInternalMovement(Vector3.zero);
			renderer.enabled = false;
			holding = false;

			SetTarget(true);

			overrider.Release();
			if (target)
			{
				var pm = target.GetComponent<PlayerManager>();
				if (pm)
					affectedPlayers.RemoveAll(x => x.Key == pm);
			}

			StartCoroutine(PopWait());
			target = null;
		}
		
		void OnDestroy() 
		{
			if (target)
			{
				var pm = target.GetComponent<PlayerManager>();
				if (pm)
					affectedPlayers.RemoveAll(x => x.Key == pm);
			}
		}

		public void Initialize() =>
			initialized = true;

		public void OnTriggerEnter(Collider other)
		{
			if (!initialized || holding || hasPopped || other.gameObject == owner.gameObject || other.GetComponent<Bubble>()) return;
			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e && e.Override(overrider))
				{
					target = e;
					height = e.InternalHeight;
					target.Teleport(transform.position);
					overrider.SetHeight(height + 0.6f);
					holding = true;
					SetTarget(false);
				}
				PlayerManager pm = other.GetComponent<PlayerManager>();
				if (other.CompareTag("Player") && pm)
				{
					affectedPlayers.Add(new(pm, this));
					bubbleCanvas.gameObject.SetActive(true);
					bubbleCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
				}
			}
		}

		void Update()
		{
			if (hasPopped || !initialized) return;

			entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
			renderer.transform.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(8.5f * Time.fixedTime));

			if (holding)
			{
				if (!target || (transform.position - target.transform.position).magnitude > 10f)
				{
					Pop();
					return;
				}
				target.Teleport(transform.position);
			}
		}

		void SetTarget(bool active)
		{
			if (!target)
				return;

			overrider.SetFrozen(!active);
			overrider.SetInteractionState(active);
			if (active)
				overrider.SetHeight(height);
			
		}

		IEnumerator PopWait()
		{
			bubbleCanvas.gameObject.SetActive(false);
			audMan.QueueAudio(audPop);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			Destroy(gameObject);

			yield break;
		}

		readonly EntityOverrider overrider = new();

		public static List<KeyValuePair<PlayerManager, Bubble>> affectedPlayers = [];
	}
}
