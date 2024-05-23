using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using MTM101BaldAPI;
using MTM101BaldAPI.ObjectCreation;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class MagicalStudentCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("MGS_Throw.wav", "Vfx_MGS_Magic", SoundType.Voice, Color.white)];
		protected override Sprite[] GenerateSpriteOrder() =>
			[GetSprite(65f, "MGS_Throw1.png"), GetSprite(65f, "MGS_Throw2.png"), GetSprite(65f, "MGS_Throw3.png"),
			GetSprite(25f, "MGS_Magic.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();

			// magic prefab
			var moe = new EntityBuilder()
				.SetName("Magic")
				.AddTrigger(4f)
				.SetBaseRadius(4f)
				.AddRenderbaseFunction((e) =>
				{
					var r = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[3]);
					r.transform.SetParent(e.transform);
					r.transform.localPosition = Vector3.zero;
					return r.transform;
				})
				.Build();
			moe.gameObject.ConvertToPrefab(true);

			var mo = moe.gameObject.AddComponent<MagicObject>();
			mo.entity = moe;
			
			// MGS Setup
			var mgs = (MagicalStudent)Npc;
			mgs.magicPre = mo;
			mgs.audMan = GetComponent<PropagatedAudioManager>();
			mgs.audThrow = soundObjects[0];
			mgs.throwSprites = storedSprites;
			mgs.renderer = mgs.spriteRenderer[0];
		}
	}
}
