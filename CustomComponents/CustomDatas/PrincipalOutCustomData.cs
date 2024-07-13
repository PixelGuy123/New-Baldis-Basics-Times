using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PrincipalOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject sd = GetSound("baldi_walking.wav", "Event_PriOut0", SoundType.Effect, Color.green);
			sd.additionalKeys = [
				new() {time = 0.72f, key = "Event_PriOut1"},
				new() {time = 4.66f, key = "Event_PriOut2"}
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
