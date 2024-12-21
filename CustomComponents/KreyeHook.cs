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
			entity.OnEntityMoveInitialCollision += (hit) => CancelThrow();

			this.ec = ec;
			this.owner = owner;
			positions = [owner.transform.position, transform.position];

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
		}

		void Despawn()
		{
			thrown = false;
			returning = false;
			disabled = true;
			entity.UpdateInternalMovement(Vector3.zero);
			targettedEntity?.ExternalActivity.moveMods.Remove(moveMod);
		}

		void CancelThrow()
		{
			entity.SetActive(false);
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
				dir = (owner.transform.position - transform.position).normalized;

			moveMod.movementAddend = dir * speed * ec.EnvironmentTimeScale;
			entity.UpdateInternalMovement(moveMod.movementAddend);
		}

		void LateUpdate()
		{
			if (!initialized) return;
			positions[0] = owner.transform.position;
			positions[1] = transform.position;

			lineRenderer.SetPositions(positions);
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

		Vector3[] positions;
		Vector3 dir;
		float speed;
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
		internal float defaultHeight = 5f;


		EnvironmentController ec;
		bool initialized = false, thrown = false, returning = false, disabled = false;
		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
