using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class BlackOut : RandomEvent
	{
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
			ec.SetAllLights(on);
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

			foreach (var ve in data.Vents)
				ve.DisableVent(!on); // disables/enables vents
			if (on) data.Vents[Random.Range(0, data.Vents.Count)].BlockMe();

			foreach (var soda in FindObjectsOfType<SodaMachine>())
			{
				soda.GetComponent<MeshRenderer>().materials[1].SetTexture("_LightGuide", on ? sodaMachineLight : null); // Switches the texture from the material to make it not glow
			}
			
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
				strength += 0.45f * Time.deltaTime;
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
