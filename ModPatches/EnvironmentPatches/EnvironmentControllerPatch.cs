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
			if (data.shapes == null) // it can't be null, right?
				return;

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
		public static void ResetData() => data = new(null, null, false);

        public static FindPathData data = new([], [], false);


		[HarmonyPatch("InitializeLighting")]
		[HarmonyPostfix]
		private static void FixLighting(EnvironmentController __instance)
		{
			int maxX = Singleton<CoreGameManager>.Instance.lightMapTexture.width;
			int maxZ = Singleton<CoreGameManager>.Instance.lightMapTexture.height;
			for (int x = 0; x < maxX; x++)
			{
				for (int z = 0; z < maxZ; z++)
				{
					if (__instance.ContainsCoordinates(x, z) || Singleton<CoreGameManager>.Instance.lightMapTexture.GetPixel(x, z).a <= 0.1f)
						continue;

					Singleton<CoreGameManager>.Instance.UpdateLighting(__instance.standardDarkLevel, new(x, z)); // Should fix the red lighting appearing in earlier floors after beating F3

				}
			}
		}
    }

    public readonly struct FindPathData(TileShape[] shapesToExclude, RoomType[] limitToRoomTypes, bool persistent = false)
    {
        public readonly RoomType[] LimitToRoomTypes = limitToRoomTypes;

        public readonly TileShape[] shapes = shapesToExclude;

        public readonly bool Persistent = persistent;
    }
}
