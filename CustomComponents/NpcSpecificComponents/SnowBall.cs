using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class SnowBall : EnvironmentObject, IEntityTrigger
	{
		public void Spawn(GameObject owner, Vector3 pos, Vector3 dir, float speed, float ySpeed, EnvironmentController ec, float startingHeight = 5f)
		{
			initialized = true;
			entity.Initialize(ec, pos);
			entity.OnEntityMoveInitialCollision += (hit) => { if (!hidden) Destroy(gameObject); };
			moveMod.movementMultiplier = slowFactor;

			this.ec = ec;

			this.dir = dir;
			this.speed = speed;
			yVelocity = ySpeed;

			this.owner = owner;
			height = startingHeight;
		}

		const float heightOffset = -5f;
		float yVelocity = 5f, speed = 0f, height;
		GameObject owner;
		Vector3 dir;
		bool initialized = false, hidden = false;

		void Update()
		{
			if (!initialized)
				return;

			if (hidden)
			{
				entity.UpdateInternalMovement(Vector3.zero);
				return;
			}

			entity.UpdateInternalMovement(dir * speed * ec.EnvironmentTimeScale);

			renderer.localPosition = Vector3.up * (height + heightOffset);
			yVelocity -= ec.EnvironmentTimeScale * Time.deltaTime;

			height += yVelocity * Time.deltaTime * 1.5f;

			if (height > 9.35f)
			{
				yVelocity = 0f;
				height = 9.35f;
			}

			if (height <= 0f)
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
					e.AddForce(new((e.transform.position - transform.position).normalized, hitForce, -hitForce));
					audMan.PlaySingle(audHit);
					StartCoroutine(SlowDown(e, other.GetComponent<PlayerManager>()));
				}
			}
		}

		public void EntityTriggerStay(Collider other) { }

		public void EntityTriggerExit(Collider other) 
		{
			if (other.gameObject == owner)
				owner = null;
		}

		IEnumerator SlowDown(Entity e, PlayerManager pm = null)
		{
			hidden = true;
			renderer.gameObject.SetActive(false);
			entity.collider.enabled = false;
			e.ExternalActivity.moveMods.Add(moveMod);
			targettedMod = e.ExternalActivity;
			PlayerAttributesComponent pmm = null;
			if (pm)
				pmm = pm.GetAttribute();

			while (freezeCooldown > 0f)
			{
				freezeCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				if (pmm && pmm.HasAttribute("boots"))
					break;
				yield return null;
			}

			Destroy(gameObject);
		}

		void OnDestroy() =>
			targettedMod?.moveMods.Remove(moveMod);


		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal float freezeCooldown = 5f, hitForce = 25.5f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slowFactor = 0.05f;

		ActivityModifier targettedMod;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.05f);
	}
}
