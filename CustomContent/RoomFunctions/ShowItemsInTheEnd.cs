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
			targetNotebooks = Mathf.FloorToInt(Singleton<BaseGameManager>.Instance.NotebookTotal * notebookCheckFactor);
		}

		void Update()
		{
			if (end || targetNotebooks == -1) return;

			if (Singleton<BaseGameManager>.Instance.FoundNotebooks >= targetNotebooks)
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
		int targetNotebooks = -1;

		readonly List<Pickup> itemsToShowUp = [];

		[SerializeField]
		[Range(0.05f, 1f)]
		internal float notebookCheckFactor = 0.5f;
	}
}
