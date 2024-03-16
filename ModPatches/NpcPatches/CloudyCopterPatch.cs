using HarmonyLib;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Cumulo), "StopBlowing")]
	internal class CloudyCopterPatch
	{
		private static void Postfix(ref AudioManager ___audMan)
		{
			if (aud_PAH != null)
				___audMan.PlaySingle(aud_PAH);
		}

		internal static SoundObject aud_PAH;
	}
}
