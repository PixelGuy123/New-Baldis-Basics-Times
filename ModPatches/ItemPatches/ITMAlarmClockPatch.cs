using HarmonyLib;

namespace BBTimes.ModPatches.ItemPatches
{
	[HarmonyPatch(typeof(ITM_AlarmClock))]
	internal class ITMAlarmClockPatch
	{
		[HarmonyPatch("Use")]
		private static void Prefix(PlayerManager pm) =>
			pm.RuleBreak("littering", 5f, 0.8f);
	}
}
