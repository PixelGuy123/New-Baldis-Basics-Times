using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch]
	internal class BalloonAndNumberBalloonPatch
	{
		[HarmonyPatch(typeof(Balloon), "Initialize")]
		private static void Prefix(Balloon __instance) =>
			__instance.gameObject.SetActive(true); // The only thing literally
		

		[HarmonyPatch(typeof(MathMachineNumber), "Pop")]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => // Removes the gameObject.SetActive(false) instruction
			new CodeMatcher(instructions)
			.MatchForward(false, 
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.LoadField(typeof(MathMachineNumber), "sprite")),
				new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), "gameObject")), // Component has the gameObject thingy, bruh
				new(OpCodes.Ldc_I4_0),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), "SetActive", [typeof(bool)]))
				)

			.RemoveInstructions(5)
			.InstructionEnumeration();

		[HarmonyPatch(typeof(MathMachineNumber), "Pop")]
		private static void Prefix(ref Transform ___sprite, bool ___popping)
		{
			if(!___popping)
				___sprite.GetComponent<SpriteRenderer>().sprite = explodeVisual; // YOU CAN HAVE TWO METHODS WITH THE SAME NAME LIKE THAT? OMG
		}

		//[HarmonyPatch(typeof(MathMachineNumber), "Start")]
		//private static void Prefix(MathMachineNumber __instance) => __instance.gameObject.layer = BBTimesManager.; // Fix the layer being wrong since it is disabled

		internal static Sprite explodeVisual;
	}
}
