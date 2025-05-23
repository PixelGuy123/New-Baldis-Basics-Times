using System.Collections.Generic;
using System.Reflection.Emit;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using HarmonyLib;
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
				new(CodeInstruction.LoadField(typeof(ITM_Boots), "gauge")),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(HudGauge), "Deactivate", []))
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
