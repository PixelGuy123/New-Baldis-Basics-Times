using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class RottenCheeseCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("cheesePlace.wav", "Vfx_RotCheese_Place", SoundType.Effect, Color.white)];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(3, 2, 35f, "cheese.png");

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var r = GetComponent<ITM_RottenCheese>();
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			renderer.transform.SetParent(transform);
			renderer.name = "RottenCheeseVisual";

			r.entity = gameObject.CreateEntity(2f, 2f, renderer.transform);
			r.entity.SetHeight(1f);
			r.sprs = [.. storedSprites];
			r.renderer = renderer;
			r.audPut = soundObjects[0];
			r.audMan = gameObject.CreatePropagatedAudioManager();
		}
	}
}
