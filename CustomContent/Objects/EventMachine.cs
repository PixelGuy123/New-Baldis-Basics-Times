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
			List<RandomEventType> types = new(ec.CurrentEventTypes);
			types.ForEach(x => ec.GetEvent(x).EndEarlier()); // guarantee every event is ended

			if (audBalAngry != null)
				Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).BaldiTv.Speak(audBalAngry[Random.Range(0, audBalAngry.Length)]);

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

		[SerializeField]
		internal SoundObject[] audBalAngry;

		public MapIcon mapIcon;

		bool _isDead = false;
		public static void AddItemToTrigger(Items item) => _itemsToAccept.Add(item);
		readonly static HashSet<Items> _itemsToAccept = [];
	}
}
