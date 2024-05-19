using BBTimes.CustomContent.NPCs;
using HarmonyLib;
using UnityEngine;
using PixelInternalAPI.Extensions;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class CrazyClockCustomData : CustomNPCData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
		[GetSound("clock_tick.wav", "Vfx_CC_Tick", SoundType.Voice, Color.yellow),
		GetSound("clock_tack.wav", "Vfx_CC_Tack", SoundType.Voice, Color.yellow),
		GetSound("clock_Scream.wav", "Vfx_CC_Scream", SoundType.Voice, Color.yellow),
		GetSound("clock_frown.wav", "Vfx_CC_Frown", SoundType.Voice, Color.yellow)];

		protected override Sprite[] GenerateSpriteOrder()
		{
			int z = 0;
			var sp = new Sprite[22];
			GetBoth("nervousTick");
			GetBoth("nervousTock");
			GetBoth("NormalTick");
			GetBoth("NormalTock");
			sp[z++] = GetSprite(35f, "ofrown.png");
			sp[z++] = GetSprite(35f, "PcrazyClockTick.png");
			sp[z++] = GetSprite(33f, "PcrazyClockTock.png");
			sp[z++] = GetSprite(35f, "qhide.png");
			for (int i = 0; i < 10; i++)
				sp[z++] = GetSprite(35f, $"qhide{i}.png");


			void GetBoth(string name, float p = 35f)
			{
				for (int i = 1; i <= 2; i++)
					sp[z++] = GetSprite(p, name + $"{i}.png");
			}

			return sp;
		}

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var clock = (CrazyClock)Npc;
			clock.data = this;
			clock.spriteRenderer[0].material = new(ObjectCreationExtensions.NonBillBoardPrefab.material);
			clock.GetComponents<Collider>().Do(x => x.enabled = false);
			clock.audMan = GetComponent<AudioManager>();
			clock.Navigator.enabled = false; // It's a static npc
			clock.Navigator.Entity.SetActive(false);
			clock.Navigator.Entity.enabled = false;
		}

	}
}
