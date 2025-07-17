using BBTimes.Extensions;
using BBTimes.Plugin;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches;

[HarmonyPatch(typeof(PlayerMovement))]
static class PlayerMovementPatch
{
    [HarmonyPatch("StaminaUpdate"), HarmonyPrefix]
    static bool PreventPlayerRunning(PlayerMovement __instance) =>
        !__instance.pm.GetAttribute().HasAttribute(Storage.DRIBBLE_ATTR_PREVENT_RUNNING_TAG);
}