using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.CustomContent.Events;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CurtainsClosedEventCustomData : CustomEventData
	{
		protected override SoundObject[] GenerateSoundObjects() {
			var sd = GetSound("baldi_curtains.wav", "Event_CurtClosed0", SoundType.Effect, Color.green);
			sd.additionalKeys = [new() { time = 0.862f, key = "Event_CurtClosed1" }, new() { time = 2.532f, key = "Event_CurtClosed2" }];
			return [GetSound("curtainClose.wav", "Vfx_Curtain_Slide", SoundType.Voice, Color.white),
		GetSound("curtainOpen.wav", "Vfx_Curtain_Slide", SoundType.Voice, Color.white), sd];
			}

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(pixs, "curtainClosed.png"), GetSprite(pixs, "curtainOpen.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var ev = GetComponent<CurtainsClosedEvent>();
			ev.eventIntro = soundObjects[2];

			var curt = new GameObject("Curtain").AddComponent<Curtains>();
			curt.gameObject.ConvertToPrefab(true);
			curt.audClose = soundObjects[0];
			curt.audOpen = soundObjects[1];
			curt.sprClosed = storedSprites[0];
			curt.sprOpen = storedSprites[1];
			curt.audMan = curt.gameObject.CreatePropagatedAudioManager(65, 85);
			curt.collider = curt.gameObject.AddBoxCollider(Vector3.zero, new(5f, 10f, 0.7f), false);
			curt.collider.enabled = false;

			curt.renderers = [CurtFace(0.01f), CurtFace(-0.01f)];
			
			SpriteRenderer CurtFace(float offset)
			{
				var curtFace = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1], false);
				curtFace.transform.SetParent(curt.transform);
				curtFace.transform.localPosition = Vector3.forward * offset + Vector3.up * 5f;
				return curtFace;
			}

			ev.curtPre = curt;
		}

		const float pixs = 35f;
	}
}
