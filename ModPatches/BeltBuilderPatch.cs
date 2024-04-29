using BBTimes.CustomComponents;
using BBTimes.Manager;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(BeltBuilder))]
	internal class BeltBuilderPatch
	{
		[HarmonyPatch("Build", [typeof(EnvironmentController), typeof(LevelBuilder), typeof(RoomController), typeof(System.Random)])]
		private static void Postfix(BeltManager ___beltManager, System.Random cRng)
		{
			var data = BBTimesManager.CurrentFloorData;
			if (data == null) return;

			___beltManager.SetSpeed(UnityEngine.Mathf.Max(1, ___beltManager.Speed + cRng.Next(-data.ConveyorSpeedOffset, data.ConveyorSpeedOffset + 1)));
		}

		[HarmonyPostfix]
		[HarmonyPatch("Build", [typeof(EnvironmentController), typeof(LevelBuilder), typeof(RoomController), typeof(System.Random)])]
		[HarmonyPatch("Load")]
		static void RegisterConveyorHere(EnvironmentController ec, BeltManager ___beltManager) =>
			ec.GetComponent<EnvironmentControllerData>().ConveyorBelts.Add(___beltManager);

	}
}
