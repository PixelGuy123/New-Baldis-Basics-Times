using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Bully), "SentToDetention")]
	internal class BullyPatch
	{
		private static void Postfix(ref SpriteRenderer ___spriteToHide) =>
			___spriteToHide.enabled = true; // yeah, he exists, but uuh not hidden now
		
	}
}
