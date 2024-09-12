
namespace BBTimes.Extensions
{
	public static class RoomExtensions
	{
		public static void MysteryRoomCover(this RoomAsset asset)
		{
			for (int i = 0; i < asset.cells.Count; i++)
				asset.blockedWallCells.Add(asset.cells[i].pos);
			
			asset.entitySafeCells.Clear();
			asset.eventSafeCells.Clear();
			asset.windowChance = 0;
		}
	}
}
