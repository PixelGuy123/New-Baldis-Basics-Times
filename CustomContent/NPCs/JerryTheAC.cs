using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class JerryTheAC : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();
			audRolling = this.GetSound("acRolling.wav", "Vfx_JerryAc_Rolling", SoundType.Voice, Color.white);
			audActive = this.GetSound("acRunning.wav", "Vfx_JerryAc_Cool", SoundType.Voice, Color.white);

			var sprs = this.GetSpriteSheet(4, 2, 25f, "jerry.png");
			spriteRenderer[0].CreateAnimatedSpriteRotator(
				[new() { angleCount = 8, spriteSheet = sprs }]
				);
			spriteRenderer[0].sprite = sprs[0];



			var system = new GameObject("JerryParticles").AddComponent<ParticleSystem>();
			system.transform.SetParent(transform);
			system.transform.localPosition = Vector3.forward * 1.25f;
			system.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = this.GetTexture("freezingParticles.png") };

			var main = system.main;
			main.gravityModifierMultiplier = 0.05f;
			main.startLifetimeMultiplier = 1.8f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(0.5f, 1f);

			var emission = system.emission;
			emission.rateOverTimeMultiplier = 100f;
			emission.enabled = false;

			var vel = system.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.Local;
			vel.x = new(-35f, 35f);
			vel.y = new(-4.5f, 4.5f);
			vel.z = new(25f, 65f);

			var an = system.textureSheetAnimation;
			an.enabled = true;
			an.numTilesX = 2;
			an.numTilesY = 2;
			an.animation = ParticleSystemAnimationType.WholeSheet;
			an.fps = 0f;
			an.timeMode = ParticleSystemAnimationTimeMode.FPS;
			an.cycleCount = 1;
			an.startFrame = new(0, 3); // 2x2

			var col = system.collision;
			col.enabled = true;
			col.type = ParticleSystemCollisionType.World;
			col.enableDynamicColliders = false;

			parts = system;

			slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").SafeDuplicatePrefab(true);
			((SpriteRenderer)slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = this.GetSprite(12f, "wat.png");
			slipMatPre.antiForceReduceFactor = 0.735f;
			slipMatPre.name = "JerryIcePatch";
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
		internal SoundObject audRolling, audActive;

		[SerializeField]
		internal ParticleSystem parts;

		[SerializeField]
		internal float minActive = 30f, maxActive = 60f;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		public override void Initialize()
		{
			base.Initialize();
			foreach (var room in ec.rooms)
				if (room.type == RoomType.Room && room.category != RoomCategory.Special)
					cells.AddRange(room.AllEntitySafeCellsNoGarbage().Where(x => x.open && !x.HasAnyHardCoverage && x.shape == TileShape.Corner));

			if (cells.Count == 0)
			{
				Debug.LogWarning("JERRY HAS FAILED TO FIND ANY GOOD SPOT, NOOOOOOOO!!!");
				behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(this, 0));
				behaviorStateMachine.ChangeState(new NpcState(this));
				return;
			}

			behaviorStateMachine.ChangeState(new JerryTheAC_GoToRoom(this));
		}

		public override void Despawn()
		{
			base.Despawn();
			RemoveFuncIfExists();
		}

		public void RollingOn()
		{
			audMan.FlushQueue(true);
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audRolling);

			navigator.maxSpeed = 24.5f;
			navigator.SetSpeed(24.5f);
			nextPos = zero;

			var em = parts.emission;
			em.enabled = false;

			RemoveFuncIfExists();
		}

		public void ActivateAirConditioner(RoomController room)
		{
			behaviorStateMachine.ChangeNavigationState(new NavigationState_DoNothing(this, 0));
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);

			var em = parts.emission;
			em.enabled = true;

			audMan.FlushQueue(true);
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audActive);

			RemoveFuncIfExists();
			lastCreatedFunction = room.functionObject.AddComponent<FreezingRoomFunction>();
			lastCreatedFunction.slipMatPre = slipMatPre;
			room.functions.AddFunction(lastCreatedFunction);
			lastCreatedFunction.Initialize(room);

			var cell = ec.CellFromPosition(transform.position);
			if (cell.shape != TileShape.Corner) return;

			Vector3 fwd = new();
			cell.AllWallDirections.ForEach(x => fwd += x.GetOpposite().ToVector3()); // (A + B).normalized gives the diagonal between two points (thanks to ChatGPT, probably some general knowledge, I haven't reached study in Vectors yet lol)

			nextPos = transform.position + fwd.normalized;
		}

		void RemoveFuncIfExists()
		{
			if (lastCreatedFunction)
			{
				lastCreatedFunction.Room.functions.RemoveFunction(lastCreatedFunction);
				Destroy(lastCreatedFunction);
			}
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (nextPos != zero)
			{
				transform.RotateSmoothlyToNextPoint(nextPos, 1f);
			}
		}
		Vector3 nextPos;
		readonly Vector3 zero = Vector3.zero;

		FreezingRoomFunction lastCreatedFunction;
		public Cell GetRandomSpotToGo => cells[Random.Range(0, cells.Count)];
		public float ActiveCooldown => Random.Range(minActive, maxActive);

		readonly List<Cell> cells = [];
		
	}

	internal class JerryTheAC_StateBase(JerryTheAC jr) : NpcState(jr)
	{
		protected JerryTheAC jr = jr;
	}

	internal class JerryTheAC_GoToRoom(JerryTheAC jr) : JerryTheAC_StateBase(jr)
	{
		NavigationState_TargetPosition spotGo;
		readonly Cell spot = jr.GetRandomSpotToGo;
		public override void Enter()
		{
			base.Enter();
			jr.RollingOn();
			spotGo = new(jr, 64, spot.FloorWorldPosition);
			ChangeNavigationState(spotGo);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (jr.ec.CellFromPosition(jr.transform.position) != spot)
				ChangeNavigationState(spotGo);
			else
				jr.behaviorStateMachine.ChangeState(new JerryTheAC_Activate(jr, spot.room));
		}
		public override void Exit()
		{
			base.Exit();
			spotGo.priority = 0;
		}
	}

	internal class JerryTheAC_Activate(JerryTheAC jr, RoomController room) : JerryTheAC_StateBase(jr)
	{
		float activeCooldown = jr.ActiveCooldown;
		readonly Vector3 pos = jr.transform.position;
		public override void Enter()
		{
			base.Enter();
			jr.ActivateAirConditioner(room);
		}
		public override void Update()
		{
			base.Update();
			activeCooldown -= jr.TimeScale * Time.deltaTime;
			if (activeCooldown <= 0f || jr.transform.position != pos)
				jr.behaviorStateMachine.ChangeState(new JerryTheAC_GoToRoom(jr));
		}
	}
}
