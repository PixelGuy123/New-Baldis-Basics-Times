using BBTimes.CustomContent.Events;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BlackOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("blackout_on.wav", SoundType.Music),
			GetSoundNoSub("blackout_out.wav", SoundType.Music)];

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
