using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class PosterAsideFromObject : RoomFunction
	{

		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			foreach (var obj in room.objectObject.transform.AllChilds())
			{
				if (obj.name.StartsWith(targetPrefabName))
				{
					var cell = room.ec.CellFromPosition(obj.transform.position);
					if (cell.AllWallDirections.Count != 0)
						room.ec.BuildPoster(posterPre, cell, cell.AllWallDirections[0]);
				}
			}
		}

		[SerializeField]
		internal PosterObject posterPre;

		[SerializeField]
		internal string targetPrefabName = string.Empty;
	}
}
