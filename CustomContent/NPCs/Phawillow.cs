using BBTimes.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Phawillow : NPC, IClickable<int>
	{
		public override void Initialize()
		{
			base.Initialize();
			rendererPos = floatingRenderer.transform.localPosition;
			behaviorStateMachine.ChangeState(new Phawillow_Wandering(this, null));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			floatingRenderer.transform.localPosition = rendererPos + Vector3.up * Mathf.Cos(Time.fixedTime * TimeScale * 0.5f * navigator.speed) * 0.88f;
			itemRenderHolder.transform.localPosition = floatingRenderer.transform.localPosition;
			itemRender.transform.localPosition = floatingRenderer.GetRotationalPosFrom(new(3f, -0.8f));
			floatingRenderer.GetPropertyBlock(block);
			itemRender.SetSpriteRotation(block.GetFloat("_SpriteRotation"));
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
			if (behaviorStateMachine.CurrentState is not Phawillow_Disable)
				behaviorStateMachine.ChangeState(new Phawillow_Wandering(this, null));
		}
		public void ClickableUnsighted(int player) { }
		public void ClickableSighted(int player) { }
		public bool ClickableRequiresNormalHeight() => true;
		public bool ClickableHidden() => holdingItem == null;

		public override void VirtualOnTriggerEnter(Collider other)
		{
			base.VirtualOnTriggerEnter(other);
			if (other.isTrigger && !other.CompareTag("Player") && !other.CompareTag("NPC"))
			{
				if (other.GetComponent<Entity>())
				{
					behaviorStateMachine.ChangeState(new Phawillow_Disable(this, behaviorStateMachine.CurrentState));
				}
			}

			
		}


		Vector3 rendererPos;
		MaterialPropertyBlock block = new();

		[SerializeField]
		internal Transform itemRenderHolder;

		[SerializeField]
		internal SpriteRenderer floatingRenderer, itemRender;

		[SerializeField]
		internal Sprite sprNormal, sprActive, sprSplashed;

		[SerializeField]
		internal SoundObject audWander, audRestart, audLaugh;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		ItemObject holdingItem;

		const float normSpeed = 10f, fleeSpeed = 18f;
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

	internal class Phawillow_SubStateBase(Phawillow wi) : NpcState(wi)
	{
		protected Phawillow wi = wi;
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
			wi.floatingRenderer.sprite = !prevItem || prevItem.itemType == Items.None ? wi.sprNormal : wi.sprActive;
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
			if (!prevItem || player.Tagged) return;

			wi.behaviorStateMachine.ChangeState(new Phawillow_FleeFromPlayer(wi, this, player));
		}
	}

	internal class Phawillow_FleeFromPlayer(Phawillow wi, NpcState prevState, PlayerManager pm) : Phawillow_StateBase(wi)
	{

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderFlee(wi, 63, pm.DijkstraMap));
			wi.SetSpeed(true);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (player == pm)
				wi.behaviorStateMachine.ChangeState(prevState);
		}
	}

	internal class Phawillow_Disable(Phawillow wi, NpcState prevState) : Phawillow_SubStateBase(wi)
	{
		float deadCooldown = 15f;
		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
		Sprite prevSpr;
		public override void Enter()
		{
			base.Enter();
			prevSpr = wi.floatingRenderer.sprite;
			wi.floatingRenderer.sprite = wi.sprSplashed;
			wi.Navigator.Am.moveMods.Add(moveMod);
			ChangeNavigationState(new NavigationState_DoNothing(wi, 0));
		}

		public override void Update()
		{
			base.Update();
			if (deadCooldown <= 0f)
			{
				if (!wi.audMan.QueuedAudioIsPlaying)
					wi.behaviorStateMachine.ChangeState(prevState);
				
				return;
			}

			deadCooldown -= wi.TimeScale * Time.deltaTime;
			if (deadCooldown <= 0f)
			{
				wi.audMan.FlushQueue(true);
				wi.audMan.QueueAudio(wi.audRestart);
			}
		}
		public override void Exit()
		{
			base.Exit();
			wi.Navigator.Am.moveMods.Remove(moveMod);
			wi.floatingRenderer.sprite = prevSpr;
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
				{
					if (!pickups[i].gameObject.activeSelf || wi.ec.CellFromPosition(pickups[i].transform.position).room.category == RoomCategory.Store)
						pickups.RemoveAt(i--);
					else
					{
						wi.ec.FindPath(wi.ec.CellFromPosition(wi.transform.position), wi.ec.CellFromPosition(pickups[i].transform.position), PathType.Nav, out _, out bool success);
						if (!success)
							pickups.RemoveAt(i--);
					}
				}
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
				wi.audMan.FlushQueue(true);
				wi.audMan.QueueAudio(wi.audLaugh);
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
			wi.ec.FindPath(wi.ec.CellFromPosition(wi.transform.position), wi.ec.CellFromPosition(pickup.transform.position), PathType.Nav, out _, out bool success);
			if (!success)
			{
				Enter();
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
