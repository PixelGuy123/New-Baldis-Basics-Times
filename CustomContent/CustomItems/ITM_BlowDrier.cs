﻿using UnityEngine;
using System.Collections;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_BlowDrier : Item
	{
		public override bool Use(PlayerManager pm)
		{
			if (++blowersUsed > 1)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			audMan.maintainLoop = true;
			audMan.QueueAudio(audBlow);
			audMan.SetLoop(true);
			StartCoroutine(Blow());
			return true;
		}

		void OnDestroy() =>
			blowersUsed--;

		IEnumerator Blow()
		{
			float timer = Random.Range(15f, 30f);
			float speed = 0f;
			MovementModifier moveMod = new(Vector3.zero, 0.85f) { forceTrigger = true };
			pm.Am.moveMods.Add(moveMod);
			var cam = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber);

			while (timer > 0f)
			{
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				speed += Time.deltaTime * pm.ec.EnvironmentTimeScale * 1.2f;
				if (speed > maxSpeed)
					speed = maxSpeed;
				moveMod.movementAddend += -cam.transform.forward * speed;
				moveMod.movementAddend.Limit(maxSpeed, maxSpeed, maxSpeed);
				yield return null;
			}

			pm.Am.moveMods.Remove(moveMod);
			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audBlow;

		const float maxSpeed = 45f;

		internal static int blowersUsed = 0;
	}
}
