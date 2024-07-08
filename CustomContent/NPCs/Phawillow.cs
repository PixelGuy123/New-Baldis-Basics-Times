using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Phawillow : NPC, IClickable<int>
	{
		public override void Initialize()
		{
			base.Initialize();
			rendererPos = floatingRenderer.localPosition;
			behaviorStateMachine.ChangeState(new Phawillow_Wandering(this, null));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			floatingRenderer.localPosition = rendererPos + Vector3.up * Mathf.Cos(Time.fixedTime * TimeScale * 0.5f * navigator.speed) * 0.88f;
			itemRenderHolder.transform.localPosition = floatingRenderer.localPosition;
		}

		public void PlayWander() =>
			audMan.PlaySingle(audWander);

		public void SetSpeed(bool fleeing)
		{
			navigator.maxSpeed = fleeing ? fleeSpeed : normSpeed;
			navigator.SetSpeed(fleeing ? fleeSpeed : normSpeed);
		}

		public void UpdateRenderer(ItemObject item)
		{
			holdingItem = item;
			itemRender.sprite = item.itemSpriteLarge;
		}

		public void Clicked(int player) 
		{
			if (holdingItem == null) return;

			Singleton<CoreGameManager>.Instance.GetPlayer(player).itm.AddItem(holdingItem);
			holdingItem = null;
			itemRender.sprite = null;
			behaviorStateMachine.ChangeState(new Phawillow_Wandering(this, null));
		}
		public void ClickableUnsighted(int player) { }
		public void ClickableSighted(int player) { }
		public bool ClickableRequiresNormalHeight() => true;
		public bool ClickableHidden() => holdingItem == null;


		Vector3 rendererPos;

		[SerializeField]
		internal Transform floatingRenderer, itemRenderHolder;

		[SerializeField]
		internal SpriteRenderer itemRender;

		[SerializeField]
		internal SoundObject audWander;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		ItemObject holdingItem;

		const float normSpeed = 10f, fleeSpeed = 25f;
	}

	internal class Phawillow_StateBase(Phawillow wi) : NpcState(wi)
	{
		protected Phawillow wi = wi;

		float wander = Random.Range(10f, 20f);
		public override void Update()
		{
			base.Update();
			wander -= wi.TimeScale * Time.deltaTime;
			if (wander <= 0f)
			{
				wander += Random.Range(10f, 20f); ;
				if (Random.value > 0.6f)
					wi.PlayWander();
			}
		}
	}

	internal class Phawillow_Wandering(Phawillow wi, ItemObject prevItem) : Phawillow_StateBase(wi)
	{
		float wanderCooldown = 15f;

		NavigationState_WanderRandom wander;
		public override void Enter()
		{
			base.Enter();
			wander = new(wi, 0);
			ChangeNavigationState(wander);

			wi.SetSpeed(false);
		}

		public override void Update()
		{
			base.Update();
			wanderCooldown -= wi.TimeScale * Time.deltaTime;
			if (wanderCooldown <= 0f)
				wi.behaviorStateMachine.ChangeState(new Phawillow_TargetItem(wi, this, prevItem));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (prevItem == null || player.Tagged) return;

			wi.behaviorStateMachine.ChangeState(new Phawillow_FleeFromPlayer(wi, this, player));
		}
	}

	internal class Phawillow_FleeFromPlayer(Phawillow wi, Phawillow_StateBase prevState, PlayerManager pm) : Phawillow_StateBase(wi)
	{
		float cooldown = 20f;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderFlee(wi, 63, pm.DijkstraMap));
			wi.SetSpeed(true);
		}

		public override void Update()
		{
			base.Update();
			cooldown -= wi.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				wi.behaviorStateMachine.ChangeState(prevState);
		}
	}

	internal class Phawillow_TargetItem(Phawillow wi, Phawillow_StateBase prevState, ItemObject previousItem) : Phawillow_StateBase(wi)
	{
		NavigationState_TargetPosition target;
		Pickup pickup;
		public override void Enter()
		{
			base.Enter();

			List<Pickup> pickups = new(wi.ec.items);
			if (previousItem == null)
			{
				for (int i = 0; i < pickups.Count; i++)
					if (!pickups[i].gameObject.activeSelf)
						pickups.RemoveAt(i--);
			}

			if (pickups.Count == 0)
			{
				wi.behaviorStateMachine.ChangeState(prevState);
				return;
			}

			pickup = pickups[Random.Range(0, pickups.Count)];
			target = new(wi, 64, pickup.transform.position.ZeroOutY());
			ChangeNavigationState(target);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if ((pickup.transform.position - wi.transform.position).magnitude <= 5f)
			{
				wi.UpdateRenderer(pickup.item);
				wi.behaviorStateMachine.ChangeState(new Phawillow_Wandering(wi, pickup.item));

				if (previousItem == null)
				{
					pickup.gameObject.SetActive(false);
					if (pickup.icon != null)
						pickup.icon.spriteRenderer.enabled = false;
				}
				else
				{
					pickup.AssignItem(previousItem);
					pickup.gameObject.SetActive(true);
					if (pickup.icon != null)
						pickup.icon.spriteRenderer.enabled = true;
				}
				return;
			}
			ChangeNavigationState(target);
		}

		public override void Update()
		{
			base.Update();
			if (!pickup.gameObject.activeSelf)
				wi.behaviorStateMachine.ChangeState(prevState);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (previousItem == null || player.Tagged) return;

			wi.behaviorStateMachine.ChangeState(new Phawillow_FleeFromPlayer(wi, this, player));
		}

	}


}
