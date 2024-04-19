using HarmonyLib;

namespace BBTimes.ModPatches.GeneratorPatches
{
	[HarmonyPatch(typeof(LevelBuilder), "CreateElevator")]
	internal class CreateElevatorPatch
	{
		private static void Postfix(EnvironmentController ___ec, IntVector2 pos) =>
			___ec.CellFromPosition(pos).HardCover(CellCoverage.Up); // Makes exit signs important to not be covered by anything else lol
	}
}
