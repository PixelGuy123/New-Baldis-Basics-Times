using BBTimes.ModPatches;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using PixelInternalAPI.Extensions;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void GetMusics()
		{
			//  ************************ Base Game Manager changes ******************************

			var sound = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AllNotebooksNormal.wav")), "Vfx_BAL_CongratsNormal_0", SoundType.Effect, Color.green);
			sound.additionalKeys = [
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_1", time = 2.17f},
				new SubtitleTimedKey() { key = "Vfx_BAL_AllNotebooks_3", time = 4.89f},
				new SubtitleTimedKey() { key = "Vfx_BAL_AllNotebooks_4", time = 8.201f},
				new SubtitleTimedKey() { key = ".", time = 11.337f},
				new SubtitleTimedKey() { key = "..", time = 12.78f},
				new SubtitleTimedKey() { key = "...", time = 14.061f},
				new SubtitleTimedKey() { key = "Vfx_BAL_AllNotebooks_5", time = 14.602f} // Tip: use audacity to know the audio length
			];

			var soundCRAZY = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(MiscPath, AudioFolder, "BAL_AllNotebooksFinal.wav")), "Vfx_BAL_CongratsNormal_0", SoundType.Effect, Color.green);
			soundCRAZY.additionalKeys = [
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsNormal_1", time = 2.17f},
				new SubtitleTimedKey() { key = "Vfx_BAL_AllNotebooks_3", time = 4.89f},
				new SubtitleTimedKey() { key = "Vfx_BAL_CongratsAngry_0", time = 7.233f},
				new SubtitleTimedKey() { key = ".", time = 12.653f},
				new SubtitleTimedKey() { key = "..", time = 13.5f},
				new SubtitleTimedKey() { key = "...", time = 14.302f},
				new SubtitleTimedKey() { key = "Vfx_BAL_AllNotebooks_5", time = 14.382f} // Tip: use audacity to know the audio length
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
