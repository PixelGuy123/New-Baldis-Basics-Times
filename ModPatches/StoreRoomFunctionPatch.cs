using BBTimes.CustomContent.Objects;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(StoreRoomFunction))]
	internal class StoreRoomFunctionPatch
	{
		[HarmonyPatch(nameof(StoreRoomFunction.OnPlayerExit))]
		[HarmonyPrefix]
		static void BaldiSaysByeByeToo(StoreRoomFunction __instance)
		{
			if (!__instance.itemPurchased || !__instance.open)
				return;

			foreach (var baldi in Object.FindObjectsOfType<ChristmasBaldi>())
				if (__instance.Room.ec.CellFromPosition(baldi.position).TileMatches(__instance.Room))
					baldi.SayMerryChristmas();
		}
	}
}
