using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class ChristmasBaldi : TileBasedObject, IClickable<int>
	{
		void Start()
		{
			var room = ec.CellFromPosition(position).room;
			Vector2 pos = new(transform.position.x, transform.position.z),
				offset = new Vector2(direction.ToVector3().x, direction.ToVector3().z) * 3.5f;

			for (int i = 0; i < presents; i++)
			{
				pos += offset;
				var pickup = ec.CreateItem(room, present, pos);
				pickup.showDescription = true;
				pickup.free = false;

				pickup.price = price;

				generatedPickups.Add(pickup);
				pickup.OnItemPurchased += BuyPresent;
				pickup.OnItemDenied += DenyPresent;
			}

			func = ec.CellFromPosition(position).room.functionObject.GetComponent<StoreRoomFunction>();
		}

		public void SayMerryChristmas()
		{
			if (!merryChristmased)
			{
				merryChristmased = true;

				audMan.FlushQueue(true);
				audMan.QueueAudio(audBuyItem);
			}
		}

		void BuyPresent(Pickup p, int player)
		{
			if (func)
				func.itemPurchased = true;

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBell);
		}

		void DenyPresent(Pickup p, int player)
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audNoYtps);
		}

		public void Clicked(int player)
		{
			if (interactedWith) return;

			interactedWith = true;
			audMan.FlushQueue(true);
			audMan.QueueAudio(audIntro);
		}

		public bool ClickableRequiresNormalHeight() => false;
		public bool ClickableHidden() => interactedWith;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		

		[SerializeField]
		internal int presents = 3, price = 100;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audIntro, audNoYtps, audBuyItem, audBell;

		[SerializeField]
		internal ItemObject present;

		StoreRoomFunction func;
		readonly List<Pickup> generatedPickups = [];
		bool interactedWith = false, merryChristmased = false; // yes, I made this word up lol
	}
}
