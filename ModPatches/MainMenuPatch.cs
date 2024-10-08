using HarmonyLib;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BBTimes.Manager;
using PixelInternalAPI.Components;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MainMenu), "Start")]
	public class MainMenuPatch // Intentionally public to be changed later with Endless floors
	{
		private static void Postfix(MainMenu __instance)
		{
			if (mainMenu != null)
				__instance.transform.Find("Image").GetComponent<Image>().sprite = mainMenu;
			if (aud_welcome != null)
			{
				var emptMono = new GameObject("TimesWelcomer").AddComponent<EmptyMonoBehaviour>();
				var newSrc = emptMono.gameObject.CreateAudioManager(65, 75).MakeAudioManagerNonPositional();
				newSrc.ignoreListenerPause = true;
				newSrc.audioDevice.playOnAwake = false;

				if (aud_superSecretOnlyReservedForThoseIselect && !File.Exists(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "ShouldNeverBePlayedAgain.timesMarker")))
					emptMono.StartCoroutine(ForcefullyWaitForAudioToPlay(newSrc));
				else
					emptMono.StartCoroutine(WaitForAudioPlay(newSrc));
			}
			if (!string.IsNullOrEmpty(newMidi))
				__instance.transform.GetComponentInChildren<MusicPlayer>().track = newMidi;
		}

		static IEnumerator WaitForAudioPlay(AudioManager source)
		{
			yield return null;
			yield return new WaitForSeconds(seconds); // Music manager makes this pain

			source.QueueAudio(aud_welcome);

			yield break;
		}

		static IEnumerator ForcefullyWaitForAudioToPlay(AudioManager source)
		{
			yield return null;

			CursorController.Instance.DisableClick(true);

			yield return new WaitForSeconds(seconds); // Music manager makes this pain

			source.QueueAudio(aud_superSecretOnlyReservedForThoseIselect);

			while (source.AnyAudioIsPlaying)
				yield return null;

			CursorController.Instance.DisableClick(false);

			File.WriteAllBytes(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "ShouldNeverBePlayedAgain.timesMarker"), []); // Empty file to serve as a marker lol

			yield break;
		}

		const int seconds = 4;

		public static Sprite mainMenu;

		public static SoundObject aud_welcome, aud_superSecretOnlyReservedForThoseIselect; // this is NOT lore btw, it's more of a personal thing lol

		public static string newMidi = string.Empty;
	}
}
