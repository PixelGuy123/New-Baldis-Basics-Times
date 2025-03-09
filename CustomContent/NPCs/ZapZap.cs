using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using System.Collections.Generic;
using UnityEngine;
using PixelInternalAPI.Extensions;
using BBTimes.CustomComponents.NpcSpecificComponents.ZapZap;
using PixelInternalAPI.Classes;
using BBTimes.Extensions.ObjectCreationExtensions;

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
			audActivationNoises = this.GetSoundNoSub("zap_activatesfx.wav", SoundType.Effect);
			audDeactivating = this.GetSound("zap_deactivating.wav", "Vfx_ZapZap_Deactivate", SoundType.Voice, new(0f, 0.55f, 0.75f));
			audHacked = this.GetSound("zap_hacked.wav", "Vfx_ZapZap_Hacked", SoundType.Voice, new(0f, 0.55f, 0.75f));

			var elePre = BBTimesManager.man.Get<Eletricity>("EletricityPrefab");

			elePre = elePre.SafeDuplicatePrefab(true);
			eletricityPre = elePre.gameObject.AddComponent<ZapZapEletricity>();
			eletricityPre.affectOwnerAfterExit = elePre.affectOwnerAfterExit;
			eletricityPre.ani = elePre.ani;
			eletricityPre.collider = elePre.collider;
			eletricityPre.eletricityForce = elePre.eletricityForce;
			eletricityPre.ignoreBootsAttribute = elePre.ignoreBootsAttribute;

			Destroy(elePre); // Destroy this component now
			eletricityPre.collider.size = new(eletricityPre.collider.size.x, 1.5f, eletricityPre.collider.size.z);

			var eleRenderer = ObjectCreationExtensions.CreateSpriteBillboard(elePre.ani.animation[0], false);
			eleRenderer.transform.SetParent(eletricityPre.transform);
			eleRenderer.transform.localPosition = new(0f, -0.1f, 0f);
			eleRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			eleRenderer.name = "SpriteBackwards";

			eletricityPre.ani.renderers = eletricityPre.GetComponentsInChildren<SpriteRenderer>();
			eletricityPre.name = "ZapZapDoorEletricity";

			// EletrecutationComponent Setup
			eletricityPre.compPre = new GameObject("ZapZapEletricity").SetAsPrefab(true).AddComponent<ZapZapEletrecutationComponent>();
			eletricityPre.compPre.gameObject.layer = LayerStorage.ignoreRaycast;

			eletricityPre.compPre.audMan = eletricityPre.compPre.gameObject.CreatePropagatedAudioManager(35f, 65f);
			eletricityPre.compPre.audEletrecute = eletricityPre.GetComponent<AudioManager>().soundOnStart[0]; // Shock audio

			var zapCol = eletricityPre.compPre.gameObject.AddComponent<CapsuleCollider>();
			zapCol.isTrigger = true;
			zapCol.radius = 20f;
			zapCol.height = 10f;

			var system = eletricityPre.compPre.gameObject.AddComponent<ParticleSystem>();
			system.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = this.GetTexture("zapParticles.png") };

			var main = system.main;
			main.gravityModifierMultiplier = 1.75f;
			main.startLifetimeMultiplier = 1.8f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(0.5f, 1f);

			var emission = system.emission;
			emission.rateOverTimeMultiplier = 65f;
			emission.enabled = true;

			var vel = system.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.World;
			vel.x = new(-15f, 15f);
			vel.y = new(-4.5f, 4.5f);
			vel.z = new(-15f, 15f);

			var an = system.textureSheetAnimation;
			an.enabled = true;
			an.numTilesX = 2;
			an.numTilesY = 2;
			an.animation = ParticleSystemAnimationType.WholeSheet;
			an.fps = 21f;
			an.timeMode = ParticleSystemAnimationTimeMode.FPS;
			an.cycleCount = 16;
			an.startFrame = new(0, 3); // 2x2

			var col = system.collision;
			col.enabled = true;
			col.type = ParticleSystemCollisionType.World;
			col.enableDynamicColliders = false;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";
		
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
		internal float minWaitCooldown = 30f, maxWaitCooldown = 60f, minActiveCooldown = 40f, maxActiveCooldown = 80f, eletricityPretricityDestructionDelay = 45f;

		[SerializeField]
		internal ZapZapEletricity eletricityPre;

		readonly List<ZapZapEletricity> eletricityPretricities = [];
		Cell home;
		float eletricityPretrictyDestructionCooldown = 0f;

		public override void Initialize()
		{
			base.Initialize();
			eletricityPretrictyDestructionCooldown = eletricityPretricityDestructionDelay;
			anim.Initialize(ec);
			home = ec.CellFromPosition(transform.position);
			behaviorStateMachine.ChangeState(new ZapZap_WaitDeactivated(this, false));
		}

		internal void SpawneletricityPretricity(StandardDoor door)
		{
			if (AffectedDoors.Contains(door))
				return;

			var eletricityPretricity = Instantiate(eletricityPre);
			eletricityPretricity.Initialize(gameObject, door.doors[0].transform.position, 0.25f, ec);
			eletricityPretricity.transform.rotation = Quaternion.Euler(0f, door.direction.PerpendicularList()[0].ToRotation().eulerAngles.y, 90f);
			eletricityPretricity.AffectedDoor = door;
			eletricityPretricities.Add(eletricityPretricity);
			AffectedDoors.Add(door);
		}

		public override void Despawn()
		{
			base.Despawn();
			while (eletricityPretricities.Count != 0)
			{
				Destroy(eletricityPretricities[0].gameObject);
				eletricityPretricities.RemoveAt(0);
			}

		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (eletricityPretricities.Count == 0)
			{
				eletricityPretrictyDestructionCooldown = eletricityPretricityDestructionDelay;
				return;
			}

			eletricityPretrictyDestructionCooldown -= TimeScale * Time.deltaTime;
			if (eletricityPretrictyDestructionCooldown < 0f)
			{
				AffectedDoors.Remove(eletricityPretricities[0].AffectedDoor);
				Destroy(eletricityPretricities[0].gameObject);
				eletricityPretricities.RemoveAt(0);
				eletricityPretrictyDestructionCooldown += eletricityPretricityDestructionDelay;
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
		public HashSet<Door> AffectedDoors { get; } = [];
	}

	internal class ZapZap_StateBase(ZapZap zap) : NpcState(zap)
	{
		protected ZapZap zap = zap;

		public override void DoorHit(StandardDoor door)
		{
			base.DoorHit(door);
			if (zap.IsActivated)
				zap.SpawneletricityPretricity(door);
		}
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
