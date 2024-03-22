using HarmonyLib;

namespace BBTimes.ModPatches
{

	// Some level editor Iguesss
#if CHEAT

	[HarmonyPatch(typeof(PlayerMovement))]
	internal class Fast
	{
		
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		private static void GottaGoFAST(PlayerMovement __instance)
		{
			__instance.walkSpeed *= 3;
			__instance.runSpeed *= 3;
		}
	}

#endif
}
