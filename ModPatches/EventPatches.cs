using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(RandomEvent), "Initialize")]
	internal class EventPatches
	{
		private static void Prefix(RandomEvent __instance) => __instance.gameObject.SetActive(true); // Just sets it active
	}
}
