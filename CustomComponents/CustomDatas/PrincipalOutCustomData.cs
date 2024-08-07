﻿using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PrincipalOutCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject sd = GetSound("baldi_walking.wav", "Event_PriOut0", SoundType.Effect, Color.green);
			sd.additionalKeys = [
				new() {time = 1.623f, key = "Event_PriOut1"},
				new() {time = 5.399f, key = "Event_PriOut2"},
				new() {time = 7.656f, key = "Event_PriOut3"},
				new() {time = 8.795f, key = "Event_PriOut4"},
				new() {time = 11.476f, key = "Event_PriOut5"}
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
