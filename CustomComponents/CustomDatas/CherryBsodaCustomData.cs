using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CherryBsodaCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("hit.wav", SoundType.Voice)];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var nbsoda = GetComponent<ITM_BSODA>();
			var bsoda = gameObject.AddComponent<ITM_CherryBsoda>();

			bsoda.spriteRenderer = nbsoda.spriteRenderer;
			bsoda.sound = nbsoda.sound;
			bsoda.entity = nbsoda.entity;
			bsoda.time = nbsoda.time;
			bsoda.moveMod = nbsoda.moveMod;
			bsoda.audMan = gameObject.CreatePropagatedAudioManager(65, 125);
			bsoda.audHit = soundObjects[0];
			
			bsoda.spriteRenderer.sprite = GetSprite(bsoda.spriteRenderer.sprite.pixelsPerUnit, "spray.png");
			bsoda.speed = 45f;
			bsoda.entity.collisionLayerMask = LayerStorage.entityCollisionMask; // Default entity collision mask
			Destroy(bsoda.transform.Find("RendereBase").Find("Particles").gameObject);

			Destroy(nbsoda);
		}
	}
}
