using BBTimes.CustomComponents;
using HarmonyLib;
using MidiPlayerTK;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void GetMusics()
		{
			// ******************* Sfs ********************
			MidiPlayerGlobal.MPTK_LoadLiveSF("file://" + Path.Combine(MiscPath, SfsFolder, "GS_Wavetable_Synth.sf2"));

			// ************************ Musics **************************

			string midi = AssetLoader.MidiFromFile(Path.Combine(MiscPath, AudioFolder, "mus_NewSchool.midi"), "newSchoolMusic1");
			floorDatas[0].MidiFiles.Add(midi);
			midi = AssetLoader.MidiFromFile(Path.Combine(MiscPath, AudioFolder, "mus_NewSchool1.mid"), "newSchoolMusic2");
			floorDatas[0].MidiFiles.Add(midi);
			floorDatas[3].MidiFiles.Add(midi);
			midi = AssetLoader.MidiFromFile(Path.Combine(MiscPath, AudioFolder, "mus_NewSchool2.mid"), "newSchoolMusic3");
			floorDatas[2].MidiFiles.Add(midi);
			midi = AssetLoader.MidiFromFile(Path.Combine(MiscPath, AudioFolder, "mus_NewSchool3.mid"), "newSchoolMusic4");
			floorDatas[2].MidiFiles.Add(midi);
			midi = AssetLoader.MidiFromFile(Path.Combine(MiscPath, AudioFolder, "mus_NewSchool4.mid"), "newSchoolMusic5");
			floorDatas[3].MidiFiles.Add(midi);
			floorDatas[1].MidiFiles.Add(midi);
			midi = AssetLoader.MidiFromFile(Path.Combine(MiscPath, AudioFolder, "mus_NewSchool5.mid"), "newSchoolMusic6");
			floorDatas[1].MidiFiles.Add(midi);



			//  ************************ Base Game Manager changes ******************************
			var fieldInfo = AccessTools.Field(typeof(MainGameManager), "allNotebooksNotification");

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

			foreach (var man in Resources.FindObjectsOfTypeAll<MainGameManager>()) // Main game manager
			{
				fieldInfo.SetValue(man, man.name == "Lvl3_MainGameManager 1" ? soundCRAZY : sound);
				var comp = man.GetComponent<MainGameManagerExtraComponent>();
				if (man.name.StartsWith("Lvl1"))
				{
					fieldInfo.SetValue(man, sound);
					comp.midis = [.. floorDatas[0].MidiFiles];
					continue;
				}
				if (man.name.StartsWith("Lvl2"))
				{
					fieldInfo.SetValue(man, sound);
					comp.midis = [.. floorDatas[1].MidiFiles];
					continue;
				}
				if (man.name.StartsWith("Lvl3"))
				{
					fieldInfo.SetValue(man, soundCRAZY);
					comp.midis = [.. floorDatas[2].MidiFiles];
					continue;
				}
			}
		}
	}
}
