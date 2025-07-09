using System.Collections.Generic;
using System.Reflection.Emit;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using TMPro;
using UnityEngine;

namespace BBTimes.ModPatches.EnvironmentPatches
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
				new(OpCodes.Ldc_I4_S, name: "10") // Never use a number to check a value, you don't know if it is an integer, short, whatever the compiler did to optimize it
				)
			.SetInstruction(Transpilers.EmitDelegate(() =>
			{
				var ld = Singleton<BaseGameManager>.Instance.levelObject;
				if (ld is not CustomLevelGenerationParameters cld)
					return BBTimesManager.MaximumNumballs + 1;

				var minMaxObj = cld.GetCustomModValue(BBTimesManager.plug.Info, "Times_EnvConfig_MathMachineNumballsMinMax");

				if (minMaxObj == null)
					return BBTimesManager.MaximumNumballs + 1;

				IntVector2 minMax = (IntVector2)minMaxObj;

				return Random.Range(minMax.x, minMax.z + 1) + 1;
			}))
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

		internal static Sprite rightSprite;
	}
}
