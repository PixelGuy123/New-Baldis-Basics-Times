using BBTimes.Extensions.ObjectCreationExtensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace BBTimes.ModPatches.EnvironmentPatches
{
    [HarmonyPatch(typeof(EnvironmentController))]
    public class EnvironmentControllerPatch
    {

        [HarmonyPatch("GetNavNeighbors")]
        [HarmonyPostfix]
        private static void FixTiles(ref List<Cell> list)
        {
            for (int i = 0; i < list.Count; i++)
            {

                if (data.LimitToRoomTypes.Length > 0 && !data.LimitToRoomTypes.Contains(list[i].room.type) || data.shapes.Length > 0 && data.shapes.Contains(list[i].shape))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
            if (!data.Persistent)
                data = new([], [], false);
        }
        public static void ResetData() => data = new([], [], false);

        public static FindPathData data = new([], [], false);
    }

    public readonly struct FindPathData(TileShape[] shapesToExclude, RoomType[] limitToRoomTypes, bool persistent = false)
    {
        public readonly RoomType[] LimitToRoomTypes = limitToRoomTypes;

        public readonly TileShape[] shapes = shapesToExclude;

        public readonly bool Persistent = persistent;
    }
}
