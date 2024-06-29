using BBTimes.ModPatches;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using PixelInternalAPI.Extensions;
using BBTimes.CustomComponents;
using System.Reflection;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void GetMusics()
		{
			//  ************************ Base Game Manager changes ******************************

			var sound = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AllNotebooksNormal.wav")), "Vfx_BAL_CongratsNormal_0", SoundType.Effect, Color.green);
			sound.additionalKeys = [
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_1", time = 2.103f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_2", time = 4.899f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_3", time = 8.174f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_4", time = 12.817f} // Tip: use audacity to know the audio length
			];

			var soundCRAZY = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AllNotebooksFinal.wav")), "Vfx_BAL_CongratsNormal_0", SoundType.Effect, Color.green);
			soundCRAZY.additionalKeys = [
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_1", time = 2.051f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_2", time = 4.842f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsAngry_0", time = 7.185f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_4", time = 12.694f} // Tip: use audacity to know the audio length
			];
			// Update the all notebooks notification
			GenericExtensions.FindResourceObjects<MainGameManager>().Do(man => man.allNotebooksNotification = man.name.StartsWith("Lvl3") ? soundCRAZY : sound);

			// Level Final Mode
			AudioMixerGroup group = GenericExtensions.FindResourceObjectByName<AudioMixerGroup>("Effects");
			var sObj = ScriptableObject.CreateInstance<LoopingSoundObject>();
			sObj.clips = [AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "Quiet_noise_loop.wav"))];
			sObj.mixer = group;
			MainGameManagerPatches.chaos0 = sObj;

			sObj = ScriptableObject.CreateInstance<LoopingSoundObject>();
			sObj.clips = [AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "Chaos_EarlyLoopStart.wav")), AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "Chaos_EarlyLoop.wav"))];
			sObj.mixer = group;
			MainGameManagerPatches.chaos1 = sObj;

			sObj = ScriptableObject.CreateInstance<LoopingSoundObject>();
			sObj.clips = [AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "Chaos_FinalLoop.wav")), AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "Chaos_FinalLoopNoise.wav"))];
			sObj.mixer = group;
			MainGameManagerPatches.chaos2 = sObj;

			MainGameManagerPatches.angryBal = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AngryGetOut.wav")), "Vfx_BAL_ANGRY_0", SoundType.Voice, Color.green);
			MainGameManagerPatches.angryBal.additionalKeys = 
				[new() { key = "Vfx_BAL_ANGRY_1", time = 0.358f},
				new() { key = "Vfx_BAL_ANGRY_2", time = 0.681f }, 
				new() { key = "Vfx_BAL_ANGRY_3", time = 0.934f }, 
				new() { key = "Vfx_BAL_ANGRY_4", time = 1.113f },
				new() { key = "Vfx_BAL_ANGRY_5", time = 1.738f }
				];


		}
	}
}
