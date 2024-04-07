using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace BBTimes.ModPatches
{

    [HarmonyPatch(typeof(GameButton), "BuildInArea")]
    internal class GameButtonSpawnPatch
    {

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> i)
        {
            var m = new CodeMatcher(i)
            .MatchForward(false,
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ret)
                );

            var dir = m.Advance(-3).Operand;// should get direction local variable (I freaking hate about how I need to grab a local variable, there should be a better way...)
            return m.Advance(3).InsertAndAdvance(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldloc_S, dir),
                Transpilers.EmitDelegate<System.Action<EnvironmentController, GameButton, Direction>>((x, y, z) =>
                {
                    string name = z.ToString();
                    var i = x.map.AddIcon(butIconPre.First(x => x.name.EndsWith(name)), y.transform, UnityEngine.Color.white);
                    i.gameObject.SetActive(true);
                })
                )
            .InstructionEnumeration();
        }

        internal static MapIcon[] butIconPre;

    }
}

