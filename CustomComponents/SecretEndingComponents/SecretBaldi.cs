using BBTimes.Extensions;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.SecretEndingComponents
{
	internal class SecretBaldi : EnvironmentObject
	{

		// The computer cutscene
		[SerializeField]
		internal Sprite[] sprLookingComputer, sprOnlyPeek, sprSideEyeBack, sprFacingFront, sprFacingFrontNervous;

		// The angry cutscene
		[SerializeField]
		internal Sprite[] sprAngryBal, sprAngryHappyBal, sprAngryHappySideEyeBal, sprThinkingBal, sprTakeRulerAnim, sprWithRulerBal, sprCatchBal;

		[SerializeField]
		internal SpriteVolumeAnimator volumeAnimator;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audMeetMe1, audMeetMe2, audMeetMe3, audMeetMe4,
			audAngry1, audAngry2, audAngry3, audAngry4, audAngry5, audAngry6, audAngry7, audAngry8, audAngry9;

		[SerializeField]
		internal SpriteRenderer renderer;

		public bool TriggeredDialogue { get; private set; } = false;
		public bool IsTalking => audMan.QueuedAudioIsPlaying;

		public void TriggerWarningSequence(PlayerManager pm) =>
			StartCoroutine(WarningSequence(pm));
		public void TriggerEndSequence(PlayerManager pm) =>
			StartCoroutine(EndSequence(pm));
		void OnTriggerEnter(Collider other)
		{
			if (!TriggeredDialogue && other.CompareTag("Player"))
			{
				TriggeredDialogue = true;
				StartCoroutine(MeetSequence(other.GetComponent<PlayerManager>()));
			}
		}

		internal void UpdateSpriteTo(params Sprite[] newOne)
		{
			renderer.sprite = newOne[0];
			volumeAnimator.sprites = newOne;
		}

		IEnumerator MeetSequence(PlayerManager pm)
		{
			UpdateSpriteTo(sprLookingComputer);

			ValueModifier val = new(1f, 0f);
			pm.plm.Entity.SetFrozen(true);
			var pmCustomCam = pm.GetCustomCam();
			pmCustomCam.SlideFOVAnimation(val, -12f, 4f);

			audMan.QueueAudio(audMeetMe1);

			foreach (var wall in FindObjectsOfType<NoRendererOnStart>())
				if (wall.canBeDisabled)
					wall.gameObject.SetActive(false);

			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audMeetMe2);
			UpdateSpriteTo(sprOnlyPeek);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;


			for (int i = 0; i < sprSideEyeBack.Length; i++)
			{
				UpdateSpriteTo(sprSideEyeBack[i]);
				yield return null;
				yield return null;
				yield return null; // 3 frames one frame
			}

			audMan.QueueAudio(audMeetMe3);
			UpdateSpriteTo(sprFacingFront);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audMeetMe4);
			UpdateSpriteTo(sprFacingFrontNervous);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			yield return null;
			yield return null;
			yield return null;

			pmCustomCam.ResetSlideFOVAnimation(val, 3.5f);
			pm.plm.Entity.SetFrozen(false);

			float delay = 1f;
			while (delay > 0f)
			{
				delay -= Time.deltaTime;
				yield return null;
			}

			for (int i = sprSideEyeBack.Length - 1; i >= 0; i--)
			{
				UpdateSpriteTo(sprSideEyeBack[i]);
				yield return null;
				yield return null;
				yield return null; // 3 frames one frame
			}
			yield return null;

			UpdateSpriteTo(sprLookingComputer[0]);
		}

		IEnumerator WarningSequence(PlayerManager pm)
		{
			UpdateSpriteTo(sprAngryBal);
			pm.plm.Entity.SetFrozen(true);
			audMan.FlushQueue(true);
			audMan.QueueAudio(audAngry1);

			Vector3 lookReference = transform.position;
			lookReference.y = pm.transform.position.y;

			while (audMan.QueuedAudioIsPlaying)
			{
				pm.transform.RotateSmoothlyToNextPoint(lookReference, 0.65f);
				yield return null;
			}

			// Don't set frozen back, so player needs to forcefully pull the lever

		}

		IEnumerator EndSequence(PlayerManager pm)
		{
			UpdateSpriteTo(sprAngryHappyBal);
			pm.plm.Entity.SetFrozen(true);
			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).SetControllable(false);
			audMan.FlushQueue(true);
			audMan.QueueAudio(audAngry2);

			Vector3 lookReference = transform.position;
			lookReference.y = pm.transform.position.y;

			while (audMan.QueuedAudioIsPlaying)
			{
				pm.transform.RotateSmoothlyToNextPoint(lookReference, 0.4f);
				yield return null;
			}

			UpdateSpriteTo(sprAngryBal);
			audMan.QueueAudio(audAngry3);

			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audAngry4);
			UpdateSpriteTo(sprThinkingBal);

			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audAngry5);
			UpdateSpriteTo(sprAngryHappyBal);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audAngry6);
			UpdateSpriteTo(sprAngryBal);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audAngry7);
			UpdateSpriteTo(sprAngryHappySideEyeBal);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audAngry8);
			UpdateSpriteTo(sprAngryBal);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			for (int i = 0; i < sprTakeRulerAnim.Length; i++)
			{
				UpdateSpriteTo(sprTakeRulerAnim[i]);
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				yield return null; // 6 frames one frame
			}

			audMan.QueueAudio(audAngry9);
			UpdateSpriteTo(sprWithRulerBal);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			// Don't set frozen back, so player needs to forcefully pull the lever

			Vector3 ogPos = transform.position;
			float t = 0f, frameTime = 0f, timeAccumulation = 0f;
			while (true)
			{
				t += Time.deltaTime * 12f * timeAccumulation;
				timeAccumulation += Time.deltaTime * 2f;
				frameTime += Time.deltaTime * 21f;
				transform.position = Vector3.Lerp(ogPos, pm.transform.position, t);
				if (t >= 1f)
				{
					Singleton<BaseGameManager>.Instance.CallSpecialManagerFunction(0, gameObject); // Should be the TimesEnding as expected
					yield break;
				}
				int idx = Mathf.FloorToInt(frameTime);
				if (idx < sprCatchBal.Length)
					UpdateSpriteTo(sprCatchBal[idx]);
				
				yield return null;
			}
		}
	}
}
