using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ThrowableTeleporterCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("throw.wav", SoundType.Effect)];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(25f, "telep.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var itm = GetComponent<ITM_ThrowableTeleporter>();
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			renderer.transform.SetParent(transform);
			renderer.name = "ThrowableTeleporterVisual";

			itm.audMan = gameObject.CreatePropagatedAudioManager(55f, 75f);
			itm.audThrow = soundObjects[0];
			itm.audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");

			itm.entity = gameObject.CreateEntity(2f, 2f, renderer.transform);
		}
	}
}
