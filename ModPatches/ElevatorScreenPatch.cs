using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(ElevatorScreen), "ZoomIntro", MethodType.Enumerator)]
	internal class ElevatorScreenPatch
	{
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "Elevator", "Elevator"))
			.SetInstruction(Transpilers.EmitDelegate(() => elevatorMidis[Random.Range(0, elevatorMidis.Count)]))
			.InstructionEnumeration();

		readonly internal static List<string> elevatorMidis = ["Elevator"]; // "Elevator" must be included in the list
	}
}
