using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.ModPatches.EnvironmentPatches;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(LevelGenerator))]
	public class PostRoomCreation
	{


		[HarmonyPatch("StartGenerate")]
		private static void Prefix(LevelGenerator __instance) =>
			i = __instance;


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
			.InsertAndAdvance(Transpilers.EmitDelegate(ExecutePostRoomTasks))

			.InstructionEnumeration();

		public static LevelGenerator i; // Note: this is gonna be the main generator patch to get this variable

		static void ExecutePostRoomTasks()
		{
			if (i == null) return;

			spawnedWindows.Clear();
			if (!(bool)BBTimesManager.plug.disableOutside.BoxedValue)
				WindowsPointingOutside();
		}

		static void WindowsPointingOutside()
		{
			var ec = i.Ec;
			Dictionary<Cell, Direction[]> tiles = [];
			foreach (var t in ec.mainHall.GetNewTileList())
			{
				if (t.Hidden || t.offLimits || !t.HasFreeWall) // No elevator tiles or invalid tiles
					continue;
				// A quick fix for the walls


				var dirs = Directions.All();
				dirs.RemoveAll(x => !ec.CellFromPosition(t.position + x.ToIntVector2()).Null || t.WallSoftCovered(x));

				if (dirs.Count > 0)
					tiles.Add(t, [.. dirs]);
				i.FrameShouldEnd(); // fail safe to not crash for no f reason
			}

			if (tiles.Count == 0)
				return;

			

			foreach (var tile in tiles)
			{
				if (i.controlledRNG.NextDouble() >= 0.95f)
				{
					var dir = tile.Value[i.controlledRNG.Next(tile.Value.Length)];
					var w = ec.ForceBuildWindow(tile.Key, dir, window);
					if (w != null)
					{
						w.aTile.AddRenderer(w.windows[0]); // A small optimization
						spawnedWindows.Add(w);
					}
				}
				i.FrameShouldEnd();
			}

		}

		public static WindowObject window;
		internal static List<Window> spawnedWindows = [];

	}
}
