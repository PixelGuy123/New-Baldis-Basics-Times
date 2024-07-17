using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Misc
{
	public class JoeChef : EnvironmentObject, IClickable<int>
	{
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			positions = [transform.position - (transform.forward * 2f), 
				transform.position + ((transform.right * 16f) - (transform.forward * 2f)),
			transform.position + ((-transform.right * 16f) - (transform.forward * 2f))];
			ogPosition = transform.position;
			target = ogPosition;
		}

		public void Clicked(int player)
		{
			if (foodToGive != null && (transform.position - target).magnitude <= 3f)
			{
				Singleton<CoreGameManager>.Instance.GetPlayer(player).itm.AddItem(foodToGive);
				foodToGive = null;
				itemRenderer.sprite = null;
				return;
			}
			if (workingOn) return;
			workingOn = true;
			audMan.PlaySingle(audWelcome);
			kitchenAudMan.QueueAudio(audKitchen);
			kitchenAudMan.SetLoop(true);
			cooldown = Random.Range(20f, 40f);
		}

		public void ClickableUnsighted(int player) { }
		public void ClickableSighted(int player) { }
		public bool ClickableRequiresNormalHeight() => true;
		public bool ClickableHidden() => workingOn || (transform.position - target).magnitude > 3f;

		void Update()
		{
			if (workingOn)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				if (cooldown <= 0f)
				{
					kitchenAudMan.FlushQueue(true);
					audMan.PlaySingle(audScream);
					workingOn = false;
					target = ogPosition;
					foodToGive = WeightedItemObject.RandomSelection([.. foods]);
					itemRenderer.sprite = foodToGive.itemSpriteLarge;
				}
				else if ((transform.position - target).magnitude <= 3f)
					target = positions[Random.Range(0, positions.Length)];
			}

			transform.position = Vector3.SmoothDamp(transform.position, target, ref _velocity, 0.45f);
		}

		Vector3[] positions;
		Vector3 ogPosition;
		Vector3 target;
		Vector3 _velocity;
		ItemObject foodToGive = null;
		bool workingOn = false;
		float cooldown = 0f;

		[SerializeField]
		internal PropagatedAudioManager audMan, kitchenAudMan;

		[SerializeField]
		internal SoundObject audWelcome, audScream, audKitchen;

		[SerializeField]
		internal SpriteRenderer itemRenderer;

		readonly static List<WeightedItemObject> foods = [];

		public static void AddFood(ItemObject obj, int weight) =>
			foods.Add(new() { selection = obj, weight = weight });
	}
}
