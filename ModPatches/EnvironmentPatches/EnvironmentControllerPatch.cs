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
        private static void FixTiles(ref List<Cell> list) // A very *kinda invasive* specific patch to allow nav neighbors to search avoiding unwanted spots
        {
			if (shapes == null) // it can't be null, right?
				return;

            for (int i = 0; i < list.Count; i++)
            {
                if ((limits.Length != 0 && !limits.Contains(list[i].room.type)) || (shapes.Length != 0 && shapes.Contains(list[i].shape)))
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
			if (!persistentData)
				ResetData();
        }
		public static void SetNewData(TileShape[] shapes, RoomType[] limitToRoomTypes, bool persistent)
		{
			EnvironmentControllerPatch.shapes = shapes;
			limits = limitToRoomTypes;
			persistentData = persistent;
		}
		public static void ResetData()
		{
			shapes = null;
			limits = null;
			persistentData = false;
		}

		static TileShape[] shapes = null;
		static RoomType[] limits = null;
		static bool persistentData = false;


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
}
