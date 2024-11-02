using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents.ScienceTeacher;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using MTM101BaldAPI;
using BBTimes.Manager;
using System.Collections;
using HarmonyLib;

namespace BBTimes.CustomContent.NPCs
{
	public class ScienceTeacher : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			var sprs = this.GetSpriteSheet(3, 1, 25f, "scienceTeacher.png");
			spriteRenderer[0].sprite = sprs[1];
			renderer = spriteRenderer[0];

			audMan = GetComponent<PropagatedAudioManager>();
			footStepAudMan = gameObject.CreatePropagatedAudioManager(85f, 100f);
			var subColor = new Color(0.159765625f, 0.43125f, 0.133203125f);

			audComplains = 
				[
				this.GetSound("Potion_Fired.wav", "Vfx_SciTeacher_spillComplain1", SoundType.Voice, subColor),
				this.GetSound("Potion_Job.wav", "Vfx_SciTeacher_spillComplain2", SoundType.Voice, subColor)
				];
			audFindPlayer = [
				this.GetSound("Potion_AboutToThrow.wav", "Vfx_SciTeacher_findPlayer1", SoundType.Voice, subColor),
				this.GetSound("Potion_YouThere.wav", "Vfx_SciTeacher_findPlayer2", SoundType.Voice, subColor)
				];
			audComeOn = this.GetSound("Potion_ComeOn.wav", "Vfx_SciTeacher_expectingNoSpill", SoundType.Voice, subColor);
			audWalking = this.GetSound("Potion_Walking.wav", "Vfx_SciTeacher_walking", SoundType.Voice, subColor);

			sprNormal = sprs[0];
			sprWithPotion = sprs[1];
			sprUpset = sprs[2];

			// -------------- POTIONS CREATION --------------------

			var spillingNoise = this.GetSound("Potion_Spilling.wav", "Vfx_SciTeacher_spills", SoundType.Voice, Color.white);
			Sprite potionDefaultVisual = this.GetSprite(30f, "unknownPotion.png"); // temp

			int potIdx = 0;
			Sprite[] potDesings = this.GetSpriteSheet(2, 2, 25f, "pots.png");
			potionsPre = new Potion[3];

			CreatePotion<AcidPotion>("Acid", potDesings[0]).audAcidicEffect = this.GetSoundNoSub("acid.wav", SoundType.Voice);

			var speedingPot = CreatePotion<SpeedingOrSlowingPotion>("SpeedOrSlow", potDesings[1]);
			speedingPot.audSpeedBuff = this.GetSoundNoSub("speedup.wav", SoundType.Voice);
			speedingPot.audSpeedNerf = this.GetSoundNoSub("slowdown.wav", SoundType.Voice);
			speedingPot.sprFast = potDesings[2];
			speedingPot.sprSlow = potDesings[1];

			var slipPot = CreatePotion<SlipperyPotion>("Slippery", potDesings[3]);
			slipPot.audSlip = BBTimesManager.man.Get<SoundObject>("slipAud");

			

			T CreatePotion<T>(string name, Sprite potionSpillingVisual) where T : Potion
			{
				var potionObj = ObjectCreationExtensions.CreateSpriteBillboard(potionDefaultVisual).AddSpriteHolder(out var potionRenderer, 0f, LayerStorage.ignoreRaycast);
				potionObj.name = $"{name}Potion";
				potionRenderer.name = $"{potionObj.name}_PotionRenderer";
				potionObj.gameObject.ConvertToPrefab(true);

				var potionRendBase = new GameObject(name + "Potion_RendererBase");
				potionRendBase.transform.SetParent(potionObj.transform);
				potionRendBase.transform.localPosition = Vector3.zero;

				potionRenderer.transform.SetParent(potionRendBase.transform);
				potionRenderer.transform.localPosition = Vector3.zero;

				var potion = potionObj.gameObject.AddComponent<T>();

				potion.entity = potionObj.gameObject.CreateEntity(2f, 2f, potionRendBase.transform);
				potion.entity.SetGrounded(false);
				potion.collider = (CapsuleCollider)potion.entity.collider;
				potion.audMan = potionObj.gameObject.CreatePropagatedAudioManager(45f, 75f);
				potion.audCrashOnGround = spillingNoise;
				potion.audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");
				potion.potionRenderer = potionRenderer;

				potion.splashRenderer = ObjectCreationExtensions.CreateSpriteBillboard(potionSpillingVisual, false);
				potion.splashRenderer.name = $"{potionObj.name}_SplashRenderer";
				potion.splashRenderer.gameObject.layer = 0;
				potion.splashRenderer.transform.SetParent(potionRendBase.transform);
				potion.splashRenderer.transform.localPosition = Vector3.down * 4.95f;
				potion.splashRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
				potion.splashRenderer.enabled = false;

				potionObj.renderers = potionObj.renderers.AddToArray(potion.splashRenderer);

				potion.sprPuddleVariant = potionSpillingVisual;

				potionsPre[potIdx++] = potion;

				return potion;
			}
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
			behaviorStateMachine.ChangeState(new ScienceTeacher_Wandering(this));
		}

		public void FoundPlayer()
		{
			audMan.PlayRandomAudio(audFindPlayer);
			navigator.maxSpeed = 17f;
			navigator.SetSpeed(17f);
		}

		public void Wander()
		{
			footStepAudMan.maintainLoop = true;
			footStepAudMan.SetLoop(true);
			footStepAudMan.QueueAudio(audWalking);
			navigator.maxSpeed = 10.2f;
			navigator.SetSpeed(10.2f);
		}

		public void ThrowPotion(Vector3 direction)
		{
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			footStepAudMan.FlushQueue(true);

			audMan.FlushQueue(true);
			audMan.PlaySingle(audComeOn);

			var pot = Instantiate(potionsPre[Random.Range(0, potionsPre.Length)]);
			pot.Initialize(gameObject, ec, transform.position, direction, throwSpeed, potionDespawnCooldown, potionThrowSpeed);
			renderer.sprite = sprNormal;

			StartCoroutine(WaitForPotToBreak(pot));
		}

		public void HasPotion() =>
			renderer.sprite = sprWithPotion;

		public void BeFrustratedForSpill()
		{
			renderer.sprite = sprUpset;
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audComplains);
		}

		IEnumerator WaitForPotToBreak(Potion pot)
		{
			while (pot && !pot.IsAPuddle)
				yield return null;

			behaviorStateMachine.ChangeState(pot ? new ScienceTeacher_WalkUpset(this) : new ScienceTeacher_Wandering(this)); // If pot stops existing, something is wrong lmao
		}

		public float RefillPotCooldown => Random.Range(minRefillPotCooldown, maxRefillPotCooldown);

		[SerializeField]
		internal Potion[] potionsPre;

		[SerializeField]
		internal PropagatedAudioManager audMan, footStepAudMan;

		[SerializeField]
		internal SoundObject[] audComplains, audFindPlayer;

		[SerializeField]
		internal SoundObject audWalking, audComeOn;

		[SerializeField]
		internal Sprite sprNormal, sprWithPotion, sprUpset;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal float potionThrowDistanceBuffer = 25f, throwSpeed = 20f, potionDespawnCooldown = 30f, potionThrowSpeed = 3f, minRefillPotCooldown = 45f, maxRefillPotCooldown = 75f;

	}

	internal class ScienceTeacher_StateBase(ScienceTeacher sci) : NpcState(sci)
	{
		protected ScienceTeacher sci = sci;
		public override void DoorHit(StandardDoor door)
		{
			if (door.locked)
			{
				door.Unlock();
				door.OpenTimed(5f, false);
				return;
			}
			base.DoorHit(door);
		}
	}

	internal class ScienceTeacher_Wandering(ScienceTeacher sci) : ScienceTeacher_StateBase(sci)
	{
		public override void Enter()
		{
			base.Enter();
			sci.HasPotion();
			sci.Wander();
			ChangeNavigationState(new NavigationState_WanderRandom(sci, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			sci.behaviorStateMachine.ChangeState(new ScienceTeacher_GoAfterPlayerToThrow(sci, player));
		}
	}

	internal class ScienceTeacher_GoAfterPlayerToThrow(ScienceTeacher sci, PlayerManager pm) : ScienceTeacher_StateBase(sci)
	{
		readonly PlayerManager pm = pm;
		NavigationState_TargetPlayer tarPla;
		public override void Enter()
		{
			base.Enter();
			sci.FoundPlayer();
			tarPla = new NavigationState_TargetPlayer(sci, 63, pm.transform.position);
			ChangeNavigationState(tarPla);
		}
		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (pm == player)
			{
				tarPla.UpdatePosition(player.transform.position);
				if (Vector3.Distance(player.transform.position, sci.transform.position) <= sci.potionThrowDistanceBuffer)
					sci.behaviorStateMachine.ChangeState(new ScienceTeacher_ThrowPotion(sci, pm));
			}
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			sci.behaviorStateMachine.ChangeState(new ScienceTeacher_Wandering(sci));
		}
		public override void Exit()
		{
			base.Exit();
			tarPla.priority = 0;
		}
	}

	internal class ScienceTeacher_ThrowPotion(ScienceTeacher sci, PlayerManager pm) : ScienceTeacher_StateBase(sci)
	{
		readonly PlayerManager pm = pm;

		public override void Enter()
		{
			base.Enter();
			sci.ThrowPotion((pm.transform.position - sci.transform.position).normalized);
			sci.SetGuilt(5f, "littering");
			ChangeNavigationState(new NavigationState_DoNothing(sci, 0));
		}
	}

	internal class ScienceTeacher_WalkUpset(ScienceTeacher sci) : ScienceTeacher_StateBase(sci)
	{
		float refillCooldown = sci.RefillPotCooldown;
		public override void Enter()
		{
			base.Enter();
			sci.BeFrustratedForSpill();
			sci.Wander();
			ChangeNavigationState(new NavigationState_WanderRandom(sci, 0));
		}

		public override void Update()
		{
			base.Update();
			refillCooldown -= sci.TimeScale * Time.deltaTime;
			if (refillCooldown <= 0f)
			{
				sci.behaviorStateMachine.ChangeState(new ScienceTeacher_Wandering(sci));
			}
		}
	}
}
