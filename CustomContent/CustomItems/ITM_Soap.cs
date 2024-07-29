using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Soap : Item, IEntityTrigger
	{
		public override bool Use(PlayerManager pm)
		{
			ec = pm.ec;
			pm.RuleBreak("littering", 2f, 0.8f);
			StartCoroutine(Fall());
			StartCoroutine(SpeedDelay());
			entity.Initialize(ec, pm.transform.position);
			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				direction = Vector3.Reflect(direction, hit.normal);
				time -= 0.2f;
			};
			direction = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			audMan.PlaySingle(audThrow);
			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (!canHitEntities) return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					e.AddForce(new((other.transform.position - transform.position).normalized, speed * 2.5f, -speed * 2.5f));
					audMan.PlaySingle(audHit);
					direction = Random.insideUnitSphere.normalized;
				}
			}
		}
		public void EntityTriggerExit(Collider other) 
		{
			if (target && target.gameObject == other.gameObject)
			{
				target.ExternalActivity.moveMods.Remove(moveMod);
				target = null;
				canCarry = true;
				canHitEntities = false;
			}
		}
		public void EntityTriggerStay(Collider other)
		{
			if (!canCarry) return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					canHitEntities = true;
					canCarry = false;
					target = e;
					e.Teleport(transform.position);
					e.ExternalActivity.moveMods.Add(moveMod);
				}
			}
		}

		IEnumerator Fall()
		{
			renderer.transform.localPosition = Vector3.zero;
			float fallSpeed = 5f;
			while (true)
			{
				fallSpeed -= ec.EnvironmentTimeScale * Time.deltaTime * 36f;
				renderer.transform.localPosition += Vector3.up * fallSpeed * Time.deltaTime * ec.EnvironmentTimeScale;
				if (renderer.transform.localPosition.y <= fallLimit)
				{
					renderer.transform.localPosition = Vector3.up * fallLimit;
					break;
				}

				yield return null;
			}

			canCarry = true;
			audMan.FlushQueue(true);
			audMan.QueueAudio(audRunLoop);
			audMan.SetLoop(true);
			yield break;
		}

		IEnumerator SpeedDelay()
		{
			while (true)
			{
				speed += (speedLimit - speed) * 0.4f * ec.EnvironmentTimeScale * Time.deltaTime;
				if (speed <= speedLimit)
				{
					speed = speedLimit;
					yield break;
				}
				yield return null;
			}
		}

		void Update()
		{
			entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
			moveMod.movementAddend = entity.ExternalActivity.Addend + direction * speed * ec.EnvironmentTimeScale;
			time -= Time.deltaTime * ec.EnvironmentTimeScale;

			if (time <= 0f)
				Destroy(gameObject);
			
		}

		void OnDestroy() =>
			target?.ExternalActivity.moveMods.Remove(moveMod);
		

		bool canCarry = false, canHitEntities = false;
		float time = 60f, speed = Random.Range(68f, 78f);
		readonly float speedLimit = Random.Range(45f, 56f);
		const float fallLimit = -4f;

		Entity target;
		EnvironmentController ec;
		readonly MovementModifier moveMod = new(Vector3.zero, 0f, 6);
		Vector3 direction;

		public bool HoldingEntity => canHitEntities;
		public Vector3 Direction => direction;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow, audHit, audRunLoop;
	}
}
