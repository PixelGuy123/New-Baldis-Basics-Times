using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Basketball : Item, IEntityTrigger
	{
		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audThrow);
			this.pm = pm;
			target = pm.gameObject;
			entity.Initialize(pm.ec, pm.transform.position);
			dir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;

			entity.OnEntityMoveInitialCollision += (hit) => // Basically just bounce over
			{
				if (hasHit) return; // stop BONG spam

				dir = Vector3.Reflect(dir, hit.normal); // crazy math I guess
				audMan.PlaySingle(audBong);
			};

			return true;
		}

		void Update()
		{
			if (hasHit) return;

			entity.UpdateInternalMovement(dir * speed * pm.ec.EnvironmentTimeScale);

			lifeTime -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime < 0f)
				Destroy(gameObject);


			// animation loop
			frame += 8f * pm.ec.EnvironmentTimeScale * Time.deltaTime;
			frame %= spriteAnim.Length;
			renderer.sprite = spriteAnim[Mathf.FloorToInt(frame)];
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (hasHit || other.gameObject == target) return;
			bool isnpc = other.CompareTag("NPC");
			if (other.isTrigger && (isnpc || other.CompareTag("Player")))
			{
				Entity e = other.GetComponent<Entity>();
				if (e)
				{
					if (isnpc) pm.RuleBreak("Bullying", 1f);
					audMan.PlaySingle(audHit);
					renderer.enabled = false;
					hasHit = true;

					e.AddForce(new((other.transform.position - transform.position).normalized, speed * 1.9f, -speed));

					StartCoroutine(Timer(e));
				}
			}

		}
		public void EntityTriggerStay(Collider other)
		{
		}
		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == target)
				target = null;
		}

		IEnumerator Timer(Entity e)
		{
			e.ExternalActivity.moveMods.Add(moveMod);
			float cooldown = 15f;
			while (cooldown > 0)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			e.ExternalActivity.moveMods.Remove(moveMod);
			Destroy(gameObject);

			yield break;
		}

		GameObject target;
		float frame = 0f, lifeTime = 160f;
		bool hasHit = false;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] spriteAnim;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow, audHit, audBong;

		Vector3 dir;

		const float speed = 25f;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.2f);
	}
}
