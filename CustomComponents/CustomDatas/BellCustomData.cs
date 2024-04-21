using BBTimes.Extensions.ObjectCreationExtensions;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Classes;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.IO;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BellCustomData : CustomItemData
	{

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var rendererBase = ObjectCreationExtension.CreateSpriteBillboard(storedSprites[0], -4f, false).GetComponent<RendererContainer>();
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.gameObject.SetActive(true);

			var comp = GetComponent<ITM_Bell>();
			gameObject.layer = LayerStorage.standardEntities;
			comp.entity = gameObject.CreateEntity(1.5f, 2.5f, rendererBase.transform, [comp]);

			comp.audMan = gameObject.CreateAudioManager(165, 200, true);
			comp.audBell = soundObjects[0];

			comp.renderer = (SpriteRenderer)rendererBase.renderers[0];
			comp.deactiveSprite = storedSprites[1];
		}

		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "bell_bellnoise.wav")), "Vfx_BEL_Ring", SoundType.Voice, Color.white)];
		
	}
}
