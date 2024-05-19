﻿using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.CustomContent.CustomItems;
using PixelInternalAPI.Classes;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
namespace BBTimes.CustomComponents.CustomDatas
{
	public class BellCustomData : CustomItemData
	{

		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("bell_bellnoise.wav", "Vfx_BEL_Ring", SoundType.Voice, Color.white)];

		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(25f, "bellActive.png"), GetSprite(25f, "bellDeactive.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]).AddSpriteHolder(-4f);
			var rendererBase = renderer.transform.parent;
			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;

			var comp = GetComponent<ITM_Bell>();
			gameObject.layer = LayerStorage.standardEntities;
			comp.entity = gameObject.CreateEntity(1.5f, 2.5f, rendererBase, [comp]);

			comp.audMan = gameObject.CreatePropagatedAudioManager(165, 200);
			comp.audBell = soundObjects[0];

			comp.renderer = renderer;
			comp.deactiveSprite = storedSprites[1];
		}
		
	}
}
