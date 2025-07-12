using HarmonyLib;

[HarmonyPatch(typeof(PickupBobValue), "Update")]
internal static class PickupBobValuePatch
{
    static void Prefix(PickupBobValue __instance) =>
        activeBob = __instance;

    public static PickupBobValue activeBob;
}