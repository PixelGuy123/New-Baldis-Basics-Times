using HarmonyLib;
using UnityEngine;
namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(GameCamera), "Awake")]
	internal class GameCameraPatch
	{
		private static void Prefix(GameCamera __instance)
		{
			var visual = Object.Instantiate(playerVisual, __instance.transform);
			visual.localPosition = Vector3.zero;
		}

		static internal Transform playerVisual;
	}
}
