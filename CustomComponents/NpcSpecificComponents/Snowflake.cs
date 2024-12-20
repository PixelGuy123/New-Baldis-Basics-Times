using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class Snowflake : EnvironmentObject, IEntityTrigger
	{
		public void Spawn(GameObject owner, Vector3 pos, Vector3 dir, float speed, EnvironmentController ec)
		{
			initialized = true;
			this.ec = ec;
			moveMod.movementMultiplier = slowFactor;

			entity.Initialize(ec, pos);
			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				this.dir = Vector3.Reflect(this.dir, hit.normal);
				lifeTime -= 0.5f;
			};

			this.dir = dir;
			this.speed = speed;

			this.owner = owner;
		}

		void Update()
		{
			if (!initialized) return;

			if (hidden)
			{
				entity.UpdateInternalMovement(Vector3.zero);
				return;
			}

			entity.UpdateInternalMovement(dir * speed * ec.EnvironmentTimeScale);

			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime < 0f)
				Destroy(gameObject);
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (other.gameObject == owner || hidden)
				return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					audMan.PlaySingle(audHit);
					StartCoroutine(SlowDown(e, other.GetComponent<PlayerManager>()));
				}
			}
		}

		public void EntityTriggerStay(Collider other) { }

		public void EntityTriggerExit(Collider other) { }

		IEnumerator SlowDown(Entity e, PlayerManager pm = null)
		{
			AffectEntity(e, pm);
			hidden = true;
			renderer.SetActive(false);
			e.ExternalActivity.moveMods.Add(moveMod);
			targettedMod = e.ExternalActivity;
			PlayerAttributesComponent pmm = null;
			if (pm)
				pmm = pm.GetAttribute();

			while (freezeCooldown > 0f)
			{
				freezeCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;

				if (!ignoreBootAttribute && pmm && pmm.HasAttribute("boots"))
					break;
				yield return null;
			}

			Destroy(gameObject);
		}

		void OnDestroy()
		{
			Despawn();
			targettedMod?.moveMods.Remove(moveMod);
		}

		protected virtual void AffectEntity(Entity e, PlayerManager pm) { }
		protected virtual void Despawn() { }

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal GameObject renderer;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slowFactor = 0.65f;

		[SerializeField]
		internal float freezeCooldown = 15f, lifeTime = 30f;

		[SerializeField]
		internal bool ignoreBootAttribute = false;


		bool initialized = false, hidden = false;
		float speed = 2f;
		Vector3 dir;
		GameObject owner;
		ActivityModifier targettedMod;

		readonly MovementModifier moveMod = new(Vector3.zero, 1f);
	}
}
