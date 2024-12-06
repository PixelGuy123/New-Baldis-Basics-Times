using BBTimes.CustomComponents;

namespace BBTimes.CompatibilityModule.GrapplingHookTweaksCompats
{
    internal class GrapplingHookTweaksCompat
    {
        internal static void Loadup()
        {
            GrapplingHookTweaks.Plugin.GrapplingHookPatch.OnWindowBreak += (pm, w) =>
            {
                if (!w.GetComponent<CustomWindowComponent>()?.unbreakable ?? true)
                    pm.RuleBreak("breakingproperty", 3f, 0.15f);
            };
        }
    }
}
