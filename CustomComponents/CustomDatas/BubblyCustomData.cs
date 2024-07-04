using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class BubblyCustomData : CustomNPCData
	{
		protected override Sprite[] GenerateSpriteOrder()
		{
			Sprite[] anim = new Sprite[10];
			for (int i = 0; i < 8; i++)
				anim[i] = GetSprite(pixs, $"Bubbly{i + 1}.png");
			anim[8] = GetSprite(pixs, "Bubblyactive.png");
			anim[9] = GetSprite(16f, "bubble.png");
			return anim;
		}
		protected override SoundObject[] GenerateSoundObjects() =>
			[BBTimesManager.man.Get<SoundObject>("audPop"), GetSound("Bubbly_BubbleSpawn.mp3", "Vfx_Bubbly_Fillup", SoundType.Voice, new(0.6f, 0.6f, 0f))];
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var bub = (Bubbly)Npc;
			bub.audMan = GetComponent<PropagatedAudioManager>();
			bub.sprWalkingAnim = [.. storedSprites.Take(7)];
			bub.sprPrepareBub = storedSprites[8];
			bub.renderer = bub.spriteRenderer[0];
			bub.audFillUp = soundObjects[1];

			var bubble = new GameObject("Bubble").AddComponent<Bubble>();
			bubble.gameObject.ConvertToPrefab(true);
			bubble.audPop = soundObjects[0];
			bubble.audMan = bubble.gameObject.CreatePropagatedAudioManager(85, 105);

			var visual = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[9]).AddSpriteHolder(0f, 0);
			visual.transform.parent.SetParent(bubble.transform);
			visual.transform.parent.localPosition = Vector3.zero;
			visual.transform.parent.gameObject.AddComponent<BillboardRotator>().invertFace = true;

			visual.transform.localPosition = Vector3.forward * 0.5f;

			bubble.renderer = visual;
			bubble.gameObject.layer = LayerStorage.standardEntities;
			bubble.entity = bubble.gameObject.CreateEntity(1f, 4f, visual.transform);
			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(bubble.transform);
			ObjectCreationExtensions.CreateImage(canvas, TextureExtensions.CreateSolidTexture(1, 1, new(0f, 0.5f, 0.5f, 0.35f)));
			bubble.bubbleCanvas = canvas;
			canvas.gameObject.SetActive(false);

			bub.bubPre = bubble;
		}

		const float pixs = 21f;

	}
}
