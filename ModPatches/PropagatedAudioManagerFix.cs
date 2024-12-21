using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(PropagatedAudioManager))]
	internal class PropagatedAudioManagerFix
	{
		[HarmonyPatch("VirtualAwake")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> AvoidDestroyingDevice(IEnumerable<CodeInstruction> i) => // This is to avoid the Destroy() issue if an audio device already exists due to update cycles
			new CodeMatcher(i)
			.MatchForward(
				false,
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.LoadField(typeof(AudioManager), "audioDevice")),
				new(CodeInstruction.Call(typeof(Object), "Destroy", [typeof(Object)]))
				)
			.SetInstructionAndAdvance(new(OpCodes.Nop))
			.SetInstructionAndAdvance(new(OpCodes.Nop))
			.SetInstructionAndAdvance(new(OpCodes.Nop))
			.InstructionEnumeration();
	}
}
