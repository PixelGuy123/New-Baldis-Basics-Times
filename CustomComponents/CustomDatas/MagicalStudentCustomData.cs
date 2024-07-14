using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.NPCs;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class MagicalStudentCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSound("MGS_Throw.wav", "Vfx_MGS_Magic", SoundType.Voice, Color.white)];
		protected override Sprite[] GenerateSpriteOrder() =>
			[.. GetSpriteSheet(3, 1, 65f, "MGS.png"), GetSprite(25f, "MGS_Magic.png")];

		public override void SetupPrefab()
		{
			base.SetupPrefab();

			// magic prefab
			var mos = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[3]).AddSpriteHolder(0f, LayerStorage.standardEntities);
			var moHolder = mos.transform.parent;
			mos.name = "MagicRenderer";
			moHolder.name = "Magic";

			moHolder.gameObject.ConvertToPrefab(true);

			var mo = moHolder.gameObject.AddComponent<MagicObject>();
			mo.entity = moHolder.gameObject.CreateEntity(4f, 4f, mos.transform).SetEntityCollisionLayerMask(0);
			
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
