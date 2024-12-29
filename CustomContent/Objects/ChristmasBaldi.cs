using BBTimes.Extensions;
using System.Collections;
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
				pickup.OnItemCollected += CollectPresent;
			}

			func = ec.CellFromPosition(position).room.functionObject.GetComponent<StoreRoomFunction>();
		}

		public void SayMerryChristmas()
		{
			if (!merryChristmased)
			{
				merryChristmased = true;

				if (johnnyResponse != null)
					StopCoroutine(johnnyResponse);

				audMan.FlushQueue(true);
				audMan.QueueAudio(audBuyItem);
			}
		}

		void CollectPresent(Pickup p, int player)
		{
			p.free = true;
			p.price = 0;
			p.showDescription = false;
		}

		void BuyPresent(Pickup p, int player)
		{
			if (johnnyResponse != null)
				StopCoroutine(johnnyResponse);

			if (func)
				func.itemPurchased = true;

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBell);

			if (!audMan.QueuedAudioIsPlaying || audMan.IsPlayingClip(audIntro))
			{
				audMan.FlushQueue(true);
				audMan.QueueRandomAudio(audCollectingPresent);
			}
		}

		void DenyPresent(Pickup p, int player)
		{
			audMan.FlushQueue(true);

			if (johnnyResponse != null)
				StopCoroutine(johnnyResponse);

			if (!Singleton<CoreGameManager>.Instance.johnnyHelped && feelingGenerous && price - Singleton<CoreGameManager>.Instance.GetPoints(player) <= generousOffset)
			{
				feelingGenerous = false;
				audMan.QueueAudio(audGenerous);
				p.Collect(player);
				Singleton<CoreGameManager>.Instance.AddPoints(-Singleton<CoreGameManager>.Instance.GetPoints(player), player, true);
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBell);
				Singleton<CoreGameManager>.Instance.johnnyHelped = true;
				if (func)
				{
					func.itemPurchased = true;
					johnnyResponse = StartCoroutine(WaitForJohnnyToRespond());
				}
				return;
			}
			
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

		IEnumerator WaitForJohnnyToRespond()
		{
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			func.johnnyAudioManager.FlushQueue(true);
			func.johnnyAudioManager.QueueAudio(func.audHelp);
		}
		

		[SerializeField]
		internal int presents = 3, price = 100, generousOffset = 25;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audIntro, audNoYtps, audBuyItem, audGenerous, audBell;

		[SerializeField]
		internal SoundObject[] audCollectingPresent;

		[SerializeField]
		internal ItemObject present;

		StoreRoomFunction func;
		Coroutine johnnyResponse;
		readonly List<Pickup> generatedPickups = [];
		bool interactedWith = false, merryChristmased = false, feelingGenerous = true; // yes, I made this word up lol
	}
}
