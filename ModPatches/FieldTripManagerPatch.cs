using HarmonyLib;
namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(FieldTripManager), "End")]
	internal class FieldTripManagerPatch
	{
		private static void Postfix(ref AudioManager ___baldiMan, int rank)
		{
			if (fieldTripYay != null && rank >= 3) // 3 stars
				___baldiMan.PlaySingle(fieldTripYay);
		}
		internal static SoundObject fieldTripYay;
	}
}
