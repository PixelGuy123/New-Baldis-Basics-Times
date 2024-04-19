using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BBTimes.ModPatches.NpcPatches
{
    [HarmonyPatch(typeof(NPC))]
    internal class NPCPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("SentToDetention")]
        private static void StuckInDetention(NPC __instance)
        {
            IEnumerator WaitToLeave()
            {
				__instance.Navigator.Entity.SetFrozen(true);
                Vector3 ogPos = __instance.transform.position;
                float cooldown = 15f;
                while (cooldown > 0f && __instance.transform.position == ogPos)
                {
                    cooldown -= __instance.ec.EnvironmentTimeScale * Time.deltaTime;
                    yield return null;
                }

				__instance.Navigator.Entity.SetFrozen(false);
				yield break;
            }

            __instance.Navigator.Entity.StartCoroutine(WaitToLeave());
        }
    }
}
