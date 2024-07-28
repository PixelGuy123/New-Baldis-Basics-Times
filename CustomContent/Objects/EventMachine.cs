using BBTimes.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class EventMachine : EnvironmentObject, IItemAcceptor
	{
		public bool ItemFits(Items item) =>
			!_isDead && ec.CurrentEventTypes.Count != 0 && _itemsToAccept.Contains(item);

		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			while (ec.CurrentEventTypes.Count != 0)
				ec.GetEvent(ec.CurrentEventTypes[0]).EndEarlier();
			_isDead = true;
			spriteToChange.sprite = sprDead;
			if (mapIcon)
				mapIcon.gameObject.SetActive(false);
		}

		void Update()
		{
			if (!_isDead)
				spriteToChange.sprite = ec.CurrentEventTypes.Count != 0 ? sprWorking : sprNoEvents;
		}
		

		[SerializeField]
		internal Sprite sprNoEvents, sprWorking, sprDead;

		[SerializeField]
		internal SpriteRenderer spriteToChange;

		public MapIcon mapIcon;

		bool _isDead = false;
		public static void AddItemToTrigger(Items item) => _itemsToAccept.Add(item);
		readonly static HashSet<Items> _itemsToAccept = [];
	}
}
