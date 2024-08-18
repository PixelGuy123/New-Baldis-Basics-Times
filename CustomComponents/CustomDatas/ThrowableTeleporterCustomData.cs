﻿using BBTimes.CustomContent.CustomItems;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class ThrowableTeleporterCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("throw.wav", SoundType.Effect), BBTimesManager.man.Get<SoundObject>("teleportAud")];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(25f, "telep.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var itm = GetComponent<ITM_ThrowableTeleporter>();
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			renderer.transform.SetParent(transform);
			renderer.name = "ThrowableTeleporterVisual";

			itm.audMan = gameObject.CreatePropagatedAudioManager(85f, 115f);
			itm.audThrow = soundObjects[0];
			itm.audTeleport = soundObjects[1];

			itm.entity = gameObject.CreateEntity(2f, 2f, renderer.transform);
		}
	}
}
