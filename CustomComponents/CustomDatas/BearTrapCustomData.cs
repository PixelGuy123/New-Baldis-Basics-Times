using BBTimes.CustomContent.CustomItems;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BearTrapCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "trap_catch.wav")), "Vfx_BT_catch", SoundType.Voice, Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var trap = GetComponent<ITM_Beartrap>();
			trap.audMan = gameObject.CreateAudioManager(75f, 105f, true);
			trap.audTrap = soundObjects[0];
			trap.closedTrap = storedSprites[0];

			var rendererBase = ObjectCreationExtension.CreateSpriteBillboard(storedSprites[1], -4f, false);
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.SetActive(true);

			trap.renderer = (SpriteRenderer)rendererBase.GetComponent<RendererContainer>().renderers[0];
			trap.entity = gameObject.CreateEntity(1f, 1f, rendererBase.transform, [trap]);
		}
	}
}
