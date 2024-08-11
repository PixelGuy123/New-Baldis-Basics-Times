using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.CustomContent.Events;
using PixelInternalAPI.Extensions;
using UnityEngine;
using MTM101BaldAPI;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class SkateboardDayEventCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			var sd = GetSound("SkateboardDay.wav", "Event_SkateboardDay1", SoundType.Effect, Color.green);
			sd.additionalKeys = [new() { time = 2.083f, key = "Event_SkateboardDay2" }, new() { time = 5.738f, key = "Event_SkateboardDay3" }];

			return [sd];
		}
		protected override Sprite[] GenerateSpriteOrder()
		{
			return base.GenerateSpriteOrder();
		}
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var ev = GetComponent<SkateboardDayEvent>();
			ev.eventIntro = soundObjects[0];

			var skRender = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(0f);
			skRender.transform.parent.gameObject.ConvertToPrefab(true);

			var sk = skRender.transform.parent.gameObject.AddComponent<Skateboard>();
			sk.entity = sk.gameObject.CreateEntity(2f, 1f, skRender.transform);
			sk.name = "Skateboard";

			ev.skatePre = sk;

		}
	}
}
