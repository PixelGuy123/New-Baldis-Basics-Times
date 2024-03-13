using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(BaseGameManager))]
	internal class PostGenPatch
	{
		[HarmonyPatch("Initialize")]
		[HarmonyPostfix]
		private static void PostGen(BaseGameManager __instance)
		{
			var comp = __instance.GetComponent<MainGameManagerExtraComponent>();
			if (BooleanStorage.SkyboxOverride)
				Shader.SetGlobalTexture("_Skybox", comp == null || comp.mapForToday == null ? ObjectCreationExtension.defaultCubemap : comp.mapForToday);
			

		}
	}
}
