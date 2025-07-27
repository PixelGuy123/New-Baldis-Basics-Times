using System.Collections;
using BBTimes.Plugin;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.FrozenEvent
{
	public class SnowMan : EnvironmentObject
	{
		void OnTriggerEnter(Collider other)
		{
			if (Dead)
				return;

			bool isPlayer = other.CompareTag("Player");

			if (other.isTrigger && (other.CompareTag("NPC") || isPlayer))
			{
				if (isPlayer && other.TryGetComponent<PlayerAttributesComponent>(out var attrs) && attrs.HasAttribute(Storage.HOTCHOCOLATE_ATTR_TAG))
				{
					collider.enabled = false;
					StartCoroutine(Die());
				}

				if (other.TryGetComponent<Entity>(out var e))
					HitEntity(e);
			}
		}

		void HitEntity(Entity entity)
		{
			renderer.sprite = spritesForEachHit[++lifeCount];
			float factor = Mathf.Clamp01(1f - (lifeHitFactor * (lifeCount / (spritesForEachHit.Length - 1)))); // lifeCount: 0 = Full force, 1 = slightly strong force, 2 = mild force...
			entity.AddForce(new((entity.transform.position - transform.position).normalized, hitForce * factor, hitAcceleration)); // The acceleration stays the same
			audMan.PlaySingle(audHit);

			if (Dead)
			{
				collider.enabled = false;
				StartCoroutine(Die());
			}
		}

		IEnumerator Die()
		{
			while (delayBeforeDeath > 0f)
			{
				delayBeforeDeath -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			Color color = Color.white;
			float alphaSpeed = 0f;

			while (true)
			{
				alphaSpeed += ec.EnvironmentTimeScale * Time.deltaTime * 0.05f;
				color.a -= alphaSpeed;
				color.a = Mathf.Clamp01(color.a);

				renderer.color = color;
				if (color.a == 0f)
				{
					Destroy(gameObject);
					yield break;
				}

				yield return null;
			}
		}

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal Sprite[] spritesForEachHit;

		[SerializeField]
		internal Collider collider;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal float delayBeforeDeath = 5f, hitForce = 45f, hitAcceleration = -16.5f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float lifeHitFactor = 0.75f;

		int lifeCount = 0;

		public bool Dead => lifeCount >= (spritesForEachHit.Length - 1);
	}
}
