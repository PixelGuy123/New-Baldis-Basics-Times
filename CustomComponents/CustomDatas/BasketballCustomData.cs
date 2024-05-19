using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BasketballCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
		[GetSoundNoSub("throw.wav", SoundType.Effect), 
		GetSound("punch.wav", "BB_Hit", SoundType.Voice, Color.white),
		GetSound("bounce.wav", "BB_Bong", SoundType.Voice, Color.white)];

		protected override Sprite[] GenerateSpriteOrder()
		{
			Sprite[] sprites = new Sprite[5];
			for (int i = 0; i < sprites.Length; i++)
				GetSprite(25f, $"basketball{i}.png");
			return sprites;
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.gameObject.SetActive(true);

			var comp = GetComponent<ITM_Basketball>();
			gameObject.layer = LayerStorage.standardEntities;
			comp.entity = gameObject.CreateEntity(2f, 2f, rendererBase.transform, [comp]);

			comp.audMan = gameObject.CreatePropagatedAudioManager(75, 105);
			comp.audThrow = soundObjects[0];
			comp.audHit = soundObjects[1];
			comp.audBong = soundObjects[2];
			comp.spriteAnim = storedSprites;

			comp.renderer = rendererBase.GetComponent<SpriteRenderer>();
		}
	}
}
