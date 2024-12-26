using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class KreyeHook : MonoBehaviour, IEntityTrigger
	{
		public void Initialize(EnvironmentController ec, MrKreye owner)
		{
			entity.Initialize(ec, transform.position);
			entity.SetHeight(defaultHeight);
			entity.SetActive(false);
			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				if (!thrown) return;
				dir = Vector3.Reflect(dir, hit.normal);

				positionArray[++hits] = transform.position;
				lineRenderer.SetPositions(positionArray);

				if (hits > hitsBeforeDespawning)
					CancelThrow();
			};

			this.ec = ec;
			this.owner = owner;

			positionArray = new Vector3[hitsBeforeDespawning + 2];

			lineRenderer.positionCount = positionArray.Length;

			initialized = true;
		}
		public void Throw(Entity target, float speed)
		{
			entity.SetActive(true);
			entity.Teleport(owner.transform.position);

			this.speed = speed;
			dir = dir = (target.transform.position - transform.position).normalized;
			thrown = true;
			disabled = false;
			targettedEntity = target;

			hits = 0;
		}

		void Despawn()
		{
			entity.SetActive(false);
			thrown = false;
			returning = false;
			disabled = true;
			entity.UpdateInternalMovement(Vector3.zero);
			targettedEntity?.ExternalActivity.moveMods.Remove(moveMod);
		}

		void CancelThrow()
		{
			Despawn();
			owner.WanderAgain();
		}

		void Return()
		{
			returning = true;
			thrown = false;
			targettedEntity.ExternalActivity.moveMods.Add(moveMod);
		}

		void Update()
		{
			if (!initialized)
				return;

			if (disabled)
			{
				entity.UpdateInternalMovement(Vector3.zero);
				return;
			}

			if (!targettedEntity)
			{
				CancelThrow();
				return;
			}

			if (returning)
			{
				dir = (positionArray[hits] - transform.position).normalized;
				if (Vector3.Distance(transform.position, positionArray[hits]) <= backWayDistanceCheck)
				{
					if (hits > 0)
						hits--;
				}

			}

			moveMod.movementAddend = dir * speed * ec.EnvironmentTimeScale;
			entity.UpdateInternalMovement(moveMod.movementAddend);
		}

		void LateUpdate()
		{
			if (!initialized) return;
			positionArray[0] = owner.transform.position;
			for (int i = hits + 1; i < positionArray.Length; i++)
				positionArray[i] = transform.position;


			lineRenderer.SetPositions(positionArray);
		}

		void OnDestroy() =>
			targettedEntity?.ExternalActivity.moveMods.Remove(moveMod);

		public void EntityTriggerEnter(Collider other) { }

		public void EntityTriggerStay(Collider other)
		{
			if (other.gameObject == owner.gameObject && returning)
			{
				Despawn();
				owner.SendToDetention(targettedEntity);
				return;
			}

			if (!thrown || other.gameObject == owner.gameObject)
				return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				audMan.PlaySingle(audGrab);
				targettedEntity = other.GetComponent<Entity>();
				Return();
			}
		}

		public void EntityTriggerExit(Collider other) 
		{
			if (other.transform == targettedEntity.transform)
				CancelThrow();
		}

		Vector3[] positionArray = [];
		Vector3 dir;
		float speed;
		int hits = 0;
		Entity targettedEntity = null;
		MrKreye owner;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal LineRenderer lineRenderer;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audGrab;

		[SerializeField]
		internal float defaultHeight = 5f, backWayDistanceCheck = 5f;

		[SerializeField]
		internal int hitsBeforeDespawning = 3;


		EnvironmentController ec;
		bool initialized = false, thrown = false, returning = false, disabled = false;
		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
