using HarmonyLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MainMenu), "Start")]
	public class MainMenuPatch // Intentionally public to be changed later with Endless floors
	{
		private static void Postfix(MainMenu __instance, ref AudioSource ___audioSource)
		{
			if (mainMenu != null)
				__instance.transform.Find("Image").GetComponent<Image>().sprite = mainMenu;
			if (aud_welcome != null)
			{
				var newSrc = __instance.gameObject.AddComponent<AudioSource>();
				newSrc.bypassListenerEffects = ___audioSource.bypassListenerEffects; // ALL the priorities lol
				newSrc.dopplerLevel = ___audioSource.dopplerLevel;
				newSrc.ignoreListenerPause = ___audioSource.ignoreListenerPause;
				newSrc.ignoreListenerVolume = ___audioSource.ignoreListenerVolume;
				newSrc.loop = ___audioSource.loop;
				newSrc.maxDistance = ___audioSource.maxDistance;
				newSrc.minDistance = ___audioSource.minDistance;
				newSrc.panStereo = ___audioSource.panStereo;
				newSrc.pitch = ___audioSource.pitch;
				newSrc.priority = ___audioSource.priority;
				newSrc.reverbZoneMix = ___audioSource.reverbZoneMix;
				newSrc.rolloffMode = ___audioSource.rolloffMode;
				newSrc.spatialBlend = ___audioSource.spatialBlend;
				newSrc.spread = ___audioSource.spread;
				newSrc.spatialize = ___audioSource.spatialize;

				__instance.StartCoroutine(WaitForAudioPlay(newSrc));
			}
			if (!string.IsNullOrEmpty(newMidi))
				__instance.transform.GetComponentInChildren<MusicPlayer>().track = newMidi;
		}

		static IEnumerator WaitForAudioPlay(AudioSource source)
		{
			yield return null;
			source.clip = aud_welcome;
			yield return new WaitForSeconds(seconds); // Music manager makes this pain

			source.Play();

			yield break;
		}

		const int seconds = 4;

		public static Sprite mainMenu;

		public static AudioClip aud_welcome;

		public static string newMidi = string.Empty;
	}
}
