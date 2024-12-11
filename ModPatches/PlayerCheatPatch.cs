using BBTimes.CustomComponents;
using BepInEx.Bootstrap;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(StructureBuilder), "Finished")]
	internal class LogFinishedStructures
	{
		static void Prefix(StructureBuilder __instance) =>
			Debug.LogWarning($"The builder {__instance.GetType().Name} is finished.");
	}

	[HarmonyPatch(typeof(PlayerMovement), "PlayerMove")]
	internal class PlayerCheatPatch
	{
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

			var dat =ec.GetComponent<EnvironmentControllerData>();
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

	//[HarmonyPatch]
	//internal class CrashDebugging
	//{
	//	[HarmonyTargetMethods]
	//	static MethodInfo[] AllTheBuildCalls()
	//	{
	//		List<MethodInfo> methods = [AccessTools.Method(typeof(RoomFunctionContainer), "Build")];

	//		foreach (var type in AccessTools.AllTypes())
	//		{
	//			if (type.IsSubclassOf(typeof(ObjectBuilder)) || type.IsSubclassOf(typeof(HallBuilder)))
	//			{
	//				Debug.Log("type detected: " + type.FullName);
	//				methods.Add(type.GetMethod("Build"));
	//			}
	//		}
	//		return [.. methods];
	//	}

	//	[HarmonyFinalizer]
	//	static System.Exception DebugLogFinalizer(System.Exception __exception)
	//	{
	//		if (__exception != null)
	//			Debug.LogException(__exception);

	//		return null;
	//	}



	//}
}
