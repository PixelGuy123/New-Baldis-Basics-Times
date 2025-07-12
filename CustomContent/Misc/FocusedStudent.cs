using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.Misc
{
	public class FocusedStudent : EnvironmentObject
	{
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			var room = ec.CellFromPosition(transform.position).room;
			var fun = room.functionObject.GetComponent<FocusRoomFunction>();
			if (!fun)
			{
				fun = room.functionObject.AddComponent<FocusRoomFunction>();
				room.functions.AddFunction(fun);
				fun.Initialize(room);
			}
			fun.Setup(this);
			pos = transform.position;
			initialized = true;

			activeAppearance = appearanceSet[Random.Range(0, appearanceSet.Length)];
			renderer.sprite = activeAppearance.Reading;
			audMan.subtitleColor = activeAppearance.subtitleColor;
		}

		public bool Disturbed(PlayerManager player)
		{
			speaking = true;
			audMan.FlushQueue(true);
			if (++disturbedCount >= 3)
			{
				shaking = true;
				renderer.sprite = activeAppearance.Screaming;
				FullyRelax();
				player.RuleBreak("Running", 5f, 0.2f);
				audMan.QueueAudio(activeAppearance.audDisturbed);
				ec.CallOutPrincipals(transform.position);
				return true;
			}
			audMan.QueueAudio(disturbedCount == 1 ? activeAppearance.audAskSilence : activeAppearance.audAskSilence2);
			renderer.sprite = activeAppearance.Speaking;
			return false;
		}

		public void Relax() =>
			disturbedCount = Mathf.Max(0, disturbedCount - 1);

		public void FullyRelax() =>
			disturbedCount = 0;

		void Update()
		{
			if (!initialized) return;

			if (!audMan.QueuedAudioIsPlaying)
			{
				renderer.sprite = activeAppearance.Reading;
				shaking = false;
				speaking = false;
				transform.position = pos;
			}

			if (shaking && Time.timeScale != 0)
				transform.position = pos + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
		}


		Vector3 pos;
		bool shaking = false, speaking = false, initialized = false;
		public bool IsSpeaking => speaking;
		int disturbedCount = 0;
		Appearances activeAppearance;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SpriteRenderer renderer;


		internal static Appearances[] appearanceSet; // Sadly this can't be serialized as instance; I have no idea how to do that

		[System.Serializable]
		public class Appearances
		{
			public Sprite Reading;
			public Sprite Speaking;
			public Sprite Screaming;

			public SoundObject audAskSilence;
			public SoundObject audAskSilence2;
			public SoundObject audDisturbed;

			public Color subtitleColor;
		}
	}

}
