using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class ZeroPrize : NPC, INPCPrefab
	{

		public void SetupPrefab()
		{
			audStartSweep = this.GetSound("0thprize_timetosweep.wav", "Vfx_0TH_WannaSweep", SoundType.Voice, new(0.99609375f, 0.99609375f, 0.796875f));
			audSweep = this.GetSound("0thprize_mustsweep.wav", "Vfx_0TH_Sweep", SoundType.Voice, new(0.99609375f, 0.99609375f, 0.796875f));
			var storedSprites = this.GetSpriteSheet(2, 1, 45f, "0thprize.png");
			activeSprite = storedSprites[0];
			deactiveSprite = storedSprites[1];
			spriteRenderer[0].sprite = activeSprite;

			audMan = GetComponent<PropagatedAudioManager>();

			((CapsuleCollider)baseTrigger[0]).radius = 4f; // default radius of Gotta Sweep
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		void Start()
		{
			home = ec.CellFromPosition(transform.position);
		}

		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new ZeroPrize_Wait(this, SleepingCooldown, false));
		}

		internal void StartSweeping()
		{
			blinking = false;
			moveMod.forceTrigger = true;
			audMan.PlaySingle(audStartSweep);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			
		}

		internal void StopSweeping()
		{
			moveMod.forceTrigger = false;
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			blinking = false;

			ClearActs();
		}

		public override void VirtualOnTriggerEnter(Collider other) // copypaste from gotta sweep's code
		{
			if (IsSleeping) return;


			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				Entity component = other.GetComponent<Entity>();
				if (component != null)
				{
					audMan.PlaySingle(audSweep); 
					ActivityModifier externalActivity = component.ExternalActivity;
					if (!externalActivity.moveMods.Contains(moveMod))
					{
						externalActivity.moveMods.Add(moveMod);
						actMods.Add(externalActivity);
					}
				}
			}
		}
		public override void VirtualOnTriggerExit(Collider other) // copypaste from gotta sweep's code
		{
			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				Entity component = other.GetComponent<Entity>();
				if (component != null)
				{
					ActivityModifier externalActivity = component.ExternalActivity;
					externalActivity.moveMods.Remove(moveMod);
					actMods.Remove(externalActivity);
				}
			}
		}



		public override void Despawn()
		{
			base.Despawn();
			ClearActs();
		}

		void ClearActs()
		{
			while (actMods.Count > 0)
			{
				actMods[0].moveMods.Remove(moveMod);
				actMods.RemoveAt(0);
			}
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (!blinking)
				spriteRenderer[0].sprite = IsSleeping ? deactiveSprite : activeSprite;

			moveMod.movementAddend = navigator.Velocity.normalized * navigator.speed * moveModMultiplier * navigator.Am.Multiplier;
		}
		
		internal void Blink()
		{
			blinking = !blinking;
			spriteRenderer[0].sprite = activeSprite;
		}

		internal bool IsHome => home == ec.CellFromPosition(transform.position);
		internal bool IsSleeping => navigationStateMachine.currentState is NavigationState_DoNothing;
		internal float ActiveCooldown => Random.Range(minActive, maxActive);
		internal float SleepingCooldown => Random.Range(minWait, maxWait);
		internal Cell home;


		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
		readonly List<ActivityModifier> actMods = [];

		[SerializeField]
		internal SoundObject audSweep, audStartSweep;

		[SerializeField]
		internal Sprite activeSprite, deactiveSprite;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal float moveModMultiplier = 0.97f, minActive = 30f, maxActive = 50f, minWait = 40f, maxWait = 60f, speed = 80f;

		bool blinking = false;
	}
	internal class ZeroPrize_StateBase(ZeroPrize prize) : NpcState(prize)
	{
		protected ZeroPrize prize = prize;
	}

	internal class ZeroPrize_Wait(ZeroPrize prize, float cooldown, bool isActive) : ZeroPrize_StateBase(prize) // reusable for either make him active or deactive
	{
		float waitTime = cooldown;

		readonly bool active = isActive;

		public override void Enter()
		{
			base.Enter();
			if (!active) // he is not active
			{
				prize.StopSweeping();
				ChangeNavigationState(new NavigationState_DoNothing(prize, 0));
				return;
			}

			prize.StartSweeping(); // he is active
			ChangeNavigationState(new NavigationState_WanderRandom(prize, 0));
		}

		public override void Update()
		{
			base.Update();
			waitTime -= Time.deltaTime * prize.TimeScale;
			if (waitTime <= 0f)
			{
				if (!active)
					prize.behaviorStateMachine.ChangeState(prize.IsSleeping // if he is in party, there's no reason to *wake up*
						? new ZeroPrize_Awakening(prize) : new ZeroPrize_Wait(prize, prize.ActiveCooldown, true));
				else 
					prize.behaviorStateMachine.ChangeState(new ZeroPrize_WaitForSpawnBack(prize));
			}
		}
	}

	internal class ZeroPrize_Awakening(ZeroPrize prize) : ZeroPrize_StateBase(prize)
	{
		public override void Update()
		{
			base.Update();
			blinkTime -= prize.TimeScale * Time.deltaTime;
			if (blinkTime <= 0f)
			{
				prize.Blink();

				if (++blinks >= 6)
				{
					maxBlinkTime /= 2f;
					if (++blinkCycles >= 3)
						prize.behaviorStateMachine.ChangeState(new ZeroPrize_Wait(prize, prize.ActiveCooldown, true));
					blinks -= 6;
				}

				blinkTime += maxBlinkTime;
			}
			
		}

		float blinkTime = 1f, maxBlinkTime = 1f;
		int blinks = 0, blinkCycles = 0;

	}

	internal class ZeroPrize_WaitForSpawnBack(ZeroPrize prize) : ZeroPrize_StateBase(prize)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_TargetPosition(prize, 63, prize.home.FloorWorldPosition));
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (!prize.IsHome)
			{
				prize.behaviorStateMachine.CurrentNavigationState.UpdatePosition(prize.home.FloorWorldPosition);
				return;
			}
			prize.behaviorStateMachine.ChangeState(new ZeroPrize_Wait(prize, prize.SleepingCooldown, false));
		}
	}
}
