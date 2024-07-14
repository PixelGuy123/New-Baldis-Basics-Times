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

		protected override Sprite[] GenerateSpriteOrder() =>
			GetSpriteSheet(5, 5, 35f, "crazyClockSheet.png");

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var clock = (CrazyClock)Npc;
			clock.audTick = soundObjects[0];
			clock.audTack = soundObjects[1];
			clock.allClockAudios = [.. soundObjects];
			clock.allClockSprites = [.. storedSprites];
			clock.spriteRenderer[0].material = new(ObjectCreationExtensions.NonBillBoardPrefab.material);
			clock.GetComponents<Collider>().Do(x => x.enabled = false);
			clock.audMan = GetComponent<AudioManager>();
			clock.Navigator.enabled = false; // It's a static npc
			clock.Navigator.Entity.SetActive(false);
			clock.Navigator.Entity.enabled = false;
		}

	}
}
