using UnityEngine;

namespace BBTimes.Misc
{
    public struct MinMax
    {
        public MinMax(int min, int max, System.Random rng = null)
        {
            if (min > max)
                throw new System.ArgumentException($"Min is higher than the max (Min: {min}, Max: {max})");

            Min = min;
            Max = max;
            this.rng = rng;
        }

        System.Random rng = null;
        public int Min = 0;
        public int Max = 1;

        public System.Random Rng { readonly get => rng; set => rng = value; }

        public readonly int RandomVal => rng == null ? Random.Range(Min, Max + 1) : rng.Next(Min, Max + 1);

        public static bool operator ==(MinMax mm1, MinMax mm2) =>
            mm1.Min == mm2.Min && mm1.Max == mm2.Max;

        public static bool operator !=(MinMax mm1, MinMax mm2) =>
            mm1.Min != mm2.Min || mm1.Max != mm2.Max;

        public readonly override bool Equals(object obj) =>
            obj is MinMax max1 && this == max1; // i didn't know this was vaid syntax!!

        public override int GetHashCode() =>
            Min.GetHashCode() ^ Max.GetHashCode();

        public readonly override string ToString() => $"{Min} >> {Max}";
    }
}
