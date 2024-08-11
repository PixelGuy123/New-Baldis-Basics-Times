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
			sd.additionalKeys = [new() { time = 2.083f, key = "Event_SkateboardDay2" }, new() { time = 5.377f, key = "Event_SkateboardDay3" }];

			return [sd, GetSound("skateNoises.wav", "Vfx_Ska_Noise", SoundType.Voice, Color.white)];
		}
		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(4, 2, 15f, "skate.png");
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var ev = GetComponent<SkateboardDayEvent>();
			ev.eventIntro = soundObjects[0];

			var skRender = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(0f);
			skRender.transform.parent.gameObject.ConvertToPrefab(true);
			skRender.CreateAnimatedSpriteRotator(new SpriteRotationMap() { angleCount = 8, spriteSheet = [.. storedSprites] });

			var sk = skRender.transform.parent.gameObject.AddComponent<Skateboard>();
			sk.entity = sk.gameObject.CreateEntity(2f, 1f, skRender.transform);
			sk.name = "Skateboard";
			sk.audMan = sk.gameObject.CreatePropagatedAudioManager(45f, 65f);
			sk.audRoll = soundObjects[1];

			ev.skatePre = sk;

		}
	}
}
