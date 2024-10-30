using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ScienceTeacher
{
	public abstract class Potion : EnvironmentObject, IEntityTrigger
	{
		public void Initialize(EnvironmentController ec, Vector3 position, Vector3 throwingDirection, float throwSpeed, float throwUpSpeed = 0f)
		{
			if (initialized)
				return;

			initialized = true;
			this.ec = ec;
			entity.Initialize(ec, position);
			dir = throwingDirection * throwSpeed;
			verticalSpeed = throwUpSpeed;
		}

		void TurnIntoPuddle()
		{
			if (isAPuddle) return;

			entity.SetFrozen(true);
			isAPuddle = true;
			StartCoroutine(PuddleAnimation());
		}

		public void Despawn()
		{
			if (despawned) return;

			despawned = true;
			StartCoroutine(DespawnAnimation());
		}

		protected abstract void CrashOnEntity(Entity entity);
		protected abstract void OnEntityEnter(Entity entity);
		protected abstract void OnEntityStay(Entity entity);
		protected abstract void OnEntityExit(Entity entity);

		public void EntityTriggerEnter(Collider other)
		{
			if (isAPuddle && IsEntity(other))
			{
				var e = other.GetComponent<Entity>();

				if (e && e.Grounded)
					OnEntityEnter(e);
			}
		}

		public void EntityTriggerExit(Collider other)
		{
			if (isAPuddle && IsEntity(other))
			{
				var e = other.GetComponent<Entity>();
				if (e && e.Grounded)
					OnEntityExit(e);
			}
		}

		public void EntityTriggerStay(Collider other)
		{
			if (IsEntity(other))
			{
				var e = other.GetComponent<Entity>();
				if (!e) return;

				if (!isAPuddle)
				{
					CrashOnEntity(e);
					TurnIntoPuddle();
					return;
				}
				if (e.Grounded)
					OnEntityStay(e);
			}
		}

		bool IsEntity(Collider other) => other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player"));

		IEnumerator PuddleAnimation()
		{
			renderer.sprite = sprPuddleVariant;
			renderer.transform.localScale = Vector3.zero;
			float lerp = 0;
			while (true)
			{
				lerp += puddleSpeed * ec.EnvironmentTimeScale * Time.deltaTime;
				if (lerp >= 1f)
					break;
				renderer.transform.localScale = Vector3.one * lerp;
				yield return null;
			}

			renderer.transform.localScale = Vector3.one;
		}

		IEnumerator DespawnAnimation()
		{
			renderer.transform.localScale = Vector3.one;
			float lerp = 1f;
			while (true)
			{
				lerp += puddleSpeed * ec.EnvironmentTimeScale * Time.deltaTime;
				if (lerp <= 0f)
					break;
				renderer.transform.localScale = Vector3.one * lerp;
				yield return null;
			}

			renderer.transform.localScale = Vector3.zero;
			Destroy(gameObject);
		}

		void Update()
		{
			if (!IsAPuddle)
			{
				entity.UpdateInternalMovement(Vector3.zero);
				return;
			}
			entity.UpdateInternalMovement(dir);

			verticalSpeed += gravityConstant * Time.deltaTime * ec.EnvironmentTimeScale * slownessConstant;
			height += ec.EnvironmentTimeScale * verticalSpeed * slownessConstant;

			float limit = heightLimit - heightForcedOffset;

			if (height >= limit)
			{
				height = limit;
				verticalSpeed = 0f;
			}
			renderer.transform.localPosition = Vector3.up * height;
		}

		float verticalSpeed, height;
		Vector3 dir;
		bool isAPuddle = false, initialized = false, despawned = false;
		public bool IsAPuddle => isAPuddle;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprPuddleVariant;

		[SerializeField]
		internal float gravityConstant = -4f, heightLimit = 9f;

		const float slownessConstant = 0.15f, heightForcedOffset = 5f, puddleSpeed = 5f;
	}
}
