using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.FrozenEvent
{
	public class SnowMan : EnvironmentObject
	{
		void OnTriggerEnter(Collider other)
		{
			if (Dead)
				return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
					HitEntity(e);
			}
		}

		void HitEntity(Entity entity)
		{
			renderer.sprite = spritesForEachHit[++lifeCount];
			entity.AddForce(new((entity.transform.position - transform.position).normalized, hitForce, hitAcceleration));
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
		internal float delayBeforeDeath = 5f, hitForce = 45f, hitAcceleration = -12.2f;

		int lifeCount = 0;

		public bool Dead => lifeCount >= (spritesForEachHit.Length - 1);
	}
}
