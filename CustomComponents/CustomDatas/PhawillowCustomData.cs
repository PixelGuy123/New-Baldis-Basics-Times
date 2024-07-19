using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PhawillowCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("breathing.wav", "Vfx_Phawillow_Wandering", SoundType.Voice, new(0.84705f, 0.84705f, 0.84705f)),
			GetSound("Phawillow_Laughing.wav", "Vfx_Phawillow_Laught", SoundType.Voice, new(0.84705f, 0.84705f, 0.84705f)),
			GetSound("Phawillow_Restarting.wav", "Vfx_Phawillow_Restart", SoundType.Voice, new(0.84705f, 0.84705f, 0.84705f))];

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(3, 1, 22f, "phawillowSpritesheet.png");
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var wi = (Phawillow)Npc;
			wi.audMan = GetComponent<PropagatedAudioManager>();
			wi.audWander = soundObjects[0];
			wi.audLaugh = soundObjects[1];
			wi.audRestart = soundObjects[2];
			wi.gameObject.layer = LayerStorage.iClickableLayer;
			wi.floatingRenderer = wi.spriteRenderer[0];

			var itemHolder = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(new Vector3(3f, -0.8f, 0f), 0);
			itemHolder.transform.parent.SetParent(wi.transform);
			itemHolder.transform.parent.localPosition = Vector3.zero;
			itemHolder.transform.parent.gameObject.AddComponent<BillboardRotator>();

			wi.itemRender = itemHolder;
			wi.itemRenderHolder = itemHolder.transform.parent;
			wi.sprNormal = storedSprites[0];
			wi.sprSplashed = storedSprites[1];
			wi.sprActive = storedSprites[2];
		}
	}
}
