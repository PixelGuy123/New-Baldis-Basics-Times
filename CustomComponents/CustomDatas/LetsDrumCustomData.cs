using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class LetsDrumCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "drum_lovetodrum.wav")), "Vfx_DRUM_Annoyence", SoundType.Effect, new(0.59765625f, 0f, 0.99609375f)),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "drum_music.wav")), "Vfx_DRUM_Music", SoundType.Voice, new(0.59765625f, 0f, 0.99609375f)),
		ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "drum_wannadrum.wav")), "Vfx_DRUM_LetsDrum", SoundType.Effect, new(0.59765625f, 0f, 0.99609375f))];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var d = GetComponent<LetsDrum>();
			Destroy(GetComponent<PropagatedAudioManager>()); // Removes the original propagated audio manager

			d.superLoudMan = d.gameObject.CreateAudioManager(d.GetComponent<AudioSource>(), false, [], true);
			d.GetComponent<AudioSource>().enabled = true;
			d.superLoudMan.MakeAudioManagerNonPositional();
			d.superLoudMan.ignoreListenerPause = true;
			d.superLoudMan.audioDevice.ignoreListenerVolume = true;

			d.musicMan = d.gameObject.CreateAudioManager(true, [soundObjects[1]], 115, 135, true);

			d.voiceMan = d.gameObject.CreateAudioManager(45, 100, true);

			d.dat = this;
		}
	}
}
