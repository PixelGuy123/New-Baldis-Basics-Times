using BBTimes.CustomContent.RoomFunctions;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(Structure_Vent))]
	static class VentBuilderPatch
	{
		[HarmonyPatch("Generate")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> FixVentInHighCeil(IEnumerable<CodeInstruction> i) =>
			new CodeMatcher(i)
			.MatchForward(true,
				new(OpCodes.Ldloc_3),
				new(OpCodes.Ldarg_0),
				new(CodeInstruction.LoadField(typeof(StructureBuilder), nameof(StructureBuilder.ec))),
				new(CodeInstruction.LoadField(typeof(EnvironmentController), nameof(EnvironmentController.rooms))),
				new(OpCodes.Callvirt, AccessTools.Method(typeof(List<RoomController>), "AddRange", [typeof(IEnumerable<RoomController>)])) // After the rooms have been added, basically
				)
			.Advance(1)
			.InsertAndAdvance(
				new(OpCodes.Ldloc_3),
				Transpilers.EmitDelegate((List<RoomController> rooms) => {
					for (int i = 0; i < rooms.Count; i++)
					{
						int idx = rooms[i].functions.functions.FindIndex(x => x is HighCeilingRoomFunction);
						if (idx != -1)
						{
							var func = (HighCeilingRoomFunction)rooms[i].functions.functions[idx];
							if (func.HasGeneratedCeiling) // If the ceiling has been generated, remove this room from the vent's spawn points
								rooms.RemoveAt(i--);
						}
							
					}
				})
				)
			.InstructionEnumeration();
    }
}
