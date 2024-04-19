using BBTimes.Manager;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(BeltBuilder), "Build", [typeof(EnvironmentController), typeof(LevelBuilder), typeof(RoomController), typeof(System.Random)])]
	internal class BeltManagerPatch
	{
		private static void Postfix(BeltManager ___beltManager, System.Random cRng)
		{
			var data = BBTimesManager.CurrentFloorData;
			if (data == null) return;

			___beltManager.SetSpeed(UnityEngine.Mathf.Max(1, ___beltManager.Speed + cRng.Next(-data.ConveyorSpeedOffset, data.ConveyorSpeedOffset + 1)));
		}
	}
}
