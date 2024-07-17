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
				__instance.StartCoroutine(WaitForAudioPlay(___audioSource));
			if (!string.IsNullOrEmpty(newMidi))
				__instance.transform.GetComponentInChildren<MusicPlayer>().track = newMidi;
		}

		static IEnumerator WaitForAudioPlay(AudioSource source)
		{
			yield return new WaitForSeconds(seconds); // Music manager makes this pain

			source.PlayOneShot(aud_welcome);

			yield break;
		}

		const int seconds = 4;

		public static Sprite mainMenu;

		public static AudioClip aud_welcome;

		public static string newMidi = string.Empty;
	}
}
