using HarmonyLib;
using System.Collections;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(RandomEvent))]
	internal class EventPatches
	{
		[HarmonyPatch("End")]
		static void Postfix(RandomEvent __instance, ref IEnumerator ___eventTime) // Basically, if the event is forced to end earlier, the timer is stopped
		{
			if (___eventTime != null)
				__instance.StopCoroutine(___eventTime);
		}
	}
}
