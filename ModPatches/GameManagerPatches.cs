using BBTimes.CustomComponents;
using BBTimes.Manager;
using BBTimes.ModPatches.GeneratorPatches;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(MainGameManager))]
	internal class MainGameManagerPatches
	{
		[HarmonyPatch("BeginPlay")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> MusicChanges(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "school", "school"))
			.SetInstruction(Transpilers.EmitDelegate(() =>
			{
				var comp = Singleton<BaseGameManager>.Instance.GetComponent<MainGameManagerExtraComponent>();
				var rng = new System.Random(PostRoomCreation.i?.controlledRNG.Next() ?? 0);
				if (comp == null)
					return "school";

				int idx = rng.Next(comp.midis.Length + 1);
				if (idx >= comp.midis.Length) 
					return "school";

				return comp.midis[idx];
			}))
			.InstructionEnumeration();
	}

	[HarmonyPatch(typeof(EndlessGameManager))]

	internal class EndlessGameManagerPatches
	{
		[HarmonyPatch("BeginPlay")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> MusicChanges(IEnumerable<CodeInstruction> instructions) =>
			new CodeMatcher(instructions)
			.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "school", "school"))
			.SetInstruction(Transpilers.EmitDelegate(() =>
			{
				var midis = BBTimesManager.floorDatas[3].MidiFiles;
				var rng = new System.Random(PostRoomCreation.i?.controlledRNG.Next() ?? 0);
				if (midis.Count == 0) return "school"; // For some reason mthe MainGameManagerExtraComponent isn't added to random endless manager. So I'm manually selecting the floorData

				int idx = rng.Next(midis.Count + 1);
				if (idx >= midis.Count)
					return "school";

				return midis[rng.Next(midis.Count)];
			}))
			.InstructionEnumeration();
	}
}
