using BBTimes.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(LevelGenerator))]
	public class WindowOutsidePatch
	{
		[HarmonyPatch("StartGenerate")]
		private static void Prefix(LevelGenerator __instance) => i = __instance;

		[HarmonyPatch("Generate", MethodType.Enumerator)]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => // Basically make windows spawn facing outside the school (REMINDER IT MUST BE UNBREAKABLE FOR AN OBVIOUS REASON)
			new CodeMatcher(instructions)
			.End()
			.MatchBack(false,
				new(OpCodes.Ldloc_2),
				new(CodeInstruction.LoadField(typeof(LevelBuilder), "ec")),
				new(OpCodes.Ldc_I4_1),
				new(OpCodes.Callvirt, AccessTools.Method("EnvironmentController:SetTileInstantiation")) // before setting tile instantiation on. First do some stuff
				)
			.InsertAndAdvance(Transpilers.EmitDelegate(WindowsPointingOutside))
			.InstructionEnumeration();

		static LevelGenerator i;



		static void WindowsPointingOutside()
		{
			artificallySpawnedWindows.Clear();

			var window = Resources.FindObjectsOfTypeAll<WindowObject>().Where(x => x.name == "WoodWindow").First(); // TEMPORARY METHOD, METAL WINDOW UNBREAK FEATURE IS ON THE WAY!!
			var ec = i.Ec;
			Dictionary<Cell, Direction[]> tiles = [];
			foreach (var t in ec.mainHall.GetNewTileList())
			{
				if (t.Hidden || t.offLimits) // No elevator tiles or invalid tiles
					continue;
				// A quick fix for the walls


				var dirs = Directions.All();
				dirs.RemoveAll(x => !ec.CellFromPosition(t.position + x.ToIntVector2()).Null || t.WallHardCovered(x));

				if (dirs.Count > 0)
					tiles.Add(t, [.. dirs]);
			}

			if (tiles.Count == 0)
				return;

			foreach (var tile in tiles)
			{
				if (i.controlledRNG.NextDouble() >= 0.6f)
				{
					var dir = tile.Value[i.controlledRNG.Next(tile.Value.Length)];
					ec.ForceBuildWindow(tile.Key, dir, window);
					artificallySpawnedWindows.Add(tile.Key.position, dir);
				}
			}

		}

		internal readonly static Dictionary<IntVector2, Direction> artificallySpawnedWindows = [];
	}
}
