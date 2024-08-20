using BBTimes.CustomContent.Events;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BlackOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			var sd = GetSound("baldi_ele.wav", "Event_BlackOut0", SoundType.Effect, UnityEngine.Color.green);
			sd.additionalKeys = [
				new() {time = 1.832f, key = "Event_BlackOut1"},
				new() {time = 4.88f, key = "Event_BlackOut2"},
				new() {time = 8.849f, key = "Event_BlackOut3"},
				new() {time = 12.788f, key = "Event_BlackOut4"}
				];
			return [GetSoundNoSub("blackout_on.wav", SoundType.Music),
			GetSoundNoSub("blackout_out.wav", SoundType.Music), sd];
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var blackout = GetComponent<BlackOut>();
			blackout.eventIntro = soundObjects[2];
			blackout.audMan = gameObject.CreateAudioManager(85, 105)
				
				.MakeAudioManagerNonPositional();
			blackout.audOff = soundObjects[1];
			blackout.audOn = soundObjects[0];
		}
	}
}
