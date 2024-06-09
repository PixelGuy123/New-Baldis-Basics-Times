using BBTimes.CustomContent.Builders;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.CustomContent.Objects;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CameraBuilderCustomData : CustomObjectPrefabData
	{
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(25f, "SecurityCamera.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var cam = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]).AddSpriteHolder(9f, 0);
			cam.name = "Sprite";
			var camHolder = cam.transform.parent;
			camHolder.name = "SecurityCamera";
			camHolder.gameObject.ConvertToPrefab(true);

			var camComp = camHolder.gameObject.AddComponent<SecurityCamera>();
			camComp.collider = camHolder.gameObject.AddBoxCollider(new(0f, 1f, 5f), new(3f, 10f, 3f), true);

			var visionIndicator = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(TextureExtensions.CreateSolidTexture(128, 128, new(1f, 1f, 1f, 0.3f)), 15f), false);
			visionIndicator.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			visionIndicator.transform.localScale = new(1f, 1.172f, 1f);
			visionIndicator.name = "CameraVisionIndicator";
			visionIndicator.gameObject.ConvertToPrefab(true);

			camComp.visionIndicatorPre = visionIndicator;

			GetComponent<CameraBuilder>().camPre = camHolder;
		}
	}
}
