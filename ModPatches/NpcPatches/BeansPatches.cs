using HarmonyLib;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Beans))]
	internal class BeansPatches
	{
		[HarmonyPatch("HitNPC")]
		[HarmonyPatch("HitPlayer")]
		private static void Prefix(Beans __instance) =>
			NPCPatches.SetGuilt(__instance, 4f, "gumming"); // Yeah
	}
}
