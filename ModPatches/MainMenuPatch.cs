using BBTimes.CustomComponents;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using PixelInternalAPI.Classes;
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
			// Main Menu itself
			bool hasInfiniteFloors = BBTimesManager.plug.HasInfiniteFloors;

			__instance.transform.Find("Image").GetComponent<Image>().sprite = hasInfiniteFloors ? mainMenuEndless : mainMenu;

			var emptMono = new GameObject("TimesWelcomer").AddComponent<EmptyMonoBehaviour>();
			var newSrc = emptMono.gameObject.CreateAudioManager(65, 75).MakeAudioManagerNonPositional();
			newSrc.ignoreListenerPause = true;
			newSrc.audioDevice.playOnAwake = false;

			if (aud_superSecretOnlyReservedForThoseIselect && !File.Exists(Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "ShouldNeverBePlayedAgain.timesMarker")))
				emptMono.StartCoroutine(ForcefullyWaitForAudioToPlay(newSrc));
			else
				emptMono.StartCoroutine(WaitForAudioPlay(newSrc, hasInfiniteFloors ? aud_welcome_endless : aud_welcome, __instance.gameObject));

			if (!string.IsNullOrEmpty(newMidi))
				__instance.transform.GetComponentInChildren<MusicPlayer>().track = newMidi;

			// Screen Selection changes

			var mainModeBut = GenericExtensions.FindResourceObject<MainModeButtonController>();

			if (baldiButPre)
				goto instantiateBaldiPrefab;

			// Adding buttons
			BaldiTutorialButton.AddProhibitedButton(mainModeBut.transform.Find("BackButton").GetComponent<StandardMenuButton>());
			BaldiTutorialButton.AddProhibitedButton(mainModeBut.transform.Find("SeedInput").GetComponent<StandardMenuButton>());

			AddButtonTut("Endless", "BAL_Explains_EndlessMode.wav", "Vfx_BAL_MenuExplain_EndlessMode_1", 
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_2", time = 2.724f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_3", time = 4.864f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_4", time = 8.569f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_5", time = 11.088f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_6", time = 14.713f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_7", time = 19.312f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_8", time = 21.823f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_9", time = 24.57f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_10", time = 27.024f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_11", time = 28.417f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_12", time = 33.176f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_13", time = 36.029f }
				);
			void AddButtonTut(string name, string audioName, string sub, params SubtitleTimedKey[] keys)
			{
				var sndobj = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(
				Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BaldiBut", BBTimesManager.GetAssetName(audioName))), sub, SoundType.Effect, Color.green);

				BaldiTutorialButton.AddButtonTutorial(mainModeBut.transform.Find(name).GetComponent<StandardMenuButton>(), sndobj);
				sndobj.additionalKeys = keys;
			}

			var baldiButtonObj = new GameObject("BaldiButton")
			{
				layer = LayerStorage.ui
			};

			var baldiSheet = TextureExtensions.LoadSpriteSheet(2, 2, 1f, BBTimesManager.MiscPath, BBTimesManager.TextureFolder, "BaldiBut", BBTimesManager.GetAssetName("baldiClickOnMeButton.png"));
			var baldiButImg = baldiButtonObj.AddComponent<Image>();
			baldiButImg.sprite = baldiSheet[0];

			var baldiAudMan = baldiButtonObj.CreateAudioManager(65, 75).MakeAudioManagerNonPositional();
			baldiAudMan.ignoreListenerPause = true;
			baldiAudMan.audioDevice.playOnAwake = false;

			var baldiButton = UIExtensions.ConvertToButton<BaldiTutorialButton>(baldiButtonObj);
			baldiButton.audMan = baldiAudMan;
			baldiButton.audTutorialIntroduction = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(
				Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BaldiBut", BBTimesManager.GetAssetName("BAL_Explains_Tutorial.wav"))), "Vfx_BAL_MenuExplain_Tutorial_1", SoundType.Effect, Color.green);
			baldiButton.audTutorialIntroduction.additionalKeys = [
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_2", time = 1.691f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_3", time = 3.757f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_4", time = 7.277f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_5", time = 10.323f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_6", time = 14.851f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_7", time = 17.865f }
				];

			baldiButton.audDontKnowButton = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(
				Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BaldiBut", BBTimesManager.GetAssetName("BAL_Explains_HowWeirdSelections.wav"))), "Vfx_BAL_MenuExplain_HowWeird_1", SoundType.Effect, Color.green);
			baldiButton.audDontKnowButton.additionalKeys = [
				new() { key = "Vfx_BAL_MenuExplain_HowWeird_2", time = 1.818f },
				new() { key = "Vfx_BAL_MenuExplain_HowWeird_3", time = 4.359f },
				new() { key = "Vfx_BAL_MenuExplain_HowWeird_4", time = 7.437f }
				];

			baldiButton.audHmLetsSee = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(
				Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BaldiBut", BBTimesManager.GetAssetName("BAL_Explains_HmmLetsSee.wav"))), "Vfx_BAL_MenuExplain_CheckingOut", SoundType.Effect, Color.green);


			baldiButton.sprClickMe = [baldiSheet[0], baldiSheet[1]];
			baldiButton.sprNormal = [baldiSheet[2], baldiSheet[3]];

			baldiButtonObj.ConvertToPrefab(true);
			baldiButPre = baldiButton;

		instantiateBaldiPrefab:
			var baldiButtonClone = Object.Instantiate(baldiButPre);
			
			baldiButtonClone.transform.SetParent(mainModeBut.transform, false);
			baldiButtonClone.transform.localPosition = Vector3.down * 200f;
			baldiButtonClone.transform.localScale = Vector3.one * 1.25f;
			baldiButtonClone.transform.SetSiblingIndex(6);
			SetupBaldiButtonCalls(baldiButtonClone);

			static void SetupBaldiButtonCalls(BaldiTutorialButton baldiButton)
			{
				baldiButton.eventOnHigh = true;
				baldiButton.OnHighlight.AddListener(() =>
				{
					baldiButton.image.sprite = baldiButton.HasBeenClicked ? baldiButton.sprNormal[1] : baldiButton.sprClickMe[1];
					baldiButton.transform.localPosition = Vector3.down * 145f;
				});
				baldiButton.OffHighlight = new();
				baldiButton.OffHighlight.AddListener(() =>
				{
					baldiButton.image.sprite = baldiButton.HasBeenClicked ? baldiButton.sprNormal[0] : baldiButton.sprClickMe[0];
					if (!baldiButton.IsWaiting)
						baldiButton.transform.localPosition = Vector3.down * 200f;
				});


				baldiButton.OnPress.AddListener(() =>
				{
					if (!baldiButton.CanBeClicked)
						baldiButton.InterruptTutorial();

				
					if (!baldiButton.HasBeenClicked)
					{
						baldiButton.ExplainTutorial();
						return;
					}

					baldiButton.StartWaitSequence();
					StandardMenuButtonPatch.AddOverrideForNextClick((but) =>
					{
						if (but == baldiButton || !baldiButton.ButtonCanBeClicked(but))
						{
							baldiButton.InterruptTutorial();
							return;
						}

						baldiButton.PlayTutorial(but);

					});
				});
			}
		}

		static IEnumerator WaitForAudioPlay(AudioManager source, SoundObject audio, GameObject menuReference)
		{
			yield return null;
			yield return new WaitForSeconds(seconds); // Music manager makes this pain

			source.QueueAudio(audio);

			while (source.AnyAudioIsPlaying)
			{
				if (!menuReference.activeSelf)
					source.FlushQueue(true);
				yield return null;
			}

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

		static BaldiTutorialButton baldiButPre;
	}
}
