using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PrincipalOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject sd = GetSound("baldi_walking.wav", "Event_PriOut0", SoundType.Effect, Color.green);
			sd.additionalKeys = [
				new() {time = 1.154f, key = "Event_PriOut1"},
				new() {time = 5.304f, key = "Event_PriOut2"},
				new() {time = 6.498f, key = "Event_PriOut3"}
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
