using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class LetsDrumCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("drum_lovetodrum.wav", "Vfx_DRUM_Annoyence", SoundType.Effect, new(0.59765625f, 0f, 0.99609375f)),
		GetSound("drum_music.wav", "Vfx_DRUM_Music", SoundType.Voice, new(0.59765625f, 0f, 0.99609375f)),
		GetSound("drum_wannadrum.wav", "Vfx_DRUM_LetsDrum", SoundType.Effect, new(0.59765625f, 0f, 0.99609375f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(55f, "LetsDrum.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var d = (LetsDrum)Npc;
			Destroy(GetComponent<PropagatedAudioManager>()); // Removes the original propagated audio manager

			//d.superLoudMan = d.gameObject.CreateAudioManager(d.GetComponent<AudioSource>(), false, [], true).MakeAudioManagerNonPositional();
			d.superLoudMan = d.gameObject.CreateAudioManager(35, 55)
				
				.MakeAudioManagerNonPositional();
			d.superLoudMan.audioDevice.enabled = true;
			d.superLoudMan.ignoreListenerPause = true;
			d.superLoudMan.audioDevice.ignoreListenerVolume = true;

			//d.musicMan = d.gameObject.CreateAudioManager(true, [soundObjects[1]], 115, 135, true);
			d.musicMan = d.gameObject.CreatePropagatedAudioManager(115, 135)
				.AddStartingAudiosToAudioManager(true, soundObjects[1]);

			d.voiceMan = d.gameObject.CreatePropagatedAudioManager(45, 100);

			d.dat = this;
		}
	}
}
