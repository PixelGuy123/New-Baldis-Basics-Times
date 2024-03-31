using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(ITM_Scissors), "Use")]
	internal class ScissorsPatch
	{
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> CutCountsAsBullying(IEnumerable<CodeInstruction> i) =>
			new CodeMatcher(i)
			.MatchForward(true, 
				new(OpCodes.Ldarg_1),
				new(CodeInstruction.LoadField(typeof(PlayerManager), "jumpropes")),
				new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(List<Jumprope>), "Count")),
				new(OpCodes.Ldc_I4_0)
			)
			.Advance(2)
			.InsertAndAdvance(
				new(OpCodes.Ldarg_1),
				new(OpCodes.Ldstr, "Bullying"), // Basically give guilt to player
				new(OpCodes.Ldc_R4, 5f),
				new(CodeInstruction.Call(typeof(PlayerManager), "RuleBreak", [typeof(string), typeof(float)]))
				)
			.InstructionEnumeration();
	}
}
