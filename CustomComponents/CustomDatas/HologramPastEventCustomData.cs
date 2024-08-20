using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.CustomContent.Events;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class HologramPastEventCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [GetSound("hologramEv.wav", "Event_BlackOut0", SoundType.Effect, Color.green)];
			sds[0].additionalKeys = [new() { time = 2f, key = "Event_PastHolograms1" }, 
				new() { time = 4.507f, key = "Event_PastHolograms2" }, 
				new() { time = 6.806f, key = "Event_PastHolograms3" },
				new() { time = 9.897f, key = "Event_PastHolograms4" },
				new() { time = 17.007f, key = "Event_PastHolograms5" }
				];

			return sds;
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var ev = GetComponent<HologramPastEvent>();
			ev.eventIntro = soundObjects[0];

			var rend = ObjectCreationExtensions.CreateSpriteBillboard(null);
			rend.name = "HologramRenderer";
			rend.gameObject.ConvertToPrefab(true);

			ev.hologramPre = rend.gameObject.AddComponent<Hologram>();
			ev.hologramPre.renderer = rend;
		}
	}
}
