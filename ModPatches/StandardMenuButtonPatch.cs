//using HarmonyLib;
//using UnityEngine.Events;

//namespace BBTimes.ModPatches
//{
//	// Why do I have this, again?
//	//[HarmonyPatch(typeof(StandardMenuButton))]
//	//internal class StandardMenuButtonPatch
//	//{
//	//	[HarmonyPatch("Press")]
//	//	static bool Prefix(StandardMenuButton __instance)
//	//	{
//	//		if (overrideActive)
//	//		{
//	//			onPressOverride.Invoke(__instance);
//	//			onPressOverride.RemoveAllListeners();
//	//			overrideActive = false;
//	//			return false;
//	//		}
//	//		return true;
//	//	}

//	//	readonly static UnityEvent<StandardMenuButton> onPressOverride = new();
//	//	static bool overrideActive = false;

//	//	public static void AddOverrideForNextClick(UnityAction<StandardMenuButton> action)
//	//	{
//	//		onPressOverride.AddListener(action);
//	//		overrideActive = true;
//	//	}
//	//}
//}
