using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.CustomContent.Events;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using UnityEngine;
using MTM101BaldAPI;


namespace BBTimes.CustomComponents.CustomDatas
{
	public class SuperFansCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sds = [GetSound("SuperFans.wav", "Event_SuperFans1", SoundType.Effect, Color.green)];
			sds[0].additionalKeys = [new() { time = 7.073f, key = "Event_SuperFans2" }];

			return sds;
		}
		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(3, 3, 75f, "fan.png");

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var ev = GetComponent<SuperFans>();
			ev.eventIntro = soundObjects[0];
			Cumulo cloud = (Cumulo)NPCMetaStorage.Instance.Get(Character.Cumulo).value;

			var superFanRend = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0], false);
			superFanRend.gameObject.ConvertToPrefab(true);
			superFanRend.name = "Superfan";
			var superFan = superFanRend.gameObject.AddComponent<SuperFan>();

			superFan.audBlow = cloud.audBlowing;

			superFan.renderer = superFanRend;
			superFan.sprites = [.. storedSprites];
			superFan.windManager = Instantiate(cloud.windManager);
			superFan.windManager.transform.SetParent(superFan.transform);
			superFan.windGraphicsParent = superFan.windManager.transform.Find("WindGraphicsParent");
			superFan.audMan = superFan.windManager.GetComponentInChildren<AudioManager>();
			superFan.windGraphics = superFan.windGraphicsParent.GetComponentsInChildren<MeshRenderer>();

			ev.superFanPre = superFan;
		}
	}
}
