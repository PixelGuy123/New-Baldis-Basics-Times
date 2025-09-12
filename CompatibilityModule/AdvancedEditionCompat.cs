using System.Collections.Generic;
using BaldisBasicsPlusAdvanced.API;
using BaldisBasicsPlusAdvanced.Game.Objects;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;

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
	[ConditionalPatchMod(Storage.guid_Advanced)]
	internal class AdvancedPatches
	{
		[HarmonyPatch(typeof(AdvancedMathMachine), "GenerateProblem")]
		[HarmonyPostfix]
		static void ChangeMaxAnswer(ref int ___maxAnswer, List<MathMachineNumber> ___currentNumbers) =>
			___maxAnswer = ___currentNumbers.Count - 1;
	}
}
