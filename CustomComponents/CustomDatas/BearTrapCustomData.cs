using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BearTrapCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("trap_catch.wav", "Vfx_BT_catch", SoundType.Voice, Color.white)];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(50f, "TrapClose.png"), GetSprite(50f, "TrapOpen.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var trap = GetComponent<ITM_Beartrap>();
			trap.audMan = gameObject.CreatePropagatedAudioManager(75f, 105f);
			trap.audTrap = soundObjects[0];
			trap.closedTrap = storedSprites[0];

			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1]).AddSpriteHolder(-4f);
			var rendererBase = renderer.transform.parent;
			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;

			trap.renderer = renderer;
			trap.entity = gameObject.CreateEntity(1f, 1f, rendererBase, [trap]);
		}
	}
}
