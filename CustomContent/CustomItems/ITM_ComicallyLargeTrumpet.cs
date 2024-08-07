﻿using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_ComicallyLargeTrumpet : Item
    {
        public override bool Use(PlayerManager pm)
        {
			if (++usingTrompets > 1)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			StartCoroutine(BlowAndPush());
            return true;
        }

		void PushEveryone()
		{
			foreach (var entity in FindObjectsOfType<Entity>()) // Find every existing entity to make them suffer lol
			{
				var ray = new Ray(transform.position, (entity.transform.position - transform.position).normalized);
				if (entity == pm.plm.Entity) continue;

				float force = pushForce - (Vector3.Distance(entity.transform.position, pm.transform.position) * pushDistance);
				if (force > 0f)
					entity.AddForce(new((entity.transform.position - pm.transform.position).normalized, force, -force * pushForceDecrement));
			}
		}

		void OnDestroy() =>
			usingTrompets--;

		IEnumerator BlowAndPush()
		{
			ValueModifier val = new();
			var cam = pm.GetCustomCam();
			cam.SlideFOVAnimation(val, -35f, 3f);
			float delay = 2f;

			while (delay > 0f)
			{
				delay -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			var cor = cam.ShakeFOVAnimation(val, 1f, 20f, 99f, 2);
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
		public float pushForce = 175f;

		[SerializeField]
		[Range(0.0f, 1.0f)]
		public float pushDistance = 0.6f, pushForceDecrement = 0.6f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audBlow;

		static int usingTrompets = 0;

	}

	
}
