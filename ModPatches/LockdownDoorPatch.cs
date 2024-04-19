using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(LockdownDoor), "Awake")]
	internal class LockdownDoorPatch
	{
		private static void Postfix(ref float ___speed)
		{
			var data = BBTimesManager.CurrentFloorData;
			if (data == null || !PostRoomCreation.i) return;

			___speed += PostRoomCreation.i.controlledRNG.Next(data.LockdownDoorSpeedOffset + 1);
		}

	}
}
