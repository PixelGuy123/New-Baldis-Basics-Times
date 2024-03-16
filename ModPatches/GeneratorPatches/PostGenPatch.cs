using BBTimes.Manager;
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
			BBTimesManager.prefabs.ForEach(x => x.SetActive(false));
		}
	}
}
