using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class MagnetCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("throw.wav", SoundType.Effect)];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(2, 2, 35f, "magnet.png");

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var itm = GetComponent<ITM_Magnet>();
			itm.renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			itm.renderer.transform.SetParent(transform);
			itm.renderer.name = "MagnetVisual";
			itm.sprs = [.. storedSprites];

			itm.audMan = gameObject.CreatePropagatedAudioManager(75f, 100f);
			itm.audThrow = soundObjects[0];

			itm.entity = gameObject.CreateEntity(2f, 12f, itm.renderer.transform);
		}
	}
}
