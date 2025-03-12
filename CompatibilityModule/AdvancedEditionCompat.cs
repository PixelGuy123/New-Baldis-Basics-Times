using BBTimes.Manager;
using BaldisBasicsPlusAdvanced.API;
using System.Collections.Generic;
using HarmonyLib;
using MTM101BaldAPI;
using BaldisBasicsPlusAdvanced.Game.Objects;

namespace BBTimes.CompatibilityModule
{
	internal class AdvancedEditionCompat
	{
		internal static void Loadup()
		{
			List<string> strs = [];
			for (int i = 1; i <= elvTips; i++)
				strs.Add($"times_elv_tip{i}");
			ApiManager.AddNewTips(BBTimesManager.plug.Info, [.. strs]);
		}
		const int elvTips = 9;
	}

	[HarmonyPatch]
	[ConditionalPatchMod("mrsasha5.baldi.basics.plus.advanced")]
	internal class AdvancedPatches
	{
		[HarmonyPatch(typeof(AdvancedMathMachine), "GenerateProblem")]
		[HarmonyPostfix]
		static void ChangeMaxAnswer(ref int ___maxAnswer, List<MathMachineNumber> ___currentNumbers) =>
			___maxAnswer = ___currentNumbers.Count - 1;
	}
}
