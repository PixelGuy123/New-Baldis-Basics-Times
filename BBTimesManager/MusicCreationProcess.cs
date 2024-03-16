using BBTimes.CustomComponents;
using BBTimes.ModPatches;
using HarmonyLib;
using MidiPlayerTK;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using System;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void GetMusics()
		{
			// ******************* Sfs ********************
			Directory.GetFiles(Path.Combine(MiscPath, SfsFolder)).Do(sf => MidiPlayerGlobal.MPTK_LoadLiveSF("file://" + Path.GetFullPath(sf))); // One liner aswell

			// ************************ Musics **************************

			foreach(var music in Directory.GetFiles(Path.Combine(MiscPath, AudioFolder, "School")))
			{
				string[] names = Path.GetFileNameWithoutExtension(music).Split('_');
				string fullPath = Path.GetFullPath(music);
				var m = AssetLoader.MidiFromFile(fullPath, names[0]);
				for (int i = 1; i < names.Length; i++)
				{
					switch (names[i])
					{
						case "F1": floorDatas[0].MidiFiles.Add(m); break;
						case "F2": floorDatas[1].MidiFiles.Add(m); break;
						case "F3": floorDatas[2].MidiFiles.Add(m); break;
						case "END": floorDatas[3].MidiFiles.Add(m); break;
						default: break;
					}
				}
				
			}

			// *********** Elevator musics **********

			Directory.GetFiles(Path.Combine(MiscPath, AudioFolder, "Elevator")).Do(audio => 
			ElevatorScreenPatch.elevatorMidis.Add(AssetLoader.MidiFromFile(Path.GetFullPath(audio), Path.GetFileNameWithoutExtension(audio)))); // One liner basically
			

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
