using BBTimes.CustomComponents;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(CullingManager))]
	internal class CullingManagerPatches
	{
		[HarmonyPatch("CullAll")]
		[HarmonyPatch("CullChunk")]
		[HarmonyPatch("RenderChunk")]
		[HarmonyPostfix]
		static void CullNullChunks(CullingManager __instance) =>
			__instance.GetComponent<NullCullingManager>()?.CheckAllChunks();
		

		[HarmonyPatch("PrepareOcclusionCalculations")]
		[HarmonyPostfix]
		static void SetupNullCulling(CullingManager __instance) =>
			__instance.GetComponent<NullCullingManager>()?.ReorganizeRendererPairs();
	}
}
