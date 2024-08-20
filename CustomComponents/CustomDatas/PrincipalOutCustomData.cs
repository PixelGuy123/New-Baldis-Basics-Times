using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PrincipalOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject sd = GetSound("baldi_walking.wav", "Event_PriOut0", SoundType.Effect, Color.green);
			sd.additionalKeys = [
				new() {time = 1.4f, key = "Event_PriOut1"},
				new() {time = 4.834f, key = "Event_PriOut2"},
				new() {time = 7.129f, key = "Event_PriOut3"},
				new() {time = 8.245f, key = "Event_PriOut4"},
				new() {time = 11.074f, key = "Event_PriOut5"}
				];

			return [sd];
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			GetComponent<RandomEvent>().eventIntro = soundObjects[0];
		}
	}
}
