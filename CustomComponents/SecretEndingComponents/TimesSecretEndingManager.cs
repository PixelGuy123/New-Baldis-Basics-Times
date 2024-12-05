﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomComponents.SecretEndingComponents
{
	internal class TimesSecretEndingManager : BaseGameManager
	{
		public override void BeginPlay()
		{
			base.BeginPlay();
			Singleton<CoreGameManager>.Instance.disablePause = true;
			Singleton<CoreGameManager>.Instance.GetPlayer(0).itm.ClearItems();
		}

		public override void Initialize()
		{
			if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free) // NUU UHU UH
			{
				Destroy(Singleton<ElevatorScreen>.Instance.gameObject);
				Singleton<CoreGameManager>.Instance.Quit();
				Destroy(gameObject);
				return;
			}
			
			Singleton<CoreGameManager>.Instance.SpawnPlayers(ec);
			Singleton<CoreGameManager>.Instance.SaveEnabled = false;
			Singleton<CoreGameManager>.Instance.readyToStart = true;

			Singleton<CoreGameManager>.Instance.ResetCameras();
			Singleton<CoreGameManager>.Instance.ResetShaders();
			Singleton<CoreGameManager>.Instance.GetHud(0).SetNotebookDisplay(false);
			if (beginPlayImmediately)
			{
				BeginPlay();
			}

			Shader.SetGlobalColor("_SkyboxColor", Color.black);
		}
		public override void CallSpecialManagerFunction(int val, GameObject source)
		{
			if (val != 0 || startedTheEnd) return; // There will be no other but one call

			startedTheEnd = true;
			StartCoroutine(StopPlayerFromListening());
		}

		IEnumerator StopPlayerFromListening()
		{
			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(0).canvasCam;

			Singleton<CoreGameManager>.Instance.GetHud(0).Hide(true); // No hud anymore
			Singleton<CoreGameManager>.Instance.GetPlayer(0).plm.Entity.SetFrozen(true);
			Singleton<CoreGameManager>.Instance.GetCamera(0).SetControllable(false);
			Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position = new(999f, 5f, 999f); // Far away from any audio manager

			Singleton<CoreGameManager>.Instance.audMan.FlushQueue(true);
			Singleton<CoreGameManager>.Instance.audMan.QueueAudio(audSlap);

			float delay = 2f;
			while (delay > 0f)
			{
				delay -= Time.deltaTime;
				yield return null;
			}

			Singleton<CoreGameManager>.Instance.audMan.QueueAudio(audSeeYaSoon);

			activeImage.sprite = timesScreen;
			Singleton<GlobalCam>.Instance.FadeIn(UiTransition.Dither, 0.2f);

			delay = 5f;
			while (delay > 0f)
			{
				delay -= Time.deltaTime;
				yield return null;
			}

			if (Singleton<ElevatorScreen>.Instance)
				Destroy(Singleton<ElevatorScreen>.Instance.gameObject);
			Singleton<CoreGameManager>.Instance.Quit();
			Destroy(gameObject);
		}

		[SerializeField]
		internal SoundObject audSlap, audSeeYaSoon;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal Image activeImage;

		[SerializeField]
		internal Sprite timesScreen;

		bool startedTheEnd = false;
	}
}
