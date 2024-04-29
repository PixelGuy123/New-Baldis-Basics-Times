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
	public class BananaCustomData : CustomItemData
	{
		public override void SetupPrefab()
		{
			
			base.SetupPrefab();
			var rendererBase = ObjectCreationExtension.CreateSpriteBillboard(storedSprites[0], -4f, false);
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.SetActive(true);

			var comp = GetComponent<ITM_Banana>();
			gameObject.layer = LayerStorage.standardEntities;
			comp.entity = gameObject.CreateEntity(1f, 2f, rendererBase.transform, [comp]);

			comp.audMan = gameObject.CreatePropagatedAudioManager(40, 65).SetAudioManagerAsPrefab();
			comp.aud_slip = soundObjects[0];

			comp.rendererBase = rendererBase.transform;
		}

		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "banana_slip.wav")), "Vfx_BN_slip", SoundType.Voice, Color.yellow)];
		
	}
}
