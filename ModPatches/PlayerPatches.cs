using BBTimes.Extensions;
using BBTimes.Plugin;
using HarmonyLib;

namespace BBTimes.ModPatches;

[HarmonyPatch(typeof(PlayerMovement))]
static class PlayerMovementPatch
{
    [HarmonyPatch("StaminaUpdate"), HarmonyPrefix]
    static bool PreventPlayerRunning(PlayerMovement __instance)
    {
        // Whether the player can run or not
        if (__instance.pm.GetAttribute().HasAttribute(Storage.ATTR_FREEZE_STAMINA_UPDATE_TAG))
        {
            Singleton<CoreGameManager>.Instance.GetHud(__instance.pm.playerNumber).SetStaminaValue(__instance.stamina / __instance.staminaMax);
            return false;
        }
        return true;
    }

    [HarmonyPatch("PlayerMove"), HarmonyPrefix]
    static bool PreventPlayerMoving(PlayerMovement __instance)
    {
        if (__instance.pm.GetAttribute().HasAttribute(Storage.ATTR_FREEZE_PLAYER_MOVEMENT_TAG))
        {
            __instance.StaminaUpdate(0f);
            return false;
        }
        return true;
    }
}