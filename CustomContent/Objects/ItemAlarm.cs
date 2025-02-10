using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class ItemAlarm : EnvironmentObject
	{
		public void AttachToPickup(Pickup itmPick)
		{
			linkedPickup = itmPick;
			itmPick.OnItemCollected += OnItemCollected;
			localPos = renderer.transform.localPosition;
			initialized = true;
		}

		void OnItemCollected(Pickup pickup, int player)
		{
			if (triggered)
				return;

			triggered = true;
			audMan.FlushQueue(true);
			audMan.QueueAudio(audAlarm);
			ec.MakeNoise(Singleton<CoreGameManager>.Instance.GetPlayer(player).transform.position, noiseValue);
			ec.GetBaldi()?.GetExtraAnger(angerLevel);

			if (shakeCor != null)
				StopCoroutine(shakeCor);
			shakeCor = StartCoroutine(Shake());
		}

		IEnumerator Shake()
		{
			while (audMan.QueuedAudioIsPlaying)
			{
				if (Time.timeScale != 0f)
					renderer.transform.localPosition = localPos + Random.insideUnitSphere * 0.15f;
				
				yield return null;
			}
			renderer.transform.localPosition = localPos;
		}

		void Update()
		{
			if (!linkedPickup && initialized)
			{
				Destroy(gameObject);
				return;
			}

			transform.position = linkedPickup.transform.position;
		}

		Coroutine shakeCor;
		Vector3 localPos;

		[SerializeField]
		internal SoundObject audAlarm;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal int noiseValue = 48;

		[SerializeField]
		internal float angerLevel = 2.5f;

		Pickup linkedPickup;
		public Pickup LinkedPickup => linkedPickup;
		bool initialized = false, triggered = false;
	}
}
