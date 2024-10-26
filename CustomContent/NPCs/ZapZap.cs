using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class ZapZap : NPC, INPCPrefab, IItemAcceptor
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();
			anim = gameObject.AddComponent<AnimationComponent>();
			anim.renderers = [spriteRenderer[0]];
			anim.speed = 6.5f;

			var sprs = this.GetSpriteSheet(3, 1, 25f, "zap.png");
			sprsActive = [sprs[0], sprs[1]];
			sprDeactivated = [sprs[2]];
			spriteRenderer[0].sprite = sprs[2];
			anim.animation = sprDeactivated;

			audActivated = this.GetSound("zap_activated.wav", "Vfx_ZapZap_Activated", SoundType.Voice, new(0f, 0.55f, 0.75f));
			audActivationNoises = this.GetSoundNoSub("zap_activatesfx.wav", SoundType.Voice);
			audDeactivating = this.GetSound("zap_deactivating.wav", "Vfx_ZapZap_Deactivate", SoundType.Voice, new(0f, 0.55f, 0.75f));
			audHacked = this.GetSound("zap_hacked.wav", "Vfx_ZapZap_Hacked", SoundType.Voice, new(0f, 0.55f, 0.75f));

			eletricityPre = BBTimesManager.man.Get<Eletricity>("DoorEletricityPrefab");
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audActivated, audActivationNoises, audDeactivating, audHacked;

		[SerializeField]
		internal Sprite[] sprsActive, sprDeactivated;

		[SerializeField]
		internal AnimationComponent anim;

		[SerializeField]
		internal float minWaitCooldown = 30f, maxWaitCooldown = 60f, minActiveCooldown = 40f, maxActiveCooldown = 80f, eletricityDestructionDelay = 15f;

		[SerializeField]
		internal Eletricity eletricityPre;

		readonly List<Transform> eletricities = [];
		Cell home;
		float eletrictyDestructionCooldown = 0f;

		public override void Initialize()
		{
			base.Initialize();
			eletrictyDestructionCooldown = eletricityDestructionDelay;
			anim.Initialize(ec);
			home = ec.CellFromPosition(transform.position);
			behaviorStateMachine.ChangeState(new ZapZap_WaitDeactivated(this, false));
		}

		internal void SpawnEletricity(StandardDoor door)
		{
			var eletricity = Instantiate(eletricityPre);
			eletricity.Initialize(gameObject, door.doors[0].transform.position, 0.25f, ec);
			eletricity.transform.rotation = Quaternion.Euler(0f, door.direction.PerpendicularList()[0].ToRotation().eulerAngles.y, 90f);
			eletricities.Add(eletricity.transform);
		}

		public override void Despawn()
		{
			base.Despawn();
			while (eletricities.Count != 0)
			{
				Destroy(eletricities[0].gameObject);
				eletricities.RemoveAt(0);
			}

		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (eletricities.Count == 0)
			{
				eletrictyDestructionCooldown = eletricityDestructionDelay;
				return;
			}

			eletrictyDestructionCooldown -= TimeScale * Time.deltaTime;
			if (eletrictyDestructionCooldown < 0f)
			{
				Destroy(eletricities[0].gameObject);
				eletricities.RemoveAt(0);
				eletrictyDestructionCooldown += eletricityDestructionDelay;
			}
		}

		readonly static HashSet<Items> acceptableItems = [];

		public static void AddDeactivator(Items item) => acceptableItems.Add(item);

		public bool ItemFits(Items item) => IsActivated && acceptableItems.Contains(item);

		public void InsertItem(PlayerManager pm, EnvironmentController ec) =>
			behaviorStateMachine.ChangeState(new ZapZap_WaitDeactivated(this, true));


		void SetActiveState(bool active) =>
			anim.animation = active ? sprsActive : sprDeactivated;

		public void Activate()
		{
			SetActiveState(true);
			audMan.FlushQueue(true);
			audMan.PlaySingle(audActivationNoises);
			audMan.PlaySingle(audActivated);

			navigator.maxSpeed = 11f;
			navigator.SetSpeed(11f);
		}

		public void Deactivate(bool hacked)
		{
			if (IsActivated)
			{
				audMan.FlushQueue(true);
				audMan.PlaySingle(hacked ? audHacked : audDeactivating);
			}
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			SetActiveState(false);
		}

		public bool IsActivated => anim.animation == sprsActive;
		public float DeactivatedCooldown => Random.Range(minWaitCooldown, maxWaitCooldown);
		public float ActiveCooldown => Random.Range(minActiveCooldown, maxActiveCooldown);
		public Cell Home => home;
	}

	internal class ZapZap_StateBase(ZapZap zap) : NpcState(zap)
	{
		protected ZapZap zap = zap;
	}

	internal class ZapZap_WaitDeactivated(ZapZap zap, bool hacked) : ZapZap_StateBase(zap)
	{
		float cooldown = zap.DeactivatedCooldown;
		public override void Enter()
		{
			base.Enter();
			zap.Deactivate(hacked);
			ChangeNavigationState(new NavigationState_DoNothing(zap, 0));
		}

		public override void Update()
		{
			base.Update();
			cooldown -= zap.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				zap.behaviorStateMachine.ChangeState(new ZapZap_Active(zap));
			}
		}
	}

	internal class ZapZap_Active(ZapZap zap) : ZapZap_StateBase(zap)
	{
		float cooldown = zap.ActiveCooldown;
		public override void Enter()
		{
			base.Enter();
			zap.Activate();
			ChangeNavigationState(new NavigationState_WanderRandom(zap, 0));
		}
		public override void DoorHit(StandardDoor door)
		{
			base.DoorHit(door);
			zap.SpawnEletricity(door);
		}
		public override void Update()
		{
			base.Update();
			cooldown -= zap.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				zap.behaviorStateMachine.ChangeState(new ZapZap_GoBack(zap));
			}
		}
	}

	internal class ZapZap_GoBack(ZapZap zap) : ZapZap_StateBase(zap)
	{
		NavigationState_TargetPosition navState;
		public override void Enter()
		{
			base.Enter();
			navState = new(zap, 64, zap.Home.FloorWorldPosition);
			ChangeNavigationState(navState);
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (zap.ec.CellFromPosition(zap.transform.position) != zap.Home)
				ChangeNavigationState(navState);
			else
				zap.behaviorStateMachine.ChangeState(new ZapZap_WaitDeactivated(zap, false));
		}
		public override void Exit()
		{
			base.Exit();
			navState.priority = 0;
		}
	}
}
