using HarmonyLib;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(LevelGenerator), "StartGenerate")]
	public class LevelGeneratorInstanceGrabber
	{
		private static void Prefix(LevelGenerator __instance) =>
			i = __instance;

		internal static LevelGenerator i;
	}
}
