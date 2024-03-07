using HarmonyLib;
using System.Collections.Generic;

namespace BBTimes.ModPatches
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
				if (data.LimitToRoomType != RoomType.Null && list[i].room.type != data.LimitToRoomType)
				{
					list.RemoveAt(i);
					i--;
				}
			}
			if (!data.Persistent)
				data = default;
		}
		public static void ResetData() => data = default;

		public static FindPathData data = default;
	}

	public readonly struct FindPathData(RoomType limitToRoomType = RoomType.Null, bool persistent = false)
	{
		public readonly RoomType LimitToRoomType = limitToRoomType;

		public readonly bool Persistent = persistent;
	}
}
