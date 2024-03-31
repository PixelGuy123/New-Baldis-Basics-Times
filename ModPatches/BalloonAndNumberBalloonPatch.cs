using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch]
	internal class BalloonAndNumberBalloonPatch
	{
		[HarmonyPatch(typeof(Balloon), "Initialize")]
		private static void Prefix(Balloon __instance) =>
			__instance.gameObject.SetActive(true); // The only thing literally

	}
}
