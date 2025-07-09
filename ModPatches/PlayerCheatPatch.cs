using System.Collections.Generic;
using System.Reflection;
using BBTimes.CustomComponents;
using BBTimes.Manager;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{


	// [HarmonyPatch(typeof(StructureBuilder), "Finished")]
	// internal class LogFinishedStructures
	// {
	// 	static void Prefix(StructureBuilder __instance) =>
	// 		Debug.LogWarning($"The builder {__instance.GetType().Name} is finished.");
	// }
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
	[HarmonyPatch(typeof(PlayerMovement), "PlayerMove")]
	internal class PlayerCheatPatch
	{

		// [HarmonyPatch(typeof(EnvironmentController), "SetTileInstantiation")]
		// [HarmonyPostfix]
		// static void ChangeThisToTrue(EnvironmentController __instance) =>
		// 	__instance.instantiateTiles = true;

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
	}

	/*
	[HarmonyPatch]
	internal static class CrashDebugging
	{
		[HarmonyTargetMethods]
		static List<MethodInfo> AllTheBuildCalls()
		{
			List<MethodInfo> methodInfos = [];
			HashSet<Assembly> allowedAssemblies = [Assembly.GetAssembly(typeof(BBTimesManager))];
			List<string> allowedNames = ["BaldisBasicsPlusAdvanced", "BBTimes"];

			foreach (var info in Chainloader.PluginInfos)
			{
				if (info.Value != null && info.Value.Instance != null)
					allowedAssemblies.Add(Assembly.GetAssembly(info.Value.Instance.GetType()));
			}

			foreach (var assembly in allowedAssemblies)
			{
				if (!allowedAssemblies.Contains(assembly) || !allowedNames.Exists(assembly.FullName.Contains))
					continue;

				bool isAssemblyCSharp = assembly.FullName.Contains("Assembly-CSharp");

				foreach (var type in assembly.GetTypes())
				{
					try
					{
						if (!type.IsClass || !type.IsPublic)
							continue;
						if (isAssemblyCSharp && type.Namespace != string.Empty) // Empty because that's what the game types comes from
							continue;

						// Skip problematic types known to throw exceptions in their static constructors
						if (type.FullName == "BaldisBasicsPlusAdvanced.Game.Objects.Voting.Topics.LightsEconomyTopic" || type.FullName == "BaldisBasicsPlusAdvanced.Cache.AssetsManagment.AssetsStorage")
						{
							Debug.LogWarning($"TIMES: Skipping problematic type: {type.FullName}");
							continue;
						}

						Debug.LogWarning($"TIMES: Type detected: {type.FullName} | Assembly: {assembly.FullName} | Namespace: {type.Namespace}");
						MethodInfo[] methods;
						try
						{
							methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
						}
						catch (System.Exception ex)
						{
							Debug.LogWarning($"TIMES: Failed to get methods for type {type.FullName}: {ex.Message}");
							continue;
						}
						for (int i = 0; i < methods.Length; i++)
						{
							try
							{
								var m = methods[i];
								// Filter out methods that Harmony/Mono.Cecil can't handle
								if (m == null)
									continue;
								if (m.IsAbstract)
									continue;
								if (m.IsGenericMethodDefinition)
									continue;
								if (m.ContainsGenericParameters)
									continue;
								if (m.DeclaringType == null)
									continue;
								if (!m.IsPublic)
									continue;
								// Harmony also can't patch methods with no body (e.g. interface methods)
								if (m.GetMethodBody() == null)
									continue;

								methodInfos.Add(m);
							}
							catch (System.Exception ex)
							{
								Debug.LogWarning($"TIMES: Skipped method due to exception: {ex.Message}");
							}
						}
					}
					catch (System.Exception e)
					{
						Debug.LogError("TIMES: Error while doing the funny: " + e.Message);
					}
				}
			}

			return methodInfos;


		}

		[HarmonyFinalizer]
		static System.Exception DebugLogFinalizer(System.Exception __exception)
		{
			if (__exception != null)
				Debug.LogException(__exception);

			return null;
		}
	}

#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
*/

	// [HarmonyPatch]
	// internal static class EasyTraceBack
	// {
	// 	[HarmonyTargetMethods]
	// 	static IEnumerable<MethodInfo> AllTheBuildCalls()
	// 	{
	// 		System.Type[] allowedTypes = [typeof(RoomFunctionContainer), typeof(StructureBuilder), typeof(LevelBuilder), typeof(RoomController)];
	// 		HashSet<string> prohibitedMethods = ["FrameShouldEnd", "ProximityCheck", "RoomFits", "get_Ec", "AddTile", "RemoveTile", "GetNewTileList", "WeightFromPos", "Update"];

	// 		foreach (var type in allowedTypes)
	// 		{
	// 			foreach (var method in AccessTools.GetDeclaredMethods(type))
	// 			{
	// 				if (!prohibitedMethods.Contains(method.Name))
	// 					yield return method;
	// 			}
	// 		}
	// 	}

	// 	[HarmonyPrefix]
	// 	static void LogExistence(MethodBase __originalMethod, object __instance)
	// 	{
	// 		Debug.Log($"Called method: {__originalMethod.DeclaringType.FullName}.{__originalMethod.Name} on Instance: {__instance}");
	// 	}

	// 	[HarmonyFinalizer]
	// 	static System.Exception DebugLogFinalizer(System.Exception __exception)
	// 	{
	// 		if (__exception != null)
	// 			Debug.LogException(__exception);

	// 		return null;
	// 	}
	// }

}
