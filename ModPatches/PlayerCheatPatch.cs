﻿//using BBTimes.CustomComponents;
//using BepInEx.Bootstrap;
//using HarmonyLib;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;

//namespace BBTimes.ModPatches
//{
	//[HarmonyPatch(typeof(PlayerMovement), "PlayerMove")]
	//internal class PlayerCheatPatch
	//{
	//static void Prefix(PlayerMovement __instance)
		//{
			//if (Input.GetKeyDown(KeyCode.T))
			//{
			//	var dat = __instance.pm.ec.GetComponent<EnvironmentControllerData>();
			//	foreach (var ev in dat.OngoingEvents)
			//		if (ev != null)
			//			__instance.pm.ec.StopCoroutine(ev);

			//	dat.OngoingEvents.Clear();
			//	if (__instance.pm.ec.events.Count == 0 || __instance.pm.ec.events[0].Active)
			//	{
			//		Debug.LogWarning("No event detected!");
			//		return;
			//	}
			//	__instance.pm.ec.StartCoroutine(__instance.pm.ec.EventTimer(__instance.pm.ec.events[0], 5f));
			//	__instance.pm.ec.events.RemoveAt(0);
			//}
		//}
	//}

//	[HarmonyPatch]
//	internal class CrashDebugging
//	{
//		[HarmonyTargetMethods]
//		static MethodInfo[] AllTheBuildCalls()
//		{
//			List<MethodInfo> methods = [AccessTools.Method(typeof(RoomFunctionContainer), "Build")];

//			foreach (var type in AccessTools.AllTypes())
//			{
//				if (type.IsSubclassOf(typeof(ObjectBuilder)) || type.IsSubclassOf(typeof(HallBuilder)))
//				{
//					Debug.Log("type detected: " + type.FullName);
//					methods.Add(type.GetMethod("Build"));
//				}
//			}
//			return [.. methods];
//        }

//		[HarmonyFinalizer]
//		static System.Exception DebugLogFinalizer(System.Exception __exception)
//		{
//			if (__exception != null)
//				Debug.LogException(__exception);

//			return null;
//		}



//	}
//}
