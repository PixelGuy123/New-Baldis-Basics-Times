using BBTimes.CustomContent.CustomItems;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BasketballCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
		[GetSoundNoSub("throw.wav", SoundType.Effect),
		BBTimesManager.man.Get<SoundObject>("audGenericPunch"),
		GetSound("bounce.wav", "BB_Bong", SoundType.Voice, Color.white),
		BBTimesManager.man.Get<SoundObject>("audPop")];

		protected override Sprite[] GenerateSpriteOrder() =>
			BBTimesManager.man.Get<Sprite[]>("basketBall");
		

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.gameObject.SetActive(true);

			var comp = GetComponent<ITM_Basketball>();
			gameObject.layer = LayerStorage.standardEntities;
			comp.entity = gameObject.CreateEntity(2f, 2f, rendererBase.transform);

			comp.audMan = gameObject.CreatePropagatedAudioManager(75, 105);
			comp.audThrow = soundObjects[0];
			comp.audHit = soundObjects[1];
			comp.audBong = soundObjects[2];
			comp.audPop = soundObjects[3];
			comp.spriteAnim = [.. storedSprites];

			comp.renderer = rendererBase;
		}
	}
}
