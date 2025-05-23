using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{

    [HarmonyPatch]
    internal class GameButtonSpawnPatch
    {
        [HarmonyTargetMethod]
        static MethodBase GetGameButtonMethod() =>
        AccessTools.Method(typeof(GameButton), nameof(GameButton.BuildInArea), [typeof(EnvironmentController), typeof(IntVector2), typeof(int), typeof(GameObject), typeof(GameButtonBase), typeof(System.Random), typeof(bool).MakeByRefType()]);

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> i)
        {
            var m = new CodeMatcher(i)
            .MatchForward(true,
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldarg_S, name: "cRng"),
                new(OpCodes.Callvirt, AccessTools.Method(typeof(Cell), nameof(Cell.RandomUncoveredDirection), [typeof(System.Random)])),
                new(OpCodes.Stloc_S, name: "V_4")
                );

            var dir = m.Operand;// should get direction local variable (I freaking hate about how I need to grab a local variable, there should be a better way...)
            return m.Advance(1).InsertAndAdvance(
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldloc_S, dir),
                Transpilers.EmitDelegate<System.Action<EnvironmentController, GameButton, Direction>>((x, y, z) =>
                {
                    string name = z.ToString();
                    x.map.AddIcon(butIconPre.First(x => x.name.EndsWith(name)), y.transform, Color.white);
                })
                )
            .InstructionEnumeration();
        }

        internal static MapIcon[] butIconPre;

    }
}

