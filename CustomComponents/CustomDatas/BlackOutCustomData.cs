using BBTimes.CustomContent.Events;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BlackOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "blackout_on.wav")), string.Empty, SoundType.Music, Color.white),
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "blackout_out.wav")), string.Empty, SoundType.Music, Color.white)];
			sds[0].subtitle = false;
			sds[1].subtitle = false;
			return sds;
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var blackout = GetComponent<BlackOut>();
			blackout.audMan = gameObject.CreateAudioManager(85, 105)
				
				.MakeAudioManagerNonPositional();
			blackout.audOff = soundObjects[1];
			blackout.audOn = soundObjects[0];
		}
	}
}
