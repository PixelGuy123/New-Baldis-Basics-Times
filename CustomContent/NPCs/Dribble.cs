﻿
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class Dribble : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [ // huge ass array lmao
			this.GetSound("bounce.wav", "BB_Bong", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSoundNoSub("throw.wav", SoundType.Voice),
			this.GetSound("DRI_Idle1.wav", "Vfx_Dribble_Idle1", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Idle2.wav", "Vfx_Dribble_Idle2", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Chase1.wav", "Vfx_Dribble_Notice1", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Chase2.wav", "Vfx_Dribble_Notice2", SoundType.Voice,new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Caught1.wav", "Vfx_Dribble_Caught1", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Caught2.wav", "Vfx_Dribble_Caught2", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Clap.wav", "Vfx_Dribble_Clap", SoundType.Voice, Color.white),
			this.GetSound("DRI_Instructions.wav", "Vfx_Dribble_Instructions", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)), // 9
			this.GetSound("DRI_Ready.wav", "Vfx_Dribble_Ready", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Catch.wav", "Vfx_Dribble_Catch", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Praise1.wav", "Vfx_Dribble_Praise1", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Praise2.wav", "Vfx_Dribble_Praise2", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Dismissed.wav", "Vfx_Dribble_Dismissed", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSoundNoSub("punch.wav", SoundType.Voice),
			this.GetSound("DRI_Disappointed1.wav", "Vfx_Dribble_Disappointed1", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Disappointed2.wav", "Vfx_Dribble_Disappointed2", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Angry1.wav", "Vfx_Dribble_Angry1", SoundType.Voice, new(1f, 0.15f, 0.15f)), // 18
			this.GetSound("DRI_Angry2.wav", "Vfx_Dribble_Angry2", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			this.GetSound("DRI_Step1.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_Step2.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.99609375f, 0.609375f, 0.3984375f)),
			this.GetSound("DRI_AngryChase1.wav", "Vfx_Dribble_ChaseAngry1", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			this.GetSound("DRI_AngryChase2.wav", "Vfx_Dribble_ChaseAngry2", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			this.GetSound("DRI_AngryCaught1.wav", "Vfx_Dribble_CaughtAngry1", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			this.GetSound("DRI_AngryCaught2.wav", "Vfx_Dribble_CaughtAngry2", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			this.GetSound("DRI_AngryPush1.wav", "Vfx_Dribble_Punch1", SoundType.Voice, new(1f, 0.15f, 0.15f)),
			this.GetSound("DRI_AngryPush2.wav", "Vfx_Dribble_Punch2", SoundType.Voice, new(1f, 0.15f, 0.15f))
			];

			audMan = GetComponent<PropagatedAudioManager>();
			bounceAudMan = gameObject.CreatePropagatedAudioManager(85f, 125f);
			audBounceBall = soundObjects[0];
			audThrow = soundObjects[1];
			audIdle = [soundObjects[2], soundObjects[3]];
			audNotice = [soundObjects[4], soundObjects[5]];
			audCaught = [soundObjects[6], soundObjects[7]];
			audClap = soundObjects[8];
			audInstructions = soundObjects[9];
			audReady = soundObjects[10];
			audCatch = soundObjects[11];
			audPraise = [soundObjects[12], soundObjects[13]];
			audDismissed = soundObjects[14];
			audDisappointed = [soundObjects[16], soundObjects[17]];
			audAngry = [soundObjects[18], soundObjects[19]];
			audStep = [soundObjects[20], soundObjects[21]];
			audChaseAngry = [soundObjects[22], soundObjects[23]];
			audAngryCaught = [soundObjects[24], soundObjects[25]];
			audPunchResponse = [soundObjects[26], soundObjects[27]];
			audPunch = soundObjects[15];

			renderer = spriteRenderer[0];

			var storedSprites = this.GetSpriteSheet(13, 1, pixelsPerUnit, "dribbleSpriteSheet.png");
			spriteRenderer[0].sprite = storedSprites[0];
			idleSprs = [storedSprites[0], storedSprites[1]];
			clapSprs = [storedSprites[5], storedSprites[6]];
			classSprs = [storedSprites[2], storedSprites[3], storedSprites[4]];
			disappointedSprs = [storedSprites[7], storedSprites[8]];
			crazySprs = [storedSprites[9], storedSprites[10]];
			chasingSprs = [storedSprites[11], storedSprites[12]];

			var basket = new GameObject("DribbleBasketBall");

			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(BBTimesManager.man.Get<Sprite[]>("basketBall")[0]);
			rendererBase.transform.SetParent(basket.transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.name = "sprite";
			basket.ConvertToPrefab(true);

			var comp = basket.AddComponent<PickableBasketball>();
			comp.gameObject.layer = LayerStorage.iClickableLayer;
			comp.entity = basket.CreateEntity(2f, 2f, basket.transform);
			comp.spriteAnim = BBTimesManager.man.Get<Sprite[]>("basketBall");
			comp.audHit = soundObjects[15];

			comp.renderer = rendererBase;

			basketPre = comp;
		}

		const float pixelsPerUnit = 87f;
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		public Character[] ReplacementNpcs { get; set; }
		public int ReplacementWeight { get; set; }

		// ---------------------------------------------------------------


		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = normSpeed;
			navigator.SetSpeed(normSpeed);
			behaviorStateMachine.ChangeState(new Dribble_Idle(this));
			Home = ec.CellFromPosition(transform.position).room;
			basketball = Instantiate(basketPre);
			basketball.Initialize(this);
		}

		internal void Bounce() =>
			bounceAudMan.PlaySingle(audBounceBall);

		internal void IdleNoise() =>
			audMan.PlayRandomAudio(audIdle);

		internal void NoticeNoise() =>
			audMan.PlayRandomAudio(audNotice);

		internal void ComingNoise() =>
			audMan.PlayRandomAudio(audCaught);
		internal void Disappointed() =>
			audMan.PlayRandomAudio(audDisappointed);
		internal void AngryNoise(bool chasing) =>
			audMan.PlayRandomAudio(chasing ? audChaseAngry : audAngry);

		internal void Clap() =>
			audMan.PlaySingle(audClap);

		internal void TeleportToClass(PlayerManager pm)
		{
			navigator.Entity.Teleport(ec.RealRoomMid(Home));
			pm.Teleport(Home.RandomEntitySafeCellNoGarbage().CenterWorldPosition);
			Physics.SyncTransforms();
			pm.transform.LookAt(transform);
		}

		internal void ThrowBasketball(PlayerManager pm)
		{
			Vector3 rot = (pm.transform.position - transform.position).normalized;
			basketball.Throw(rot, transform.position + (rot.ZeroOutY() * 1.5f), pm, Mathf.Max(0.125f, 0.7f / (succeededMinigames + 1)), 
				Random.Range(Mathf.Max(35f, 35f * ((succeededMinigames + 1) * 0.2f)), Mathf.Max(65f, 65f * ((succeededMinigames + 1) * 0.6f))));
		}

		internal void MinigameEnd(bool failed, PlayerManager player)
		{
			if (behaviorStateMachine.CurrentState is Dribble_Chase) return; // When he's already chasing, this serves no purpose
			if (!failed)
			{
				succeededMinigames++;
				Singleton<CoreGameManager>.Instance.AddPoints(Mathf.Min(200, (int)(100 * ((succeededMinigames + 1) * 0.5f))), player.playerNumber, true);
				behaviorStateMachine.ChangeState(new Dribble_MinigameSucceed(this));
				return;
			}
			minigameRecord = succeededMinigames;
			succeededMinigames = 0;
			behaviorStateMachine.ChangeState(new Dribble_MinigameFail(this, player));
		}

		internal void DisappointDribble()
		{
			minigameRecord = succeededMinigames;
			succeededMinigames = 0;
			behaviorStateMachine.ChangeState(new Dribble_Disappointed(this));
		}
		internal void Step()
		{
			_step = !_step;
			bounceAudMan.PlaySingle(_step ? audStep[0] : audStep[1]);
		}
		internal void PunchNPC(Entity entity)
		{
			bounceAudMan.PlaySingle(audPunch);
			audMan.PlayRandomAudio(audPunchResponse);
			float f = Random.Range(9f, 12f);
			float fFactor = (1 + minigameRecord) * 2.5f;
			entity.AddForce(new Force((Random.value < 0.5f ? transform.right : -transform.right) * fFactor, f, -(f - fFactor)));
			entity.StartCoroutine(Punched(entity));
		}

		IEnumerator Punched(Entity entity)
		{
			var mod = new MovementModifier(Vector3.zero, 0.35f / (1 + minigameRecord));
			entity.ExternalActivity.moveMods.Add(mod);
			float cool = 5f;
			while (cool > 0f)
			{
				cool -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}
			entity.ExternalActivity.moveMods.Remove(mod);
			yield break;
		}

		public override void Despawn()
		{
			base.Despawn();
			Destroy(basketball);
		}


		[SerializeField]
		internal Sprite[] idleSprs, clapSprs, classSprs, disappointedSprs, crazySprs, chasingSprs;

		[SerializeField]
		internal SoundObject[] audIdle, audNotice, audPraise, audDisappointed, audAngry, audChaseAngry, audCaught, audStep, audAngryCaught, audPunchResponse;

		[SerializeField]
		internal SoundObject audCatch, audClap, audDismissed, audInstructions, audReady, audBounceBall, audThrow, audPunch;

		[SerializeField]
		internal PropagatedAudioManager audMan, bounceAudMan;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal PickableBasketball basketPre;

		PickableBasketball basketball;
		bool _step = false;


		readonly internal TimeScaleModifier introMod = new(0f, 0f, 0f);

		readonly internal MovementModifier moveMod = new(Vector3.zero, 0f);

		internal float normSpeed = 14f, chaseSpeed = 21f, angryChaseSpeed = 22.5f;

		int succeededMinigames = 0;
		int minigameRecord = 0;

		internal RoomController Home { get; private set; }
	}

	internal class DribbleStateBase(Dribble dr) : NpcState(dr)
	{
		readonly protected Dribble dr = dr;

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

	internal class DribbleWanderStateBase(Dribble dr) : DribbleStateBase(dr)
	{
		public override void Update()
		{
			base.Update();
			float mag = dr.Navigator.Velocity.magnitude;
			if (Time.timeScale > 0f && mag > 0.1f * Time.deltaTime)
			{
				stepDelay -= mag;
				if (stepDelay <= 0f)
				{
					stepDelay += 5f;
					step = !step;
					if (step)
						dr.Bounce();
					dr.renderer.sprite = dr.idleSprs[step ? 1 : 0];
				}
			}
		}

		float stepDelay = 5f;
		bool step = false;
	}

	internal class Dribble_Idle(Dribble dr, float cooldown = 0f) : DribbleWanderStateBase(dr)
	{
		float cooldown = cooldown;

		public override void Enter()
		{
			base.Enter();
			dr.Navigator.maxSpeed = dr.normSpeed;
			dr.Navigator.SetSpeed(dr.normSpeed);
			dr.Navigator.Am.moveMods.Remove(dr.moveMod);
			ChangeNavigationState(new NavigationState_WanderRandom(dr, 0));
		}

		public override void Update()
		{
			base.Update();
			sayCooldown -= Time.deltaTime * dr.TimeScale;
			if (sayCooldown <= 0f)
			{
				sayCooldown += Random.Range(15f, 30f);
				if (Random.value > 0.7f)
					dr.IdleNoise();
			}

			if (cooldown > 0f)
				cooldown -= Time.deltaTime * dr.TimeScale;
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (!player.Tagged && cooldown <= 0f)
				dr.behaviorStateMachine.ChangeState(new Dribble_NoticeChase(dr, player));
		}

		float sayCooldown = Random.Range(15f, 30f);
	}

	internal class Dribble_NoticeChase(Dribble dr, PlayerManager player) : DribbleWanderStateBase(dr)
	{
		readonly PlayerManager player = player;
		NavigationState_TargetPlayer state;
		public override void Enter()
		{
			base.Enter();
			dr.NoticeNoise();
			dr.Navigator.maxSpeed = dr.chaseSpeed;
			dr.Navigator.SetSpeed(dr.chaseSpeed);
			state = new NavigationState_TargetPlayer(dr, 63, player.transform.position, true);
			ChangeNavigationState(state);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == this.player)
				state.UpdatePosition(player.transform.position);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == player.gameObject)
				dr.behaviorStateMachine.ChangeState(new Dribble_Inform(dr, player));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			state.priority = 0;
			dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr));
		}

		public override void Exit()
		{
			base.Exit();
			state.priority = 0;
		}
	}

	internal class Dribble_Inform(Dribble dr, PlayerManager player) : DribbleStateBase(dr)
	{
		readonly PlayerManager player = player;
		public override void Enter()
		{
			base.Enter();
			dr.ec.AddTimeScale(dr.introMod);
			dr.ComingNoise();
			dr.StartCoroutine(WaitForInform());
		}

		IEnumerator WaitForInform()
		{
			dr.renderer.sprite = dr.clapSprs[0];
			dr.Navigator.Am.moveMods.Add(dr.moveMod);
			player.plm.am.moveMods.Add(dr.moveMod);
			while (dr.audMan.QueuedAudioIsPlaying)
			{
				player.transform.RotateSmoothlyToNextPoint(dr.transform.position, 0.8f);
				yield return null;
			}
			player.plm.am.moveMods.Remove(dr.moveMod);
			float cool = Random.Range(1f, 1.5f);
			while (cool > 0f)
			{
				player.transform.RotateSmoothlyToNextPoint(dr.transform.position, 0.8f);
				cool -= Time.deltaTime;
				yield return null;
			}
			dr.renderer.sprite = dr.clapSprs[1];
			dr.Clap();
			dr.TeleportToClass(player);
			dr.ec.RemoveTimeScale(dr.introMod);

			for (int i = 0; i < 3; i++) // Frame delay
				yield return null;

			dr.renderer.sprite = dr.clapSprs[0];
			dr.behaviorStateMachine.ChangeState(new Dribble_ClassTime(dr, player));

			yield break;
		}


	}

	internal class Dribble_ClassTime(Dribble dr, PlayerManager pm) : DribbleStateBase(dr)
	{
		readonly PlayerManager player = pm;
		Coroutine classEnum;

		public override void Enter()
		{
			base.Enter();
			classEnum = dr.StartCoroutine(ClassTime());
		}

		public override void Update()
		{
			base.Update();
			if (dr.ec.CellFromPosition(player.transform.position).room != dr.Home)
			{
				dr.audMan.FlushQueue(true);
				dr.bounceAudMan.FlushQueue(true);
				dr.behaviorStateMachine.ChangeState(new Dribble_Chase(dr, player));
				dr.StopCoroutine(classEnum);
			}
		}

		IEnumerator ClassTime()
		{
			float cool = Random.Range(1.5f, 2f);

			while (cool > 0f)
			{
				cool -= Time.deltaTime;
				yield return null;
			}
			dr.renderer.sprite = dr.classSprs[0];
			dr.audMan.QueueAudio(dr.audInstructions);
			while (dr.audMan.QueuedAudioIsPlaying)
				yield return null;
			cool = 0.5f;
			while (cool > 0f)
			{
				cool -= dr.TimeScale * Time.deltaTime;
				yield return null;
			}

			dr.audMan.QueueAudio(dr.audReady);
			dr.renderer.sprite = dr.classSprs[1];

			while (dr.audMan.QueuedAudioIsPlaying)
				yield return null;

			cool = Random.Range(1.5f, 2.5f);
			while (cool > 0f)
			{
				cool -= dr.TimeScale * Time.deltaTime;
				yield return null;
			}

			dr.bounceAudMan.PlaySingle(dr.audThrow);
			dr.audMan.QueueAudio(dr.audCatch);
			dr.renderer.sprite = dr.classSprs[2];
			dr.ThrowBasketball(player);

			yield break;
		}
	}

	internal class Dribble_MinigameSucceed(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		bool clapped = false;
		float cooldown = 5f;
		public override void Enter()
		{
			base.Enter();
			dr.bounceAudMan.FlushQueue(true);
			dr.bounceAudMan.QueueRandomAudio(dr.audPraise);
		}
		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 11f;
			frame %= dr.clapSprs.Length;
			if (!clapped)
			{
				if (frame < 1f)
				{
					dr.audMan.PlaySingle(dr.audClap);
					clapped = true;
				}
			}
			else if (frame > 1f)
				clapped = false;

			dr.renderer.sprite = dr.clapSprs[Mathf.FloorToInt(frame)];

			cooldown -= Time.deltaTime * dr.TimeScale;
			if (cooldown < 0f)
			{
				dr.audMan.FlushQueue(true);
				dr.audMan.PlaySingle(dr.audDismissed);
				dr.renderer.sprite = dr.idleSprs[0];
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, Random.Range(60f, 90f)));
			}
		}
	}

	internal class Dribble_Disappointed(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		float cooldown = 2.5f;
		public override void Enter()
		{
			base.Enter();
			dr.audMan.FlushQueue(true);
			dr.Disappointed();
		}

		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 6f;
			frame %= dr.disappointedSprs.Length;
			dr.renderer.sprite = dr.disappointedSprs[Mathf.FloorToInt(frame)];
			if (!dr.audMan.QueuedAudioIsPlaying)
			{
				cooldown -= Time.deltaTime * dr.TimeScale;
				if (cooldown < 0f)
				{
					dr.audMan.FlushQueue(true);
					dr.audMan.PlaySingle(dr.audDismissed);
					dr.renderer.sprite = dr.idleSprs[0];
					dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, Random.Range(60f, 90f)));
				}
			}

		}
	}

	internal class Dribble_AngrySwingingBase(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 11f;
			frame %= dr.crazySprs.Length;

			dr.renderer.sprite = dr.crazySprs[Mathf.FloorToInt(frame)];
		}
	}

	internal class Dribble_MinigameFail(Dribble dr, PlayerManager pm) : Dribble_AngrySwingingBase(dr)
	{
		readonly PlayerManager pm = pm;
		public override void Enter()
		{
			base.Enter();
			dr.ec.MakeNoise(dr.transform.position, 36);
			dr.AngryNoise(false);
		}
		public override void Update()
		{
			base.Update();
			if (dr.ec.CellFromPosition(pm.transform.position).room != dr.Home)
				dr.behaviorStateMachine.ChangeState(new Dribble_Chase(dr, pm));
		}


	}

	internal class Dribble_Chase(Dribble dr, PlayerManager pm) : DribbleStateBase(dr) // Pretty much what dr reflex does, but not squish
	{
		NavigationState_TargetPlayer state;
		readonly PlayerManager pm = pm;
		float stepDelay = 0f;
		int idx = 0;

		public override void Enter()
		{
			base.Enter();
			dr.renderer.sprite = dr.chasingSprs[0];
			dr.Navigator.Am.moveMods.Remove(dr.moveMod);
			dr.Navigator.maxSpeed = dr.angryChaseSpeed;
			dr.Navigator.SetSpeed(dr.angryChaseSpeed);
			dr.AngryNoise(true);
			state = new NavigationState_TargetPlayer(dr, 63, pm.transform.position);
			ChangeNavigationState(state);
			dr.Navigator.passableObstacles.Add(PassableObstacle.Bully);
		}

		public override void Exit()
		{
			base.Exit();
			dr.Navigator.passableObstacles.Remove(PassableObstacle.Bully);
			ChangeNavigationState(new NavigationState_DoNothing(dr, 0));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			
			ChangeNavigationState(new NavigationState_WanderRandom(dr, 0));
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.isTrigger)
			{
				if (other.CompareTag("Player"))
				{
					var pm = other.GetComponent<PlayerManager>();
					if (pm && pm == this.pm)
						dr.behaviorStateMachine.ChangeState(new Dribble_ForceRun(dr, pm));
				}
				else if (other.CompareTag("NPC"))
				{
					var e = other.GetComponent<Entity>();
					if (e)
						dr.PunchNPC(e);
					
				}
			}
		}

		public override void Hear(Vector3 position, int value)
		{
			base.Hear(position, value);
			if (!dr.looker.PlayerInSight())
			{
				ChangeNavigationState(state);
				state.UpdatePosition(position);
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (pm == player)
			{
				ChangeNavigationState(state);
				state.UpdatePosition(player.transform.position);
			}
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (pm == player && !dr.audMan.QueuedAudioIsPlaying && Random.value > 0.6f)
				dr.AngryNoise(true);
		}

		public override void Update()
		{
			base.Update();
			float mag = dr.Navigator.Velocity.magnitude;
			if (Time.timeScale > 0f && mag > 0.1f * Time.deltaTime)
			{
				stepDelay -= mag;
				if (stepDelay <= 0f)
				{
					stepDelay += 5.6f;
					dr.Step();
					idx = 1 - idx;
					dr.renderer.sprite = dr.chasingSprs[idx];
				}
			}
		}
	}

	internal class Dribble_ForceRun(Dribble dr, PlayerManager pm) : Dribble_AngrySwingingBase(dr)
	{
		readonly private PlayerManager pm = pm;
		private PlayerRunCornerFunction func;

		public override void Enter()
		{
			base.Enter();
			pm.plm.AddStamina(45f, true);
			dr.Navigator.Am.moveMods.Add(dr.moveMod);
			dr.Navigator.Entity.Teleport(dr.ec.RealRoomMid(dr.Home));
			func = dr.Home.functionObject.GetComponent<PlayerRunCornerFunction>();
			func.MakePlayerRunAround(pm);
			dr.audMan.FlushQueue(true);
			dr.audMan.QueueRandomAudio(dr.audAngryCaught);
			dr.ec.MakeNoise(dr.transform.position, 39);
		}

		public override void Update()
		{
			base.Update();

			if (!func.IsActive)
			{
				dr.audMan.FlushQueue(true);
				dr.audMan.PlaySingle(dr.audDismissed);
				dr.renderer.sprite = dr.idleSprs[0];
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, Random.Range(60f, 90f)));
			}
		}
	}
}
