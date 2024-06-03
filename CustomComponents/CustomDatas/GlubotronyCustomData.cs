using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Extensions;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class GlubotronyCustomData : CustomNPCData
	{
		protected override Sprite[] GenerateSpriteOrder()
		{
			Sprite[] spr = new Sprite[24];
			int z = 0;
			for (int i = 1; i <= 8; i++)
				spr[z++] = GetSprite(pixelsPerUnit, $"idle{i}.png");
			for (int i = 1; i <= 8; i++)
				spr[z++] = GetSprite(pixelsPerUnit, $"leftstep{i}.png");
			for (int i = 1; i <= 8; i++)
				spr[z++] = GetSprite(pixelsPerUnit, $"rightstep{i}.png");
			return spr;
		}
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var gb = (Glubotrony)Npc;
			gb.audMan = GetComponent<PropagatedAudioManager>();

			gb.renderer = gb.CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Take(8)]),
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Skip(8).Take(8)]),
				GenericExtensions.CreateRotationMap(8, [.. storedSprites.Skip(8).Skip(8).Take(8)])
				);

			gb.sprIdle = storedSprites[0];
			gb.sprStep1 = storedSprites[8];
			gb.sprStep2 = storedSprites[16];

			
		}

		const float pixelsPerUnit = 55f;
	}
}
