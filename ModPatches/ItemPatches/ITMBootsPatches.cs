using HarmonyLib;
using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches.ItemPatches
{
	[HarmonyPatch(typeof(ITM_Boots))]
	internal class ITMBootsPatches
	{
		[HarmonyPatch("Use")]
		private static void Prefix(PlayerManager pm) =>
			pm.GetAttribute().AddAttribute("boots");

		[HarmonyPatch("Timer", MethodType.Enumerator)]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> i) =>
			new CodeMatcher(i)
			.MatchForward(false,
				new(OpCodes.Ldloc_1),
				new(CodeInstruction.LoadField(typeof(ITM_Boots), "animator")),
				new(OpCodes.Ldstr, "Up", "Up"),
				new(OpCodes.Ldc_I4_M1),
				new(OpCodes.Ldc_R4, name:"0.0"),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(Animator), "Play", [typeof(string), typeof(int), typeof(float)]))
				)
			.InsertAndAdvance(
				new(OpCodes.Ldloc_1),
				CodeInstruction.LoadField(typeof(Item), "pm"), // Basically remove "boots" attribute
				new(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "GetComponent", [], [typeof(PlayerAttributesComponent)])),
				new(OpCodes.Ldstr, "boots"),
				CodeInstruction.Call(typeof(PlayerAttributesComponent), "RemoveAttribute", [typeof(string)])
				)

			.InstructionEnumeration();
	}
}
