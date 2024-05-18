using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BasketballCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects()
		{
			SoundObject[] sbs = [ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "throw.wav")), string.Empty, SoundType.Effect, Color.white),
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "punch.wav")), "BB_Hit", SoundType.Voice, Color.white),
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "bounce.wav")), "BB_Bong", SoundType.Voice, Color.white)];
			sbs[0].subtitle = false;
			return sbs;
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
