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
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_2", time = 3.105f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_3", time = 6.869f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_4", time = 11.098f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_5", time = 13.269f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_6", time = 15.173f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_7", time = 18.761f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_8", time = 20.949f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_9", time = 24.425f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_10", time = 27.174f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_11", time = 29.48f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_12", time = 32.525f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_13", time = 35.71f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_14", time = 39.329f },
				new() { key = "Vfx_BAL_MenuExplain_EndlessMode_15", time = 42.642f }
				);

			AddButtonTut("Free", "BAL_Explains_ExplorerMode.wav", "Vfx_BAL_MenuExplain_ExplorerMode_1",
				new() { key = "Vfx_BAL_MenuExplain_ExplorerMode_2", time = 2.615f },
				new() { key = "Vfx_BAL_MenuExplain_ExplorerMode_3", time = 6.206f },
				new() { key = "Vfx_BAL_MenuExplain_ExplorerMode_4", time = 8.56f },
				new() { key = "Vfx_BAL_MenuExplain_ExplorerMode_5", time = 15.786f },
				new() { key = "Vfx_BAL_MenuExplain_ExplorerMode_6", time = 19.607f },
				new() { key = "Vfx_BAL_MenuExplain_ExplorerMode_7", time = 24.669f }
				);

			AddButtonTut("Challenge", "BAL_Explains_ChallengeMode.wav", "Vfx_BAL_MenuExplain_ChallengeMode_1",
				new() { key = "Vfx_BAL_MenuExplain_ChallengeMode_2", time = 2.904f },
				new() { key = "Vfx_BAL_MenuExplain_ChallengeMode_3", time = 6f },
				new() { key = "Vfx_BAL_MenuExplain_ChallengeMode_4", time = 7.096f },
				new() { key = "Vfx_BAL_MenuExplain_ChallengeMode_5", time = 10.517f },
				new() { key = "Vfx_BAL_MenuExplain_ChallengeMode_6", time = 14.246f },
				new() { key = "Vfx_BAL_MenuExplain_ChallengeMode_7", time = 16.133f }
				);

			AddButtonTutNoise("MainContinue", // Both MainContinue and MainNew lol
				AddButtonTut("MainNew", "BAL_Explains_HideAndSeek.wav", "Vfx_BAL_MenuExplain_HideAndSeek_1",
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_2", time = 2.417f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_3", time = 4.957f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_4", time = 7.186f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_5", time = 10.446f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_6", time = 13.822f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_7", time = 16.379f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_8", time = 18.071f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_9", time = 20.06f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_10", time = 24.712f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_11", time = 26.728f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_12", time = 29.537f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_13", time = 32.357f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_14", time = 37.196f },
				new() { key = "Vfx_BAL_MenuExplain_HideAndSeek_15", time = 40.452f }
				));

			AddButtonTut("FieldTrips", "BAL_Explains_FieldTrip.wav", "Vfx_BAL_MenuExplain_FieldTrip_1",
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_2", time = 2.804f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_3", time = 9.013f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_4", time = 12.62f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_5", time = 16.312f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_6", time = 20.228f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_7", time = 22.87f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_8", time = 26.772f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_9", time = 29.68f },
				new() { key = "Vfx_BAL_MenuExplain_FieldTrip_10", time = 31.384f }
				);

			SoundObject AddButtonTut(string name, string audioName, string sub, params SubtitleTimedKey[] keys)
			{
				var sndobj = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(
				Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BaldiBut", BBTimesManager.GetAssetName(audioName))), sub, SoundType.Effect, Color.green);

				AddButtonTutNoise(name, sndobj);
				sndobj.additionalKeys = keys;
				return sndobj;
			}

			void AddButtonTutNoise(string name, SoundObject sound) =>
				BaldiTutorialButton.AddButtonTutorial(mainModeBut.transform.Find(name).GetComponent<StandardMenuButton>(), sound);

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
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_2", time = 1.565f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_3", time = 5.268f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_4", time = 7.929f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_5", time = 12.855f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_6", time = 17.79f },
				new() { key = "Vfx_BAL_MenuExplain_Tutorial_7", time = 21.775f }
				];

			baldiButton.audDontKnowButton = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(
				Path.Combine(BBTimesManager.MiscPath, BBTimesManager.AudioFolder, "BaldiBut", BBTimesManager.GetAssetName("BAL_Explains_HowWeirdSelections.wav"))), "Vfx_BAL_MenuExplain_HowWeird_1", SoundType.Effect, Color.green);
			baldiButton.audDontKnowButton.additionalKeys = [
				new() { key = "Vfx_BAL_MenuExplain_HowWeird_2", time = 1.818f },
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
