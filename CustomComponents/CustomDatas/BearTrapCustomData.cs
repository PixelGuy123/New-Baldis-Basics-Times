using BBTimes.CustomContent.CustomItems;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BearTrapCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[BBTimesManager.man.Get<SoundObject>("BeartrapCatch")];

		protected override Sprite[] GenerateSpriteOrder() =>
			BBTimesManager.man.Get<Sprite[]>("Beartrap");

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
			trap.entity = gameObject.CreateEntity(1f, 1f, rendererBase);
		}
	}
}
