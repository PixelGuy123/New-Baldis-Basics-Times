﻿using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Phawillow : NPC, IClickable<int>, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			audWander = this.GetSound("breathing.wav", "Vfx_Phawillow_Wandering", SoundType.Voice, new(0.84705f, 0.84705f, 0.84705f));
			audLaugh = this.GetSound("Phawillow_Laughing.wav", "Vfx_Phawillow_Laught", SoundType.Voice, new(0.84705f, 0.84705f, 0.84705f));
			audRestart = this.GetSound("Phawillow_Restarting.wav", "Vfx_Phawillow_Restart", SoundType.Voice, new(0.84705f, 0.84705f, 0.84705f));
			floatingRenderer = spriteRenderer[0];

			var itemHolder = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(out var itmRenderer, new Vector3(3f, -0.8f, 0f), 0);
			itemHolder.transform.SetParent(transform);
			itemHolder.transform.localPosition = Vector3.zero;
			itemHolder.gameObject.AddComponent<BillboardRotator>();

			itemRender = itmRenderer;
			itemRenderHolder = itemHolder.transform;
			var storedSprites = this.GetSpriteSheet(3, 1, 22f, "phawillowSpritesheet.png");
			spriteRenderer[0].sprite = storedSprites[0];
			sprNormal = storedSprites[0];
			sprSplashed = storedSprites[1];
			sprActive = storedSprites[2];

			var myCol = (CapsuleCollider)baseTrigger[0];
			var col = this.CreateClickableLink().gameObject.AddComponent<CapsuleCollider>();
			col.isTrigger = true;
			col.height = myCol.height;
			col.direction = myCol.direction;
			col.radius = myCol.radius;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			rendererPos = floatingRenderer.transform.localPosition;
			behaviorStateMachine.ChangeState(new Phawillow_Wandering(this));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			floatingRenderer.transform.localPosition = rendererPos + (Vector3.up * Mathf.Cos(Time.fixedTime * TimeScale * 0.5f * navigator.speed) * 0.88f);
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
			itemRender.sprite = !item ? null : item.itemSpriteLarge;
		}

		public void Clicked(int player)
		{
			if (holdingItem == null) return;

			Singleton<CoreGameManager>.Instance.GetPlayer(player).itm.AddItem(holdingItem);
			holdingItem = null;
			itemRender.sprite = null;
			if (behaviorStateMachine.CurrentState is not Phawillow_Disable)
				behaviorStateMachine.ChangeState(new Phawillow_Wandering(this, delayAfterLosingItem));
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
		readonly MaterialPropertyBlock block = new();

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

		[SerializeField]
		internal float delayBeforeCatchAgain = 15f, delayAfterLosingItem = 3.5f;

		ItemObject holdingItem;

		const float normSpeed = 12.5f, fleeSpeed = 26f;
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

	internal class Phawillow_Wandering(Phawillow wi, float pickDelay = 0f) : Phawillow_StateBase(wi)
	{
		NavigationState_WanderRandom wander;
		float delay = pickDelay;
		public override void Enter()
		{
			base.Enter();
			wander = new(wi, 0);
			ChangeNavigationState(wander);

			wi.SetSpeed(false);
			wi.floatingRenderer.sprite = wi.sprNormal;
			wi.UpdateRenderer(null);
		}

		public override void Update()
		{
			base.Update();
			if (delay > 0f)
				delay -= wi.TimeScale * Time.deltaTime;
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);

			if (delay <= 0f && !player.Tagged && player.itm.HasItem())
				wi.behaviorStateMachine.ChangeState(new Phawillow_TargetPlayer(wi, player));
		}
	}

	internal class Phawillow_TargetPlayer(Phawillow wi, PlayerManager pm) : Phawillow_StateBase(wi)
	{
		NavigationState_TargetPlayer pTar;
		public override void Enter()
		{
			base.Enter();
			pTar = new(wi, 63, pm.transform.position);
			ChangeNavigationState(pTar);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == pm && player.itm.HasItem())
				pTar.UpdatePosition(player.transform.position);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == pm.gameObject)
				wi.behaviorStateMachine.ChangeState(new Phawillow_TargetItem
					(wi,
					pm.itm.RemoveRandomItemAndReturnIt()
					));

		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			wi.behaviorStateMachine.ChangeState(new Phawillow_Wandering(wi));
		}

		public override void Exit()
		{
			base.Exit();
			pTar.priority = 0;
		}
	}

	internal class Phawillow_FleeFromPlayer(Phawillow wi, NpcState prevState, PlayerManager pm) : Phawillow_StateBase(wi)
	{
		float escapeCooldown = 5f;
		bool samePlayerInSight = false;
		NavigationState_WanderFlee fleeNav;

		public override void Enter()
		{
			base.Enter();
			fleeNav = new(wi, 63, pm.DijkstraMap);
			ChangeNavigationState(fleeNav);
			wi.SetSpeed(true);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (player == pm)
				samePlayerInSight = false;
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (player == pm)
			{
				samePlayerInSight = true;
				escapeCooldown = 5f;
			}
		}

		public override void Update()
		{
			base.Update();
			if (!samePlayerInSight)
			{
				if (escapeCooldown > 0f)
					escapeCooldown -= wi.TimeScale * Time.deltaTime;
				else
					wi.behaviorStateMachine.ChangeState(prevState);
			}
		}
		public override void Exit()
		{
			base.Exit();
			fleeNav.priority = 0;
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

	internal class Phawillow_WanderScared(Phawillow wi, Phawillow_TargetItem tarItemPrev) : Phawillow_StateBase(wi)
	{
		float checkForPickupsDelay = 15f;
		public override void Enter()
		{
			base.Enter();
			wi.SetSpeed(false);
			ChangeNavigationState(new NavigationState_WanderRandom(wi, 0));
		}

		public override void Update()
		{
			base.Update();
			checkForPickupsDelay -= wi.TimeScale * Time.deltaTime;
			if (checkForPickupsDelay <= 0f)
			{
				var pickups = wi.ec.items;

				for (int i = 0; i < pickups.Count; i++)
				{
					if ((!pickups[i].gameObject.activeSelf || pickups[i].item.itemType == Items.None) && wi.ec.CellFromPosition(pickups[i].transform.position).room.category == RoomCategory.Store)
					{
						wi.behaviorStateMachine.ChangeState(tarItemPrev);
						return;
					}
				}

				checkForPickupsDelay += 15f;
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player.Tagged) return;

			wi.behaviorStateMachine.ChangeState(new Phawillow_FleeFromPlayer(wi, this, player));
		}
	}

	internal class Phawillow_TargetItem(Phawillow wi, ItemObject previousItem) : Phawillow_StateBase(wi)
	// Note for this christmas update:
	/*
	 * Tweaked the code a bit, so instead of targetting active pickups, he'll go for any disabled pickups (those that the player just picked items on).
	 * */
	{
		NavigationState_TargetPosition target;
		Pickup pickup;
		public override void Initialize()
		{
			base.Initialize();
			wi.audMan.FlushQueue(true);
			wi.audMan.QueueAudio(wi.audLaugh); // Will laugh once he steals the item
		}

		public override void Enter()
		{
			base.Enter();
			wi.UpdateRenderer(previousItem);
			wi.SetSpeed(false);

			List<Pickup> pickups = new(wi.ec.items);

			for (int i = 0; i < pickups.Count; i++)
			{
				if (pickups[i].gameObject.activeSelf || pickups[i].item.itemType != Items.None || wi.ec.CellFromPosition(pickups[i].transform.position).room.category == RoomCategory.Store)
					pickups.RemoveAt(i--);
				else
				{
					wi.ec.FindPath(wi.ec.CellFromPosition(wi.transform.position), wi.ec.CellFromPosition(pickups[i].transform.position), PathType.Nav, out _, out bool success);
					if (!success)
						pickups.RemoveAt(i--);
				}
			}


			if (pickups.Count == 0)
			{
				wi.behaviorStateMachine.ChangeState(new Phawillow_WanderScared(wi, this));
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
				wi.behaviorStateMachine.ChangeState(new Phawillow_Wandering(wi, wi.delayBeforeCatchAgain));

				pickup.AssignItem(previousItem);
				pickup.gameObject.SetActive(true);
				if (pickup.icon != null)
					pickup.icon.spriteRenderer.enabled = true;

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
			if (pickup && (pickup.gameObject.activeSelf || pickup.item.itemType != Items.None))
				wi.behaviorStateMachine.ChangeState(new Phawillow_WanderScared(wi, this));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player.Tagged) return;

			wi.behaviorStateMachine.ChangeState(new Phawillow_FleeFromPlayer(wi, this, player));
		}

		public override void Exit()
		{
			base.Exit();
			if (target != null)
				target.priority = 0;
		}

	}


}
