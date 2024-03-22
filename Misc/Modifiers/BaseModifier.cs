using System;

namespace BBTimes.Misc.Modifiers
{
    public class BaseModifier(float modMultiplier = 1f) // Feels like just a float held by reference
    {
        [UnityEngine.SerializeField]
        public float Mod = modMultiplier;
        public override string ToString() =>
            $"{Mod}";
        public override int GetHashCode() =>
            Mod.GetHashCode();
    }
}
