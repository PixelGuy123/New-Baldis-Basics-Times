using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_ComicallyLargeTrumpet : Item
    {
        public override bool Use(PlayerManager pm)
        {
			this.pm = pm;
			StartCoroutine(BlowAndPush());
            return true;
        }

		void PushEveryone()
		{
			foreach (var entity in FindObjectsOfType<Entity>()) // Find every existing entity to make them suffer lol
			{
				if (entity == pm.plm.Entity) continue;

				float force = pushForce - Vector3.Distance(entity.transform.position, pm.transform.position) * pushDistance;
				if (force > 0f)
					entity.AddForce(new((entity.transform.position - pm.transform.position).normalized, force, -force * pushForceDecrement));
			}
		}

		IEnumerator BlowAndPush()
		{
			ValueModifier val = new();
			var cam = pm.GetCustomCam();
			cam.SlideFOVAnimation(val, -35f, 3f);
			float delay = 2.5f;

			while (delay > 0f)
			{
				delay -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			var cor = cam.ShakeFOVAnimation(val, 1.6f, 4f, 99f, 6);
			audMan.PlaySingle(audBlow);
			PushEveryone();

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			cam.StopCoroutine(cor);
			cam.ReverseSlideFOVAnimation(val, 0f);

			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		public float pushForce = 35f;

		[SerializeField]
		[Range(0.0f, 1.0f)]
		public float pushDistance = 0.6f, pushForceDecrement = 0.9f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audBlow;

	}

	
}
