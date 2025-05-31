using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.Extensions;
using BBTimes.Plugin;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class PencilBoy : NPC, INPCPrefab, IItemAcceptor
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [this.GetSound("PB_Angry0.wav", "Vfx_PB_Wander1", SoundType.Voice, Color.yellow),
			this.GetSound("PB_Angry1.wav", "Vfx_PB_Wander2", SoundType.Voice, Color.yellow),
			this.GetSound("PB_Angry2.wav", "Vfx_PB_Wander3", SoundType.Voice, Color.yellow),
			this.GetSound("PB_EvilLaught.wav", "Vfx_PB_Catch", SoundType.Voice, Color.yellow),
			this.GetSound("PB_SeeLaught.wav", "Vfx_PB_Spot", SoundType.Voice, Color.yellow),
			this.GetSound("PB_DeathIncoming.wav", "Vfx_PB_SuperAngry", SoundType.Voice, Color.yellow)];
			audMan = GetComponent<PropagatedAudioManager>();

			audWandering = [soundObjects[0], soundObjects[1], soundObjects[2]];
			audEvilLaught = soundObjects[3];
			audSeeLaught = soundObjects[4];
			audSuperAngry = soundObjects[5];

			var storedSprites = this.GetSpriteSheet(2, 2, 65f, "pencilBoy.png");
			angrySprite = storedSprites[0];
			findPlayerSprite = storedSprites[1];
			happySprite = storedSprites[2];
			superAngrySprite = storedSprites[3];
			spriteRenderer[0].sprite = storedSprites[0];

			gaugeSprite = this.GetSprite(Storage.GaugeSprite_PixelsPerUnit, "gaugeIcon.png");
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			lastSightedPlayerLocation = player.transform.position;
		}

		public override void Hear(GameObject source, Vector3 position, int value)
		{
			base.Hear(source, position, value);
			if (!looker.PlayerInSight())
				lastSightedPlayerLocation = position;

		}

		public override void Initialize()
		{
			base.Initialize();
			container = this.GetNPCContainer();
			behaviorStateMachine.ChangeState(new PencilBoy_Wander(this));
		}

		public bool ItemFits(Items item) =>
			stabbingItems.Contains(item);

		public void InsertItem(PlayerManager pm, EnvironmentController ec) =>
			GetSuperAngry();

		internal void AngryWander(bool forced)
		{
			if (forced || Random.value <= angryHummingChance)
				audMan.PlayRandomAudio(audWandering);
		}

		internal void SeePlayer(PlayerManager player)
		{
			behaviorStateMachine.ChangeNavigationState(new NavigationState_TargetPlayer(this, 63, player.transform.position));
			audMan.PlaySingle(audSeeLaught);
			spriteRenderer[0].sprite = findPlayerSprite;
		}

		internal void PersuePlayer(PlayerManager player)
		{
			behaviorStateMachine.CurrentNavigationState.UpdatePosition(player.transform.position);
			navigator.maxSpeed = foundSpeed;
			navigator.SetSpeed(foundSpeed);
		}

		internal void SuperAngryFollow()
		{
			navigator.maxSpeed = superAngrySpeed;
			navigator.SetSpeed(superAngrySpeed);
		}
		internal void GetSuperAngry()
		{
			spriteRenderer[0].sprite = superAngrySprite;
			audMan.FlushQueue(true);
			audMan.PlaySingle(audSuperAngry);
			behaviorStateMachine.ChangeState(new PencilBoy_SuperAngry(this));
			if (!container.HasLookerMod(lookerMod))
				container.AddLookerMod(lookerMod);
		}

		internal void Unsatisfied()
		{
			spriteRenderer[0].sprite = angrySprite;
			AngryWander(true);
		}

		internal void PlayerTurnAround()
		{
			Directions.ReverseList(navigator.currentDirs);
			behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}

		internal void StabSatisfied(PlayerManager player, bool superAngry)
		{
			PlayerTurnAround();
			disabledPlayer = player.GetMovementStatModifier();
			gauge = Singleton<CoreGameManager>.Instance.GetHud(player.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, stabLifeTime);
			if (superAngry)
			{
				player.plm.AddStamina(-player.plm.stamina, true);
				StartCoroutine(DisabledPlayerCooldown(true));
				container.RemoveLookerMod(lookerMod);
			}
			else
			{
				player.plm.AddStamina(-player.plm.staminaMax * 0.5f, true); // Workaround so the stamina doesn't go below 0
				StartCoroutine(DisabledPlayerCooldown(false));
			}
			audMan.PlaySingle(ITM_Pencil.audStab);
			audMan.PlaySingle(audEvilLaught);
			SetGuilt(3f, "stabbing");

			spriteRenderer[0].sprite = happySprite;
			behaviorStateMachine.ChangeState(new PencilBoy_Satisfied(this));
		}

		IEnumerator DisabledPlayerCooldown(bool superStab)
		{
			disabledPlayer.AddModifier("staminaRise", superStab ? stMod : normStMod);
			float time = stabLifeTime;
			while (time > 0f)
			{
				time -= TimeScale * Time.deltaTime;
				gauge.SetValue(stabLifeTime, time);
				yield return null;
			}
			disabledPlayer.RemoveModifier(superStab ? stMod : normStMod);
			gauge.Deactivate();

			yield break;
		}

		public override void Despawn()
		{
			base.Despawn();
			if (disabledPlayer)
			{
				disabledPlayer.RemoveModifier(stMod);
				disabledPlayer.RemoveModifier(normStMod);
			}
			gauge?.Deactivate();
		}

		readonly ValueModifier stMod = new(0f), normStMod = new(0.45f);
		PlayerMovementStatModifier disabledPlayer;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audSeeLaught, audEvilLaught, audSuperAngry;

		[SerializeField]
		internal SoundObject[] audWandering;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float stabLifeTime = 10f, speed = 15f, foundSpeed = 18f, superAngrySpeed = 38f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float angryHummingChance = 0.3f;

		[SerializeField]
		internal Sprite findPlayerSprite, angrySprite, superAngrySprite, happySprite;

		HudGauge gauge;

		Vector3 lastSightedPlayerLocation = default;

		internal Vector3 LastSightedPlayerLocation => lastSightedPlayerLocation;

		readonly ValueModifier lookerMod = new(float.MaxValue);

		public static void AddStabbingItem(Items item) =>
			stabbingItems.Add(item);

		readonly static HashSet<Items> stabbingItems = [];

		NPCAttributesContainer container;
	}

	internal class PencilBoy_StateBase(PencilBoy boy) : NpcState(boy)
	{
		protected PencilBoy boy = boy;
	}

	internal class PencilBoy_Wander(PencilBoy boy) : PencilBoy_StateBase(boy) // Copypaste for Playtime lol
	{
		float wanderCooldown = 3f;
		bool followingPlayer = false;

		public override void Enter()
		{
			base.Enter();
			boy.Unsatisfied();
			boy.looker.Blink();
			if (!boy.Navigator.HasDestination)
				ChangeNavigationState(new NavigationState_WanderRandom(boy, 0));

		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.CompareTag("Player"))
			{
				PlayerManager component = other.GetComponent<PlayerManager>();
				if (!component.Tagged)
					boy.StabSatisfied(component, false);

			}
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRandom(boy, 0));
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (!player.Tagged)
			{
				boy.SeePlayer(player);
				followingPlayer = true;
			}

		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (!player.Tagged)
				boy.PersuePlayer(player);

		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (!player.Tagged)
			{
				boy.Unsatisfied();
				boy.PlayerTurnAround();
			}
			followingPlayer = false;
		}

		public override void Update()
		{
			base.Update();
			if (!followingPlayer)
			{
				wanderCooldown -= boy.TimeScale * Time.deltaTime;
				while (wanderCooldown <= 0f)
				{
					boy.AngryWander(false);
					wanderCooldown += 3f;
				}
			}
		}
	}

	internal class PencilBoy_SuperAngry(PencilBoy boy) : PencilBoy_StateBase(boy) // he goes on whoever player he finds, not a specific one
	{
		public override void Enter()
		{
			base.Enter();
			boy.looker.Blink();
			target = new NavigationState_TargetPlayer(boy, 63, boy.LastSightedPlayerLocation);
			ChangeNavigationState(target);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (!player.Tagged)
			{
				ChangeNavigationState(target);
				target.UpdatePosition(player.transform.position);
				boy.SuperAngryFollow();
			}
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.CompareTag("Player"))
			{
				PlayerManager component = other.GetComponent<PlayerManager>();
				if (!component.Tagged)
					boy.StabSatisfied(component, true);
			}
		}

		public override void Hear(GameObject source, Vector3 position, int value)
		{
			base.Hear(source, position, value);
			if (!boy.looker.PlayerInSight())
			{
				ChangeNavigationState(target);
				target.UpdatePosition(position);
			}
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRandom(boy, 0));
		}
		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			boy.PlayerTurnAround();
		}

		NavigationState_TargetPlayer target;
	}
	internal class PencilBoy_Satisfied(PencilBoy boy) : PencilBoy_StateBase(boy)
	{
		public override void Enter()
		{
			base.Enter();
			if (!boy.Navigator.HasDestination)
				ChangeNavigationState(new NavigationState_WanderRandom(boy, 0));
		}
		float wanderCooldown = Random.Range(20f, 30f);
		public override void Update()
		{
			base.Update();
			wanderCooldown -= boy.TimeScale * Time.deltaTime;
			if (wanderCooldown <= 0f)
				boy.behaviorStateMachine.ChangeState(new PencilBoy_Wander(boy));
		}
	}
}
