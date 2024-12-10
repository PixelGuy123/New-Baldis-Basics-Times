using BBTimes.Manager;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MathMachine))]
	internal class MathMachinePatches
	{
		[HarmonyPrefix]
		[HarmonyPatch("Start")]
		private static void RightIcon(Notebook ___notebook) =>
			___notebook.icon.spriteRenderer.sprite = rightSprite;
		

		[HarmonyPostfix]
		[HarmonyPatch("Completed", [typeof(int)])]
		private static void WOOOW(MathMachine __instance)
		{
			var t = __instance.transform.Find("Answer").GetComponent<TextMeshPro>();
			t.autoSizeTextContainer = false;
			t.autoSizeTextContainer = true; // 10+ answers don't look ugly
			// Must be exactly after completing, so it actually adapts
		}

		[HarmonyPatch("ReInit")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> ExtraNumballs(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(true,
				new(OpCodes.Ldloc_2),
				new(OpCodes.Ldc_I4_S, name:"10") // Never use a number to check a value, you don't know if it is an integer, short, whatever the compiler did to optimize it
				)
			.SetInstruction(Transpilers.EmitDelegate(() => BBTimesManager.CurrentFloorData == null ? BBTimesManager.MaximumNumballs + 1 : UnityEngine.Random.Range(BBTimesManager.CurrentFloorData.MinNumberBallAmount, BBTimesManager.CurrentFloorData.MaxNumberBallAmount + 1) + 1))
			.InstructionEnumeration();

		[HarmonyPatch("NewProblem")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> HigherThan10Answers(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.End() // Get the last one
			.MatchBack(true,
				new(OpCodes.Ldc_I4_0),
				new(OpCodes.Ldc_I4_S, name: "10"), // Never use a number to check a value, you don't know if it is an integer, short, whatever the compiler did to optimize it
				new(OpCodes.Ldloc_2),
				new(OpCodes.Sub)
				)
			.Advance(-1)
			.RemoveInstructions(2) // Removes the subtraction
			
			.InstructionEnumeration();

		internal static UnityEngine.Sprite rightSprite;
	}
}
