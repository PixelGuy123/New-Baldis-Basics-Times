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
		}

		static IEnumerator WaitForAudioPlay(AudioSource source)
		{
			while (!Singleton<MusicManager>.Instance.MidiPlaying) yield return null; // Awaits to play music so it can then wait to stop
			while (Singleton<MusicManager>.Instance.MidiPlaying) yield return null;

			source.PlayOneShot(aud_welcome);

			yield break;
		}

		public static Sprite mainMenu;

		public static AudioClip aud_welcome;
	}
}
