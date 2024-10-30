using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class DustShroom : EnvironmentObject
	{
		void OnTriggerEnter(Collider other)
		{
			if (!IsActive) return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e && e.Grounded)
				{
					if (timer != null)
						StopCoroutine(timer);
					timer = StartCoroutine(Timer());
				}
			}
		}

		void OnTriggerStay(Collider other)
		{
			if (!stillCanPush) return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e)
					e.AddForce(new((other.transform.position - transform.position).normalized, pushingForce, pushingAcceleration));
			}
		}

		IEnumerator Timer()
		{
			float timer = Random.Range(minActiveCooldown, maxActiveCooldown);

			renderer.sprite = sprUsed;
			IsActive = false;
			EnableMush(true);

			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			EnableMush(false);
			timer = Random.Range(minDisabledCooldown, maxDisabledCooldown);

			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			
			IsActive = true;
			renderer.sprite = sprActive;
		}

		void EnableMush(bool enable)
		{
			//if (enable)
				//audMan.PlaySingle(audUsed);
			
		
			var emission = particles.emission;
			emission.enabled = enable;
			raycastBlockingCollider.enabled = enable;
			stillCanPush = enable;
		}

		Coroutine timer;

		public bool IsActive { get; private set; } = true;

		bool stillCanPush = false;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprUsed, sprActive;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audUsed;

		[SerializeField]
		internal ParticleSystem particles;

		[SerializeField]
		internal BoxCollider raycastBlockingCollider;

		[SerializeField]
		internal float minDisabledCooldown = 180f, maxDisabledCooldown = 240f, minActiveCooldown = 25f, maxActiveCooldown = 35f, pushingForce = 15f, pushingAcceleration = -7f;
	}
}
