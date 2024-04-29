using BBTimes.CustomContent.Events;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(SodaMachine), "ItemFits")]
	internal class SodaMachinePatch
	{
		private static void Postfix(ref bool __result) =>
			__result = __result && BlackOut.activeBlackOuts <= 0; // checking __result like that will always return false if __result was false already
	}
}
