using HarmonyLib;

namespace BBTimes.ModPatches.ItemPatches
{
	[HarmonyPatch(typeof(ITM_NanaPeel), "Use")]
	internal class ITMNanaPeelPatch
	{
		static void Prefix(PlayerManager pm) =>
			pm.RuleBreak("littering", 5f, 0.8f);
	}
}
