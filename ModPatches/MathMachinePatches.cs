using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MathMachine))]
	internal class MathMachinePatches
	{
		[HarmonyPostfix]
		[HarmonyPatch("Completed")]
		private static void WOOOW(ref AudioManager ___audMan)
		{
			if (aud_BalWow != null)
				___audMan.PlaySingle(aud_BalWow);
		}

		internal static SoundObject aud_BalWow;
	}
}
