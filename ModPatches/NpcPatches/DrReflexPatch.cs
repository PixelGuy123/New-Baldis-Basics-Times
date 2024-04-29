using HarmonyLib;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(DrReflex), "RapidHammer")]
	internal class DrReflexPatch
	{
		static void Prefix(DrReflex __instance) =>
			__instance.behaviorStateMachine.ChangeState(new DrReflex_DoNothing(__instance)); // Fix the issue with the Hunting state always calling the HammerCheck
	}

	internal class DrReflex_DoNothing(DrReflex dr) : DrReflex_StateBase(dr)
	{
	}
}
