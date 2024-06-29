using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class ShowItemsInTheEnd : RoomFunction
	{
		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			foreach (var item in room.objectObject.transform.AllChilds())
			{
				var pick = item.GetComponent<Pickup>();
				if (pick)
					itemsToShowUp.Add(pick);
			}
			HideItems(true);
		}

		void Update()
		{
			if (end) return;

			if (Singleton<BaseGameManager>.Instance.allNotebooksFound)
			{
				end = true;
				HideItems(false);
			}
		}

		void HideItems(bool hide)
		{
			for (int i = 0; i < itemsToShowUp.Count; i++)
			{
				itemsToShowUp[i].gameObject.SetActive(!hide);
				itemsToShowUp[i].icon.gameObject.SetActive(!hide);
			}
		}

		bool end = false;

		readonly List<Pickup> itemsToShowUp = [];
	}
}
