using HarmonyLib;
using PixelInternalAPI.Extensions;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Beans))]
	internal class BeansPatches
	{
		[HarmonyPatch("HitNPC")]
		[HarmonyPatch("HitPlayer")]
		private static void Prefix(Beans __instance) =>
			PrivateCallExtensions.SetGuilt(__instance, 4f, "gumming"); // Yeah
	}
}
