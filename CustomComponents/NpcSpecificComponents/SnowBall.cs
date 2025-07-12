using System.Collections;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class SnowBall : EnvironmentObject, IEntityTrigger, IClickable<int>
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
			{
				gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, freezeCooldown);
				pmm = pm.GetAttribute();
			}

			float ogCooldown = freezeCooldown;

			while (freezeCooldown > 0f)
			{
				freezeCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				gauge?.SetValue(ogCooldown, freezeCooldown);
				if (pmm && pmm.HasAttribute("boots"))
					break;
				yield return null;
			}

			gauge?.Deactivate();

			Destroy(gameObject);
		}

		void OnDestroy() =>
			targettedMod?.moveMods.Remove(moveMod);

		public void Clicked(int player)
		{
			var pm = Singleton<CoreGameManager>.Instance.GetPlayer(player);
			if (deflectedOnce || !IsHeightInRange(pm.plm.Entity.OverriddenHeight)) return;

			deflectedOnce = true;

			dir = pm.transform.forward;
			speed *= deflectionSpeedFactor; // Reverses the direction and increases speed
			yVelocity = deflectionYVelocity; // Resets y velocity

			audMan.PlaySingle(audHit);
		}

		public void ClickableSighted(int player) { }

		public void ClickableUnsighted(int player) { }

		public bool ClickableHidden() => deflectedOnce || !IsHeightInRange(Singleton<CoreGameManager>.Instance.GetPlayer(0).plm.Entity.OverriddenHeight);

		public bool ClickableRequiresNormalHeight() => false;

		bool IsHeightInRange(float height)
		{
			float myHeight = this.height;
			return height >= myHeight - minRangeForHit && height <= myHeight + minRangeForHit;
		}

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float freezeCooldown = 5f, hitForce = 25.5f, deflectionSpeedFactor = 1.95f, deflectionYVelocity = 5f, minRangeForHit = 2.5f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slowFactor = 0.05f;

		ActivityModifier targettedMod;

		HudGauge gauge;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.12f);

		bool deflectedOnce = false;
	}
}
