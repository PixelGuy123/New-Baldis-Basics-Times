using BBTimes.Manager;
using HarmonyLib;
using System.Collections.Generic;
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
			.RemoveInstructionsInRange(0, 4) // Removes the instructions
			.InstructionEnumeration();

		[HarmonyPatch(typeof(MathMachineNumber), "Pop")]
		private static void Prefix(ref Transform ___sprite) => ___sprite.GetComponent<SpriteRenderer>().sprite = explodeVisual; // YOU CAN HAVE TWO METHODS WITH THE SAME NAME LIKE THAT? OMG

		//[HarmonyPatch(typeof(MathMachineNumber), "Start")]
		//private static void Prefix(MathMachineNumber __instance) => __instance.gameObject.layer = BBTimesManager.; // Fix the layer being wrong since it is disabled

		internal static Sprite explodeVisual;
	}
}
