using HarmonyLib;

namespace BBTimes.Extensions
{
	[HarmonyPatch]
	internal class PrivateCalls
	{
		[HarmonyPatch(typeof(AudioManager), "Start")]
		[HarmonyReversePatch]
		internal static void RestartAudioManager(object instance) => throw new System.NotImplementedException();
	}
}
