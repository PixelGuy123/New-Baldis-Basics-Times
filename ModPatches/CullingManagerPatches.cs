using BBTimes.CustomComponents;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(CullingManager))]
	internal class CullingManagerPatches
	{
		[HarmonyPatch("CullChunk")]
		[HarmonyPostfix]
		static void CullNullChunks(CullingManager __instance, int chunkId) =>
			__instance.GetComponent<NullCullingManager>().CheckAllChunks(__instance.allChunks[chunkId]);
	}
}
