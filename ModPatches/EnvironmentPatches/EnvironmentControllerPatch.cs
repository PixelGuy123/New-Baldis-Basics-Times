using BBTimes.CustomComponents;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches.EnvironmentPatches
{
    [HarmonyPatch(typeof(EnvironmentController))]
    public class EnvironmentControllerPatch
    {

        [HarmonyPatch("GetNavNeighbors")]
        [HarmonyPostfix]
        private static void FixTiles(ref List<Cell> list) // A very *kinda invasive* specific patch to allow nav neighbors to search avoiding unwanted spots
        {
			if (shapes == null) // it can't be null, right?
				return;

            for (int i = 0; i < list.Count; i++)
            {
                if ((limits.Length != 0 && !limits.Contains(list[i].room.type)) || (shapes.Length != 0 && shapes.Contains(list[i].shape)))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
			if (!persistentData)
				ResetData();
        }
		public static void SetNewData(TileShape[] shapes, RoomType[] limitToRoomTypes, bool persistent)
		{
			EnvironmentControllerPatch.shapes = shapes;
			limits = limitToRoomTypes;
			persistentData = persistent;
		}
		public static void ResetData()
		{
			shapes = null;
			limits = null;
			persistentData = false;
		}

		static TileShape[] shapes = null;
		static RoomType[] limits = null;
		static bool persistentData = false;


		[HarmonyPatch("InitializeLighting")]
		[HarmonyPostfix]
		private static void FixLighting(EnvironmentController __instance)
		{
			int maxX = Singleton<CoreGameManager>.Instance.lightMapTexture.width;
			int maxZ = Singleton<CoreGameManager>.Instance.lightMapTexture.height;
			for (int x = 0; x < maxX; x++)
			{
				for (int z = 0; z < maxZ; z++)
				{
					if (__instance.ContainsCoordinates(x, z) || Singleton<CoreGameManager>.Instance.lightMapTexture.GetPixel(x, z).a <= 0.1f)
						continue;

					Singleton<CoreGameManager>.Instance.UpdateLighting(__instance.standardDarkLevel, new(x, z)); // Should fix the red lighting appearing in earlier floors after beating F3

				}
			}
		}


		[HarmonyPatch("StartEventTimers")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> GetMyEvents(IEnumerable<CodeInstruction> i) =>
			new CodeMatcher(i)
			.MatchForward(true, 
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.LoadField(typeof(EnvironmentController), "events")),
				new(OpCodes.Ldloc_0),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(List<RandomEvent>), "get_Item", [typeof(int)])),
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.LoadField(typeof(EnvironmentController), "eventTimes")),
				new(OpCodes.Ldloc_0),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(List<float>), "get_Item", [typeof(int)])),
				new(CodeInstruction.Call(typeof(EnvironmentController), "EventTimer", [typeof(RandomEvent), typeof(float)])),
				new(CodeInstruction.Call(typeof(MonoBehaviour), "StartCoroutine", [typeof(IEnumerator)]))
				)
			.Advance(1)
			.SetInstruction(Transpilers.EmitDelegate<Action<Coroutine, EnvironmentController>>((c, e) => e.GetComponent<EnvironmentControllerData>()?.OngoingEvents.Add(c))) // Replace 'pop' (which basically means, "take out of the stack") to actually use it in a delegate
			.Insert(new CodeInstruction(OpCodes.Ldarg_0)) // Before the delegate to grab the ec reference
			.InstructionEnumeration();
		
    }
}
