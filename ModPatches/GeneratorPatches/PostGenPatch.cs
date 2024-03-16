using HarmonyLib;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(BaseGameManager))]
	internal class PostGenPatch
	{
		[HarmonyPatch("Initialize")]
		[HarmonyPostfix]
		private static void PostGen(BaseGameManager __instance)
		{
			

		}
	}
}
