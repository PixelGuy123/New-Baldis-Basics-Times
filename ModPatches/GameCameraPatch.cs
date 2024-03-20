using BBTimes.CustomComponents.PlayerComponents;
using HarmonyLib;
namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(GameCamera), "Awake")]
	internal class GameCameraPatch
	{
		private static void Prefix(GameCamera __instance) =>
			__instance.gameObject.AddComponent<CustomPlayerCameraComponent>();
		
	}
}
