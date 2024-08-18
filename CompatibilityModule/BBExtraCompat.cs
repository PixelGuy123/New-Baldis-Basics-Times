using BBTimes.CustomContent.NPCs;
using HarmonyLib;
using MTM101BaldAPI;
using BBE;
using BBE.CustomClasses;

namespace BBTimes.CompatibilityModule
{
	[HarmonyPatch]
	[ConditionalPatchMod("rost.moment.baldiplus.extramod")]
	internal static class BBExtraCompat
	{
		[HarmonyPatch(typeof(ZeroPrize), "Initialize")]
		[HarmonyPrefix]
		static void ZeroPrizeQuantum(ref float ___moveModMultiplier, ref float ___speed, ref float ___minActive, ref float ___maxActive, ref float ___minWait, ref float ___maxWait)
		{
			if (FunSettingsType.HardMode.IsActive())
			{
				___moveModMultiplier = 0.99f;
			}

			if (FunSettingsType.QuantumSweep.IsActive())
			{
				___speed = 250f;
				___minActive = int.MaxValue;
				___maxActive = int.MaxValue;
				___minWait = 1f;
				___maxWait = 1f;
			}
		}
	}
}
