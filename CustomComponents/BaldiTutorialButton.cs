using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class BaldiTutorialButton : StandardMenuButton
	{

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audTutorialIntroduction, audDontKnowButton, audHmLetsSee;

		[SerializeField]
		internal Sprite[] sprClickMe, sprNormal;

		readonly static Dictionary<StandardMenuButton, SoundObject> tutorialPairs = [];

		readonly static HashSet<StandardMenuButton> falseButtons = [];

		void OnDisable() =>
			audMan.FlushQueue(true);

		public static void AddProhibitedButton(StandardMenuButton button) =>
			falseButtons.Add(button);

		public static void AddButtonTutorial(StandardMenuButton button, SoundObject audTutorial) =>
			tutorialPairs.Add(button, audTutorial);

		public void PlayTutorial(StandardMenuButton button)
		{
			if (!ButtonCanBeClicked(button))
				return;

			InterruptTutorial();
			audMan.QueueAudio(audHmLetsSee);
			if (tutorialPairs.TryGetValue(button, out var audio))
				audMan.QueueAudio(audio);
			else
				audMan.QueueAudio(audDontKnowButton);

			tutorSequence = StartCoroutine(TutorialSequence());
		}

		public void ExplainTutorial()
		{
			InterruptTutorial();
			audMan.QueueAudio(audTutorialIntroduction);
			HasBeenClicked = true;
			tutorSequence = StartCoroutine(TutorialSequence());
		}

		public void InterruptTutorial()
		{
			if (tutorSequence != null)
				StopCoroutine(tutorSequence);
			audMan.FlushQueue(true);

			CanBeClicked = true;
			isWaiting = false;
			transform.localPosition = Vector3.down * 200f;
		}

		public void StartWaitSequence()
		{
			transform.localPosition = Vector3.down * 145f;
			isWaiting = true;
			StartCoroutine(WaitingForClickSequence());
		}

		IEnumerator TutorialSequence()
		{
			image.sprite = sprNormal[0];
			CanBeClicked = false;
			while (audMan.QueuedAudioIsPlaying)
				yield return null;
			CanBeClicked = true;
			transform.localPosition = Vector3.down * 200f;
		}

		IEnumerator WaitingForClickSequence()
		{
			float scale = transform.localScale.x;
			float time = 0f;
			while (isWaiting)
			{
				time += Time.fixedDeltaTime;
				transform.localScale = Vector3.one * (scale + (Mathf.Sin(time * 1.5f) * 0.25f));
				yield return null;
			}
			transform.localScale = Vector3.one * scale;
		}

		Coroutine tutorSequence;
		internal bool HasBeenClicked { get; set; } = false;
		internal bool CanBeClicked { get; private set; } = true;
		public bool ButtonCanBeClicked(StandardMenuButton but) => !falseButtons.Contains(but);
		bool isWaiting = false;
		public bool IsWaiting => isWaiting;
	}
}
