using System.Collections.Generic;
using System.Reflection;
using BBTimes.CustomComponents;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(StructureBuilder), "Finished")]
	internal class LogFinishedStructures
	{
		static void Prefix(StructureBuilder __instance) =>
			Debug.LogWarning($"The builder {__instance.GetType().Name} is finished.");
	}
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
	[HarmonyPatch(typeof(PlayerMovement), "PlayerMove")]
	internal class PlayerCheatPatch
	{

		//public static List<string> GetLayerNamesFromMask(LayerMask layerMask)
		//{
		//	List<string> layerNames = [];

		//	for (int i = 0; i < 32; i++)
		//	{

		//		if ((layerMask.value & (1 << i)) != 0)
		//		{
		//			string layerName = LayerMask.LayerToName(i);
		//			if (!string.IsNullOrEmpty(layerName))
		//			{
		//				layerNames.Add(layerName);
		//			}
		//		}

		//	}

		//	return layerNames;
		//}

		//static void Prefix(PlayerMovement __instance)
		//{
		//	if (Input.GetKeyDown(KeyCode.T))
		//	{
		//		PullOutAnEvent();
		//	}
		//}
		static void PullOutAnEvent() // In case I wanna trigger it through Unity Explorer
		{
			var ec = Singleton<BaseGameManager>.Instance.Ec;

			var dat = ec.GetComponent<EnvironmentControllerData>();
			foreach (var ev in dat.OngoingEvents)
				if (ev != null)
					ec.StopCoroutine(ev);

			dat.OngoingEvents.Clear();
			if (ec.events.Count == 0 || ec.events[0].Active)
			{
				Debug.LogWarning("No event detected!");
				return;
			}
			ec.StartCoroutine(ec.EventTimer(ec.events[0], 5f));
			ec.events.RemoveAt(0);
		}
	}

	// [HarmonyPatch]
	// internal class CrashDebugging
	// {
	// 	[HarmonyTargetMethods]
	// 	static MethodInfo[] AllTheBuildCalls()
	// 	{
	// 		List<MethodInfo> methods = [];

	// 		foreach (var type in AccessTools.AllTypes())
	// 		{
	// 			if (type.IsSubclassOf(typeof(StructureBuilder)) || type == typeof(RoomFunctionContainer))
	// 			{
	// 				Debug.LogWarning("TIMES: Type detected: " + type.FullName);
	// 				foreach (var methodName in AccessTools.GetMethodNames(type))
	// 				{
	// 					var method = type.GetMethod(methodName);
	// 					if (method != null && method.Name == "Build")
	// 					{
	// 						methods.Add(method);
	// 					}
	// 				}
	// 			}
	// 		}

	// 		return [.. methods];
	// 	}

	// 	[HarmonyFinalizer]
	// 	static System.Exception DebugLogFinalizer(System.Exception __exception)
	// 	{
	// 		if (__exception != null)
	// 			Debug.LogException(__exception);

	// 		return null;
	// 	}
	// }
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
}
