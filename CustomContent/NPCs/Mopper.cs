using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Mopper : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(24f, "mop.png");
			audMan = GetComponent<PropagatedAudioManager>();
			audStartSweep = this.GetSound("startMop.wav", "Vfx_MOP_StartMop", SoundType.Voice, Color.white);
			audSweep = this.GetSound("mop.wav", "Vfx_MOP_Mop", SoundType.Voice, Color.white);

			slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").DuplicatePrefab();
			((SpriteRenderer)slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = this.GetSprite(12f, "wat.png");
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// stuff above^^

		public override void Initialize()
		{
			base.Initialize();
			slipDropCool = slipDropCooldown;
			home = ec.CellFromPosition(transform.position);
			behaviorStateMachine.ChangeState(new Mopper_Wait(this));
		}

		internal void StartSweeping()
		{
			audMan.PlaySingle(audStartSweep);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

			sweeping = true;

		}

		internal void StopSweeping()
		{
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;

			sweeping = false;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (sweeping)
			{
				if (slips > 0)
				{
					var cell = ec.CellFromPosition(transform.position);
					if (cell != lastCell)
					{
						lastCell = cell;
						slips--;
						SpawnSlipper(cell);
					}
					return;
				}
				slipDropCool -= TimeScale * Time.deltaTime;
				if (slipDropCool <= 0f)
				{
					slips += slipsPerTile;
					slipDropCool += slipDropCooldown;
				}
			}
		}

		public override void VirtualOnTriggerEnter(Collider other)
		{
			base.VirtualOnTriggerEnter(other);
			if (sweeping && other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
				audMan.PlaySingle(audSweep);

		}


		internal void SpawnSlipper(Cell cell)
		{
			var slip = Instantiate(slipMatPre);
			slip.SetAnOwner(gameObject);
			slip.transform.position = cell.FloorWorldPosition;
			slip.StartCoroutine(GameExtensions.TimerToDestroy(slip.gameObject, ec, 15f));
		}

		internal bool IsHome => home == ec.CellFromPosition(transform.position);
		internal float ActiveCooldown => Random.Range(minActive, maxActive);
		internal float WaitCooldown => Random.Range(minWait, maxWait);
		internal Cell home;

		[SerializeField]
		internal SoundObject audSweep, audStartSweep;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		[SerializeField]
		internal float minActive = 30f, maxActive = 50f, minWait = 40f, maxWait = 60f, speed = 45f, slipDropCooldown = 6f;

		[SerializeField]
		internal int slipsPerTile = 3;

		bool sweeping = false;
		int slips = 0;
		float slipDropCool;
		Cell lastCell = null;
	}

	internal class Mopper_StateBase(Mopper mop) : NpcState(mop)
	{
		protected Mopper mop = mop;
	}

	internal class Mopper_Wait(Mopper mop) : Mopper_StateBase(mop)
	{
		float waitCooldown = mop.WaitCooldown;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(mop, 0));
			mop.StopSweeping();
		}
		public override void Update()
		{
			base.Update();
			waitCooldown -= mop.TimeScale * Time.deltaTime;
			if (waitCooldown <= 0f)
				mop.behaviorStateMachine.ChangeState(new Mopper_Start(mop));
		}
	}

	internal class Mopper_Start(Mopper mop) : Mopper_StateBase(mop)
	{
		float activeCooldown = mop.ActiveCooldown;
		public override void Enter()
		{
			base.Enter();
			mop.StartSweeping();
			ChangeNavigationState(new NavigationState_WanderRandom(mop, 0));

		}
		public override void Update()
		{
			base.Update();
			activeCooldown -= mop.TimeScale * Time.deltaTime;
			if (activeCooldown <= 0f)
				mop.behaviorStateMachine.ChangeState(new Mopper_GoBack(mop));
		}
	}

	internal class Mopper_GoBack(Mopper mop) : Mopper_StateBase(mop)
	{
		NavigationState_TargetPosition tar;
		public override void Enter()
		{
			base.Enter();
			tar = new(mop, 0, mop.home.FloorWorldPosition);
			ChangeNavigationState(tar);
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (mop.IsHome)
			{
				mop.behaviorStateMachine.ChangeState(new Mopper_Wait(mop));
				return;
			}
			ChangeNavigationState(tar);
		}
	}
}
