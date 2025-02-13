﻿using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections;
using UnityEngine;
using PixelInternalAPI.Extensions;


namespace BBTimes.CustomContent.Events
{
    public class BlackOut : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("Bal_Blackout.wav", "Event_BlackOut0", SoundType.Voice, Color.green);
			eventIntro.additionalKeys = [
				new() {time = 1.696f, key = "Event_BlackOut1"},
				new() {time = 3.012f, key = "Event_BlackOut2"},
				new() {time = 4.481f, key = "Event_BlackOut3"},
				new() {time = 6.550f, key = "Event_BlackOut4"}
				];

			audMan = gameObject.CreateAudioManager(85, 105)
				.MakeAudioManagerNonPositional();
			audOff = this.GetSoundNoSub("blackout_out.wav", SoundType.Effect);
			audOn = this.GetSoundNoSub("blackout_on.wav", SoundType.Effect);
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void Begin()
		{
			base.Begin();

			StartCoroutine(FadeOnFog());
			audMan.PlaySingle(audOff);
			TurnStructs(false);
		}

		public override void End()
		{
			base.End();
			ec.RemoveFog(fog);
			audMan.PlaySingle(audOn);
			TurnStructs(true);
		}

		void TurnStructs(bool on) // anything could patch this too :)
		{
			foreach (var cell in ec.AllCells())
				cell.SetPower(on); // Disable power
			
			ec.MaxRaycast = on ? float.PositiveInfinity : maxRayCast;
			if (on) activeBlackOuts--;
			else activeBlackOuts++;

			var data = ec.GetComponent<EnvironmentControllerData>();
			foreach (var co in data.ConveyorBelts)
			{
				var audMan = co.transform.Find("Audio").GetComponent<AudioManager>();
				if (on)
				{
					co.gameObject.SetActive(true);
					if (!audMan.QueuedAudioIsPlaying)
						PrivateCalls.RestartAudioManager(audMan);
				}
				else
				{
					co.gameObject.SetActive(false);
					audMan.FlushQueue(true);
				}
			}

			if (data.Vents.Count > 0) // Fixing an oversight
			{
				foreach (var ve in data.Vents)
					ve.DisableVent(!on); // disables/enables vents
				if (on) data.Vents[Random.Range(0, data.Vents.Count)].BlockMe();
			}

			foreach (var squ in data.Squishers)
				squ.TurnMe(on);

			foreach (var cam in data.Cameras)
				cam.TurnMe(on);

			foreach (var soda in FindObjectsOfType<SodaMachine>())
				soda.GetComponent<MeshRenderer>().materials[1].SetTexture("_LightGuide", on ? sodaMachineLight : null); // Switches the texture from the material to make it not glow
			
			
		}

		void OnDestroy() => activeBlackOuts--;

		IEnumerator FadeOnFog()
		{
			ec.AddFog(fog);
			fog.color = Color.black;
			fog.startDist = 4f;
			fog.maxDist = 100f;
			fog.strength = 0f;
			float strength = 0f;
			while (strength < 1f)
			{
				strength += 0.45f * Time.deltaTime * ec.EnvironmentTimeScale;
				fog.strength = strength;
				ec.UpdateFog();
				yield return null;
			}

			fog.strength = 1f;
			ec.UpdateFog();

			yield break;
		}

		readonly Fog fog = new();

		internal static int activeBlackOuts = 0;

		internal static Texture sodaMachineLight;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audOff, audOn;

		const float maxRayCast = 35f;
	}
}
