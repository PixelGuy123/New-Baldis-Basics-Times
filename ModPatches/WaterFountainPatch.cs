using BBTimes.CustomContent.Events;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(WaterFountain), "Clicked")]
	internal class WaterFountainPatch // just disable fountain when frozen event active
	{
		static bool Prefix() =>
			FrozenEvent.activeFrozenEvents <= 0;
	}
}
