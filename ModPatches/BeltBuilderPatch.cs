using BBTimes.CustomComponents;
using BBTimes.Manager;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(BeltBuilder))]
	internal class BeltBuilderPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch("Build", [typeof(EnvironmentController), typeof(LevelBuilder), typeof(RoomController), typeof(System.Random)])]
		[HarmonyPatch("Load")]
		static void RegisterConveyorHere(EnvironmentController ec, BeltManager ___beltManager) =>
			ec.GetComponent<EnvironmentControllerData>().ConveyorBelts.Add(___beltManager);

	}
}
