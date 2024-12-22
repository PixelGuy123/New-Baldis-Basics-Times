using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class CoolMop : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			var mopAnim = this.GetSpriteSheet(5, 1, 24f, "coolMop.png");
			spriteRenderer[0].sprite = mopAnim[2];
			var subColor = new Color(0.99609375f, 0.5f, 0.5f);

			audMan = GetComponent<PropagatedAudioManager>();
			audStartSweep = this.GetSound("CoolerStartMop.mp3", "Vfx_MOP_StartMop", SoundType.Voice, subColor);
			audGoofyHahahas = [
				this.GetSound("CoolerMop.mp3", "Vfx_MOP_Mop", SoundType.Voice, subColor),
				this.GetSound("CoolerMop2.mp3", "Vfx_MOP_Mop2", SoundType.Voice, subColor)
				];
			audEndSweep = this.GetSound("CoolerEndMop.mp3", "Vfx_MOP_EndMop", SoundType.Voice, subColor);
			audCarefulWithWater = this.GetSound("CoolerWatering.mp3", "Vfx_MOP_Watering", SoundType.Voice, subColor);

			slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").SafeDuplicatePrefab(true);
			((SpriteRenderer)slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = this.GetSprite(24f, "coolWater.png");

			wetSign = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "coolWetFloorSign.png")).AddSpriteHolder(out var renderer, 1.6f).gameObject.AddComponent<EmptyMonoBehaviour>();
			renderer.name = "WetFloorSign_Renderer";
			wetSign.name = "WetFloorSign";
			wetSign.gameObject.ConvertToPrefab(true);

			idle = [mopAnim[2]];
			moving = mopAnim.MirrorSprites();

			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = spriteRenderer;
			animComp.speed = 12f;
			animComp.animation = idle;
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
			animComp.Initialize(ec);
			slipDropCool = slipDropCooldown;
			home = ec.CellFromPosition(transform.position);
			behaviorStateMachine.ChangeState(new CoolMop_Wait(this));
		}

		internal void StartSweeping()
		{
			animComp.animation = moving;
			audMan.PlaySingle(audStartSweep);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

			sweeping = true;

		}
		internal void SayEndMopping() =>
			audMan.PlaySingle(audEndSweep);
		internal void StopSweeping()
		{
			animComp.animation = idle;
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
						SpawnSlipper(cell);
						slips--;
					}
					return;
				}
				slipDropCool -= TimeScale * Time.deltaTime;
				if (slipDropCool <= 0f)
				{
					slips = slipsPerTile;
					slipDropCool += slipDropCooldown;
					audMan.PlaySingle(audCarefulWithWater);
				}
			}
		}

		public override void VirtualOnTriggerEnter(Collider other)
		{
			base.VirtualOnTriggerEnter(other);
			if (sweeping && other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
				audMan.PlayRandomAudio(audGoofyHahahas);

		}


		internal void SpawnSlipper(Cell cell)
		{
			var slip = Instantiate(slipMatPre);
			slip.SetAnOwner(gameObject);
			slip.transform.position = cell.FloorWorldPosition;
			slip.StartCoroutine(GameExtensions.TimerToDestroy(slip.gameObject, ec, 15f));

			if (slips == slipsPerTile || slips == 1)
			{
				var sign = Instantiate(wetSign);
				sign.transform.position = cell.FloorWorldPosition;
				sign.StartCoroutine(GameExtensions.TimerToDestroy(sign.gameObject, ec, 15f));
			}
		}

		internal bool IsHome => home == ec.CellFromPosition(transform.position);
		internal float ActiveCooldown => Random.Range(minActive, maxActive);
		internal float WaitCooldown => Random.Range(minWait, maxWait);
		internal Cell home;

		[SerializeField]
		internal SoundObject audStartSweep, audEndSweep, audCarefulWithWater;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal Sprite[] idle, moving;

		[SerializeField]
		internal SoundObject[] audGoofyHahahas;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SlippingMaterial slipMatPre;

		[SerializeField]
		internal EmptyMonoBehaviour wetSign;

		[SerializeField]
		internal float minActive = 30f, maxActive = 50f, minWait = 40f, maxWait = 60f, speed = 45f, slipDropCooldown = 6f;

		[SerializeField]
		internal int slipsPerTile = 5;

		bool sweeping = false;
		int slips = 0;
		float slipDropCool;
		Cell lastCell = null;
	}

	internal class CoolMop_StateBase(CoolMop mop) : NpcState(mop)
	{
		protected CoolMop mop = mop;
	}

	internal class CoolMop_Wait(CoolMop mop) : CoolMop_StateBase(mop)
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
				mop.behaviorStateMachine.ChangeState(new CoolMop_Start(mop));
		}
	}

	internal class CoolMop_Start(CoolMop mop) : CoolMop_StateBase(mop)
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
				mop.behaviorStateMachine.ChangeState(new CoolMop_GoBack(mop));
		}
	}

	internal class CoolMop_GoBack(CoolMop mop) : CoolMop_StateBase(mop)
	{
		NavigationState_TargetPosition tar;
		public override void Enter()
		{
			base.Enter();
			mop.SayEndMopping();
			tar = new(mop, 0, mop.home.FloorWorldPosition);
			ChangeNavigationState(tar);
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (mop.IsHome)
			{
				mop.behaviorStateMachine.ChangeState(new CoolMop_Wait(mop));
				return;
			}
			ChangeNavigationState(tar);
		}
	}
}
