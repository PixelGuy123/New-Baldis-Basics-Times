using BBTimes.CustomContent.NPCs;
using HarmonyLib;
using MTM101BaldAPI;
using BBE;
using BBE.CustomClasses;
using BBE.NPCs;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using BBTimes.CustomComponents;
using System;

namespace BBTimes.CompatibilityModule
{
	[HarmonyPatch]
	[ConditionalPatchMod("rost.moment.baldiplus.extramod")]
	internal static class BBExtraCompat
	{
		[HarmonyPatch(typeof(ZeroPrize), "Initialize")]
		[HarmonyPrefix]
		static void ZeroPrizeQuantum(ref float ___moveModMultiplier, ref float ___speed, ref float ___minActive, ref float ___maxActive, ref float ___minWait, ref float ___maxWait)
		{
			if (FunSettingsType.HardMode.IsActive())
			{
				___moveModMultiplier = 0.99f;
			}

			if (FunSettingsType.QuantumSweep.IsActive())
			{
				___speed = 250f;
				___minActive = int.MaxValue;
				___maxActive = int.MaxValue;
				___minWait = 1f;
				___maxWait = 1f;
			}
		}

		[HarmonyPatch(typeof(CoolMop), "Initialize")]
		[HarmonyPrefix]
		static void QuantumMop(ref float ___slipDropCooldown, ref int ___slipsPerTile, ref float ___speed, ref float ___minActive, ref float ___maxActive, ref float ___minWait, ref float ___maxWait)
		{
			if (FunSettingsType.HardMode.IsActive())
			{
				___slipsPerTile = 15;
				___slipDropCooldown = 0.5f;
			}

			if (FunSettingsType.QuantumSweep.IsActive())
			{
				___speed = 250f;
				___minActive = int.MaxValue;
				___maxActive = int.MaxValue;
				___minWait = 1f;
				___maxWait = 1f;
			}
		}

		[HarmonyPatch(typeof(Mopliss), "Initialize")]
		[HarmonyPrefix]
		static void QuantumMopliss(EnvironmentController ___ec, ref int ___roomsPerActivation, ref int ___slipperRadius, ref float ___speed, ref float ___minWait, ref float ___maxWait)
		{
			if (FunSettingsType.HardMode.IsActive())
			{
				___roomsPerActivation = ___ec.rooms.Count;
				___slipperRadius = 12;
			}

			if (FunSettingsType.QuantumSweep.IsActive())
			{
				___speed = 125f;
				___minWait = 1f;
				___maxWait = 1f;
			}
		}
	}

	[HarmonyPatch]
	[ConditionalPatchModByVersion("rost.moment.baldiplus.extramod", "2.1.9.5", includePostVersions:true, invertCondition:true)]
	internal static class BBExtraCompat_KulakOldFix
	{
		[HarmonyPatch(typeof(Kulak_Angry), "OnStateTriggerStay")]
		[HarmonyPatch(typeof(Kulak_Wandering), "OnStateTriggerStay")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> KulakDontBreakUnbreakable(IEnumerable<CodeInstruction> i) =>
			new CodeMatcher(i)
			.MatchForward(true,
				new(OpCodes.Nop),
				new(OpCodes.Ldarg_1),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "GetComponent", [], [typeof(Window)])),
				new(CodeInstruction.LoadField(typeof(Window), "broken")),
				new(OpCodes.Ldc_I4_0),
				new(OpCodes.Ceq),
				new(OpCodes.Stloc_1)
				)
			.Advance(1)
			.InsertAndAdvance(
				new(OpCodes.Ldloc_1),
				new(OpCodes.Ldarg_1),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "GetComponent", [], [typeof(CustomWindowComponent)])),
				new(Transpilers.EmitDelegate<Func<bool, CustomWindowComponent, bool>>((loc, win) =>
				{
					if (!win || !loc) return false; // If it's already false, it means it is broken
					return !win.unbreakable; // Otherwise, if the window is unbreakable, return the inverse
				})),
				new(OpCodes.Stloc_1)
				)
			.InstructionEnumeration();
	}
}
