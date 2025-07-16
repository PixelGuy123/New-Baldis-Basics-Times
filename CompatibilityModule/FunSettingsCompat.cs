using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.NPCs;
using FunSettings;
using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace BBTimes.CompatibilityModule
{
	[HarmonyPatch]
	[ConditionalPatchMod("rost.moment.baldiplus.funsettings")]
	internal static class FunSettingsCompat
	{
		[HarmonyTargetMethod]
		static MethodInfo GiveMeQuantumSetting() =>
			AccessTools.Method("FunSettings.QuantumSweepFunSetting:OnNPCSpawn");

		[HarmonyPostfix]
		static void QuantumNpcSpawnPatch(NPC npc)
		{
			if (npc is ZeroPrize prize)
			{
				prize.speed = 250f;
				prize.minActive = int.MaxValue;
				prize.maxActive = int.MaxValue;
				prize.minWait = 1f;
				prize.maxWait = 1f;
				return;
			}

			if (npc is CoolMop coolMop)
			{
				coolMop.speed = 250f;
				coolMop.minActive = int.MaxValue;
				coolMop.maxActive = int.MaxValue;
				coolMop.minWait = 1f;
				coolMop.maxWait = 1f;
				return;
			}

			if (npc is Mopliss mopliss)
			{
				mopliss.speed = 125f;
				mopliss.minWait = 1f;
				mopliss.maxWait = 1f;
				return;
			}
		}

		[HarmonyTargetMethod]
		static MethodInfo GiveMeHardSetting() =>
			AccessTools.Method("FunSettings.HardModeFunSetting:OnNPCSpawn");

		[HarmonyPostfix]
		static void HardNpcSpawnPatch(NPC npc)
		{
			if (npc is ZeroPrize prize)
			{
				prize.moveModMultiplier = 0.99f;
				return;
			}

			if (npc is CoolMop coolMop)
			{
				coolMop.slipsPerTile = 15;
				coolMop.slipDropCooldown = 0.5f;
				return;
			}

			if (npc is Mopliss mopliss)
			{
				mopliss.roomsPerActivation = mopliss.ec.rooms.Count;
				mopliss.slipperRadius = 12;
				return;
			}
		}

		//[HarmonyPatch(typeof(Mopliss), "Initialize")]
		//[HarmonyPrefix]
		//static void QuantumMopliss(EnvironmentController ___ec, ref int ___roomsPerActivation, ref int ___slipperRadius, ref float ___speed, ref float ___minWait, ref float ___maxWait)
		//{
		//	if (FunSettingsType.HardMode.IsActive())
		//	{
		//		___roomsPerActivation = ___ec.rooms.Count;
		//		___slipperRadius = 12;
		//	}

		//	if (FunSettingsType.QuantumSweep.IsActive())
		//	{
		//		___speed = 125f;
		//		___minWait = 1f;
		//		___maxWait = 1f;
		//	}
		//}
	}
	/* Not needed anymore */
	//[HarmonyPatch]
	//[ConditionalPatchModByVersion("rost.moment.baldiplus.extramod", "2.1.9.5", includePostVersions:true, invertCondition:true)]
	//internal static class BBExtraCompat_KulakOldFix
	//{
	//	[HarmonyPatch(typeof(Kulak_Angry), "OnStateTriggerStay")]
	//	[HarmonyPatch(typeof(Kulak_Wandering), "OnStateTriggerStay")]
	//	[HarmonyTranspiler]
	//	static IEnumerable<CodeInstruction> KulakDontBreakUnbreakable(IEnumerable<CodeInstruction> i) =>
	//		new CodeMatcher(i)
	//		.MatchForward(true,
	//			new(OpCodes.Nop),
	//			new(OpCodes.Ldarg_1),
	//			new(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "GetComponent", [], [typeof(Window)])),
	//			new(CodeInstruction.LoadField(typeof(Window), "broken")),
	//			new(OpCodes.Ldc_I4_0),
	//			new(OpCodes.Ceq),
	//			new(OpCodes.Stloc_1)
	//			)
	//		.Advance(1)
	//		.InsertAndAdvance(
	//			new(OpCodes.Ldloc_1),
	//			new(OpCodes.Ldarg_1),
	//			new(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "GetComponent", [], [typeof(CustomWindowComponent)])),
	//			new(Transpilers.EmitDelegate<Func<bool, CustomWindowComponent, bool>>((loc, win) =>
	//			{
	//				if (!win || !loc) return false; // If it's already false, it means it is broken
	//				return !win.unbreakable; // Otherwise, if the window is unbreakable, return the inverse
	//			})),
	//			new(OpCodes.Stloc_1)
	//			)
	//		.InstructionEnumeration();
	//}
}
