using BBTimes.Manager;
using HarmonyLib;
using InfiniteFloorsRemake.API.InfiniteWeightsDictionaries;
using InfiniteFloorsRemake.BepInEx;
using MTM101BaldAPI;

namespace BBTimes.CompatibilityModule
{
	[ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.infinitefloors")]
	[HarmonyPatch(typeof(InfinitePlugin), "PostProcessDone")]

	internal static class InfiniteFloorsCompat
	{
		static void Prefix()
		{
			ControlledWeightsDictionary.GetInfiniteObjectBuilderWeight(EnumExtensions.GetFromExtendedName<Obstacle>("Vent"), BBTimesManager.plug.Info).AllowStacking = true;
			ControlledWeightsDictionary.GetInfiniteObjectBuilderWeight(EnumExtensions.GetFromExtendedName<Obstacle>("Trapdoor"), BBTimesManager.plug.Info).AllowStacking = true;
		}
	}
}
