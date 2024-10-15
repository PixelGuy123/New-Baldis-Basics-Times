using BBTimes.Manager;
using HarmonyLib;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MainMenu), "Start")]
	public class MainMenuPatch // Intentionally public to be changed later with Endless floors
	{
		private static void Postfix(MainMenu __instance)
		{
			bool hasInfiniteFloors = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.endlessfloors");

			__instance.transform.Find("Image").GetComponent<Image>().sprite = hasInfiniteFloors ? mainMenuEndless : mainMenu;

			var emptMono = new GameObject("TimesWelcomer").AddComponent<EmptyMonoBehaviour>();
			var newSrc = emptMono.gameObject.CreateAudioManager(65, 75).MakeAudioManagerNonPositional();
			newSrc.ignoreListenerPause = true;
			newSrc.audioDevice.playOnAwake = false;

			if (aud_superSecretOnlyReservedForThoseIselect && !File.Exists(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "ShouldNeverBePlayedAgain.timesMarker")))
				emptMono.StartCoroutine(ForcefullyWaitForAudioToPlay(newSrc));
			else
				emptMono.StartCoroutine(WaitForAudioPlay(newSrc, hasInfiniteFloors ? aud_welcome_endless : aud_welcome));

			if (!string.IsNullOrEmpty(newMidi))
				__instance.transform.GetComponentInChildren<MusicPlayer>().track = newMidi;
		}

		static IEnumerator WaitForAudioPlay(AudioManager source, SoundObject audio)
		{
			yield return null;
			yield return new WaitForSeconds(seconds); // Music manager makes this pain

			source.QueueAudio(audio);

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

		public static Sprite mainMenu, mainMenuEndless;

		public static SoundObject aud_welcome, aud_welcome_endless, aud_superSecretOnlyReservedForThoseIselect; // this is NOT lore btw, it's more of a personal thing lol

		public static string newMidi = string.Empty;
	}
}
