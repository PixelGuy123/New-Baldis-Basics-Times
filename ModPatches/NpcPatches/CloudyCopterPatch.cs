using HarmonyLib;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Cumulo))]
	internal class CloudyCopterPatch
	{
		[HarmonyPatch("StopBlowing")]
		private static void Postfix(Cumulo __instance)
		{
			if (aud_PAH != null)
				__instance.GetComponent<PropagatedAudioManager>()?.PlaySingle(aud_PAH);
		}

		internal static SoundObject aud_PAH;
	}
}
