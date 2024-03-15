using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BBTimes.ModPatches.NpcPatches
{
    [HarmonyPatch(typeof(NPC))]
    internal class NPCPatches
    {
        [HarmonyReversePatch] // Yes this is almost a copy to the Harmony docs for Reverse Patch
        [HarmonyPatch("SetGuilt")]
        internal static void SetGuilt(object instance, float time, string rule) =>
            throw new System.Exception(); // Stub, so doesn't run this, but the og method

        [HarmonyPrefix]
        [HarmonyPatch("SentToDetention")]
        private static void StuckInDetention(NPC __instance)
        {
            IEnumerator WaitToLeave()
            {
                __instance.Navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
                Vector3 ogPos = __instance.transform.position;
                float cooldown = 15f;
                while (cooldown > 0f && __instance.transform.position == ogPos)
                {
                    cooldown -= __instance.ec.EnvironmentTimeScale * Time.deltaTime;
                    yield return null;
                }

                __instance.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
                yield break;
            }

            __instance.Navigator.Entity.StartCoroutine(WaitToLeave());
        }

        readonly static MovementModifier moveMod = new(Vector3.zero, 0f);
    }
}
