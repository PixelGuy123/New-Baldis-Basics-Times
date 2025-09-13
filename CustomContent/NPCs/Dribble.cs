using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Manager;
using BBTimes.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Dribble : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			Color normalDribbleColor = new(0.99609375f, 0.609375f, 0.3984375f),
			angryDribbleColor = new(1f, 0.15f, 0.15f);

			var audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");
			var hit = BBTimesManager.man.Get<SoundObject>("audGenericPunch_NoSub");

			audClaps = [
			this.GetSound("DRI_Clap1.wav", "Vfx_Dribble_Clap", SoundType.Effect, normalDribbleColor),
			this.GetSound("DRI_Clap2.wav", "Vfx_Dribble_Clap", SoundType.Effect, normalDribbleColor)
			];

			audMan = GetComponent<PropagatedAudioManager>();
			bounceAudMan = gameObject.CreatePropagatedAudioManager(30f, 165f);
			clapAudMan = gameObject.CreatePropagatedAudioManager(30f, 165f);
			audBounceBall = this.GetSound("bounce.wav", "BB_Bong", SoundType.Voice, normalDribbleColor);
			this.audThrow = audThrow;
			audIdle = [(this.GetSound("DRI_Idle1.wav", "Vfx_Dribble_Idle1", SoundType.Voice, normalDribbleColor)), (this.GetSound("DRI_Idle2.wav", "Vfx_Dribble_Idle2", SoundType.Voice, normalDribbleColor))];
			audNotice = [(this.GetSound("DRI_Chase1.wav", "Vfx_Dribble_Notice1", SoundType.Voice, normalDribbleColor)), (this.GetSound("DRI_Chase2.wav", "Vfx_Dribble_Notice2", SoundType.Voice, normalDribbleColor))];
			audCaught = [(this.GetSound("DRI_Caught1.wav", "Vfx_Dribble_Caught1", SoundType.Voice, normalDribbleColor)), (this.GetSound("DRI_Caught2.wav", "Vfx_Dribble_Caught2", SoundType.Voice, normalDribbleColor))];
			audInstructions = this.GetSound("DRI_Instructions.wav", "Vfx_Dribble_Instructions", SoundType.Voice, normalDribbleColor);
			audReady = this.GetSound("DRI_Ready.wav", "Vfx_Dribble_Ready", SoundType.Voice, normalDribbleColor);
			audCatch = this.GetSound("DRI_Catch.wav", "Vfx_Dribble_Catch", SoundType.Voice, normalDribbleColor);
			audPraise = [(this.GetSound("DRI_Praise1.wav", "Vfx_Dribble_Praise1", SoundType.Voice, normalDribbleColor)), (this.GetSound("DRI_Praise2.wav", "Vfx_Dribble_Praise2", SoundType.Voice, normalDribbleColor))];
			audDismissed = this.GetSound("DRI_Dismissed.wav", "Vfx_Dribble_Dismissed", SoundType.Voice, normalDribbleColor);
			audDisappointed = [(this.GetSound("DRI_Disappointed1.wav", "Vfx_Dribble_Disappointed1", SoundType.Voice, normalDribbleColor)), (this.GetSound("DRI_Disappointed2.wav", "Vfx_Dribble_Disappointed2", SoundType.Voice, normalDribbleColor))];
			audAngry = [(this.GetSound("DRI_Angry1.wav", "Vfx_Dribble_Angry1", SoundType.Voice, angryDribbleColor)), (this.GetSound("DRI_Angry2.wav", "Vfx_Dribble_Angry2", SoundType.Voice, angryDribbleColor))];
			audStep = [(this.GetSound("DRI_Step1.wav", "Vfx_Spj_Step", SoundType.Voice, normalDribbleColor)), (this.GetSound("DRI_Step2.wav", "Vfx_Spj_Step", SoundType.Voice, normalDribbleColor))];
			audChaseAngry = [(this.GetSound("DRI_AngryChase1.wav", "Vfx_Dribble_ChaseAngry1", SoundType.Voice, angryDribbleColor)), (this.GetSound("DRI_AngryChase2.wav", "Vfx_Dribble_ChaseAngry2", SoundType.Voice, angryDribbleColor))];
			audAngryCaught = [(this.GetSound("DRI_AngryCaught1.wav", "Vfx_Dribble_CaughtAngry1", SoundType.Voice, angryDribbleColor)), (this.GetSound("DRI_AngryCaught2.wav", "Vfx_Dribble_CaughtAngry2", SoundType.Voice, angryDribbleColor))];
			audPunchResponse = [(this.GetSound("DRI_AngryPush1.wav", "Vfx_Dribble_Punch1", SoundType.Voice, angryDribbleColor)), (this.GetSound("DRI_AngryPush2.wav", "Vfx_Dribble_Punch2", SoundType.Voice, angryDribbleColor))];
			audPunch = hit;
			audHey = this.GetSound("DRI_OutOfSight.wav", "Vfx_Dribble_OutOfSight", SoundType.Voice, normalDribbleColor);
			audOkayThen = this.GetSound("DRI_TakeBackChaseFail.wav", "Vfx_Dribble_TakeBackChaseFail", SoundType.Voice, normalDribbleColor);
			audTakeBack = this.GetSound("DRI_TakeBackChase.wav", "Vfx_Dribble_TakeBackChase", SoundType.Voice, normalDribbleColor);

			renderer = spriteRenderer[0];

			// Normal
			var storedSprites = this.GetSpriteSheet(13, 1, pixelsPerUnit, "dribbleSpriteSheet.png");
			spriteRenderer[0].sprite = storedSprites[0];
			idleSprs = [storedSprites[0], storedSprites[1]];
			clapSprs = [storedSprites[5], storedSprites[6]];
			classSprs = [storedSprites[2], storedSprites[3], storedSprites[4]];
			disappointedSprs = [storedSprites[7], storedSprites[8]];
			crazySprs = [storedSprites[9], storedSprites[10]];
			chasingSprs = [storedSprites[11], storedSprites[12]];

			// Talking
			storedSprites = this.GetSpriteSheet(13, 1, pixelsPerUnit, "dribbleSpriteSheet_2.png");
			idleSprsTalking = [storedSprites[0], storedSprites[1]];
			clapSprsTalking = [storedSprites[5], storedSprites[6]];
			classSprsTalking = [storedSprites[2], storedSprites[3], storedSprites[4]];
			disappointedSprsTalking = [storedSprites[7], storedSprites[8]];
			crazySprsTalking = [storedSprites[9], storedSprites[10]];
			chasingSprsTalking = [storedSprites[11], storedSprites[12]];

			// Secret
			storedSprites = this.GetSpriteSheet(3, 1, pixelsPerUnit, "DribbleSecret.png");
			secretHappySprs = [storedSprites[0]];
			secretAngrySwingingSprs = [storedSprites[1], storedSprites[2]];

			// Secret Talking
			storedSprites = this.GetSpriteSheet(3, 1, pixelsPerUnit, "DribbleSecret_Speaking.png");
			secretHappyTalkingSprs = [storedSprites[0]];
			secretAngrySwingingTalkingSprs = [storedSprites[1], storedSprites[2]];

			var basket = new GameObject("DribbleBasketBall");

			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(BBTimesManager.man.Get<Sprite[]>("basketBall")[0]);
			rendererBase.transform.SetParent(basket.transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.name = "sprite";
			rendererBase.gameObject.AddComponent<BillboardRotator>().invertFace = true; // add a rotator for the indicator renderer below
			basket.ConvertToPrefab(true);

			var comp = basket.AddComponent<PickableBasketball>();
			comp.gameObject.layer = LayerStorage.iClickableLayer;
			comp.entity = basket.CreateEntity(2f, 2f, basket.transform);
			comp.entity.SetGrounded(false);
			comp.audHit = hit;
			comp.audThrow = audThrow;

			comp.canBeClickedIndicatorRenderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(rendererBase.sprite.pixelsPerUnit, "basketball_marker.png"));
			comp.canBeClickedIndicatorRenderer.enabled = false;
			comp.canBeClickedIndicatorRenderer.transform.SetParent(rendererBase.transform);
			comp.canBeClickedIndicatorRenderer.transform.localPosition = Vector3.forward * 0.5f;

			comp.renderer = rendererBase;

			var animComp = comp.renderer.gameObject.AddComponent<AnimationComponent>();
			animComp.animation = BBTimesManager.man.Get<Sprite[]>("basketBall");
			animComp.renderers = [comp.renderer];
			animComp.speed = 8f;
			animComp.autoStart = true;

			basketPre = comp;

			gaugeSprite = this.GetSprite(Storage.GaugeSprite_PixelsPerUnit, "gaugeIcon.png");
		}

		const float pixelsPerUnit = 48f;
		public void SetupPrefabPost()
		{
			basketballItem = ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("Basketball"), BBTimesManager.plug.Info).value;
			basketPre.gaugeSprite = basketballItem.itemSpriteSmall;
		}
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// ---------------------------------------------------------------


		public override void Initialize()
		{
			base.Initialize();
			// Setup
			navigator.maxSpeed = normSpeed;
			navigator.SetSpeed(normSpeed);
			behaviorStateMachine.ChangeState(new Dribble_Idle(this));

			RoomController altRoom = null;

			// Home Setup
			bool foundCat = false;
			bool foundOtherClassroomCategory = false;
			for (int i = 0; i < ec.rooms.Count; i++)
			{
				if (ec.rooms[i].category == expectedCategory)
				{
					foundCat = true;
					Home = ec.rooms[i];
					break;
				}

				if (ec.rooms[i].category == RoomCategory.Class)
				{
					foundOtherClassroomCategory = true;
					altRoom = ec.rooms[i];
				}
			}

			if (!foundCat) // Fail safe
				Home = foundOtherClassroomCategory ? altRoom : Home = ec.CellFromPosition(transform.position).room;

			// Initializes Basketball Instance
			basketball = Instantiate(basketPre);
			basketball.Initialize(this, Home.objectObject.GetComponentsInChildren<BasketballHoopMarker>());
		}
		public void ResetMinigameRecord() => minigameRecord = 0;
		public void FinishPunishment(int streakCount) =>
			StartCoroutine(TirePlayer(streakCount));


		internal void Bounce(float speed)
		{
			bounceAudMan.PlaySingle(audBounceBall);
			navigator.SetSpeed(speed * Random.Range(basketBallBounce_minSlowDownFactor, basketBallBounce_maxSlowDownFactor));
		}

		internal void IdleNoise() =>
			audMan.PlayRandomAudio(audIdle);

		internal void NoticeNoise(bool sightEarly)
		{
			audMan.FlushQueue(true);
			if (sightEarly)
			{
				audMan.QueueAudio(audHey);
				return;
			}
			audMan.QueueRandomAudio(audNotice);
		}

		internal void ComingNoise()
		{
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audCaught);
		}
		internal void Disappointed()
		{
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audDisappointed);
		}
		internal void AngryNoise(bool chasing) =>
			audMan.PlayRandomAudio(chasing ? audChaseAngry : audAngry);
		internal void PrepToLeaveNoise() // Awaiting audio of Dribble saying that
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audTakeBack);
		}
		internal void OkThen()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audOkayThen);
			audMan.QueueAudio(audDismissed);
		}

		internal void Clap()
		{
			clapAudMan.PlaySingle(audClaps[clapIndex]);
			clapIndex = (clapIndex + 1) % audClaps.Length;
		}

		public void TeleportToClass(PlayerManager pm)
		{
			activePlayer = pm;
			selfPreviousSpot = transform.position;
			var middle = ec.CellFromPosition(ec.RealRoomMid(Home));

			navigator.Entity.Teleport(
				middle.Null ? // Failsafe, if the room has no middle
			Home.RandomEntitySafeCellNoGarbage().CenterWorldPosition :
			middle.CenterWorldPosition);

			pm.Teleport(Home.RandomEntitySafeCellNoGarbage().CenterWorldPosition);
			pm.transform.LookAt(transform);
		}

		public void TeleportBack(PlayerManager pm)
		{
			if (selfPreviousSpot == default) return;

			navigator.Entity.Teleport(selfPreviousSpot);
			pm.Teleport(selfPreviousSpot);
		}

		internal void KickEveryoneOutOfRoom() => Home.functionObject.GetComponent<DribbleGymFunction>()?.KickOutEveryoneFromRoom(this);
		internal void UpdateMinigameCounter() => Home.functionObject.GetComponent<DribbleGymFunction>()?.UpdatePoster(succeededMinigames);
		internal void UnlockEverything() => Home.functionObject.GetComponent<DribbleGymFunction>()?.UnlockEverything();
		internal void PunishPlayerToRun() => StartCoroutine(MakePlayerRun());

		internal void ThrowBasketball(PlayerManager pm)
		{
			int streak = Mathf.Max(1, succeededMinigames + 1); // + 1 because this is called before the addition
			float speedFactor = 1f + (streak * 0.75f);
			float slowDownFactor = 0.7f / streak;

			Vector3 rot = (pm.transform.position - transform.position).normalized;
			basketball.Throw(
				rot, // Rotation
				transform.position + (rot.ZeroOutY() * 1.5f), // Position
				pm, // Player
				Mathf.Max(0.125f, slowDownFactor), // The slowdown effect [0 - 1]
				speedFactor * minigame_baseBasketballSpeed + (speedFactor * 0.5f * Random.Range(-minigame_randomBasketballRange, minigame_randomBasketballRange)) // Speed
				);
		}

		internal void MinigameEnd(bool failed, PlayerManager player)
		{
			if (behaviorStateMachine.CurrentState is Dribble_Chase) return; // When he's already chasing, this serves no purpose

			if (!failed) // If succeeded minigame
			{
				succeededMinigames++;

				for (int i = 0; i < succeededMinigames; i++)
				{
					Vector3 position = transform.position + (Random.insideUnitSphere * Random.Range(1f, 2.75f));

					if (ec.CellFromPosition(position).Null) // Failsafe
						position = transform.position;
					// Actually drop the pickup
					SpawnBasketballItem(new(position.x, position.z));
				}

				behaviorStateMachine.ChangeState(new Dribble_MinigameSucceed(this, player));
				return;
			}
			// THIS makes him angry
			UnlockEverything();
			minigameRecord = succeededMinigames;
			succeededMinigames = 0;
			behaviorStateMachine.ChangeState(new Dribble_MinigameFail(this, player));
		}

		internal void DisappointDribble(PlayerManager pm) // This one makes him disappointed (but not angry)
		{
			minigameRecord = succeededMinigames;
			succeededMinigames = 0;
			behaviorStateMachine.ChangeState(new Dribble_Disappointed(this, pm));
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

		public override void Despawn()
		{
			base.Despawn();
			gauge?.Deactivate();
			if (activePlayer)
			{
				activePlayer.plm.GetModifier().RemoveModifier(staminaMod);
				activePlayer.Am.moveMods.Remove(playerMoveMod);

				var attr = activePlayer.GetAttribute();
				attr.RemoveAttribute(Storage.ATTR_FREEZE_PLAYER_MOVEMENT_TAG);
				attr.RemoveAttribute(Storage.ATTR_FREEZE_STAMINA_UPDATE_TAG);
			}
			Destroy(basketball);
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate(); // YES, A CHAIN OF ELSE IFs, IN ALL OF ITS GLORY
			if (currentArrayInUsage == idleSprs)
				ChangeToTalkingVariantOrNot(idleSprsTalking);
			else if (currentArrayInUsage == clapSprs)
				ChangeToTalkingVariantOrNot(clapSprsTalking);
			else if (currentArrayInUsage == classSprs)
				ChangeToTalkingVariantOrNot(classSprsTalking);
			else if (currentArrayInUsage == disappointedSprs)
				ChangeToTalkingVariantOrNot(disappointedSprsTalking);
			else if (currentArrayInUsage == crazySprs)
				ChangeToTalkingVariantOrNot(crazySprsTalking);
			else if (currentArrayInUsage == chasingSprs)
				ChangeToTalkingVariantOrNot(chasingSprsTalking);
			else if (currentArrayInUsage == secretHappySprs)
				ChangeToTalkingVariantOrNot(secretHappyTalkingSprs);
			else if (currentArrayInUsage == secretAngrySwingingSprs)
				ChangeToTalkingVariantOrNot(secretAngrySwingingTalkingSprs);
		}

		void ChangeToTalkingVariantOrNot(Sprite[] talkingArray) =>
			renderer.sprite = audMan.AnyAudioIsPlaying ? talkingArray[idxInCurrentArray] : currentArrayInUsage[idxInCurrentArray];
		Pickup SpawnBasketballItem(Vector2 position)
		{
			var basketballPickup = ec.CreateItem(Home, basketballItem, new(position.x, position.y));
			ec.items.Remove(basketballPickup); // It's a standalone pickup for Dribble
			basketballPickup.AssignItem(basketballItem);
			return basketballPickup;
		}


		internal void ApplyArray(Sprite[] arrayToUse, int idx)
		{
			currentArrayInUsage = arrayToUse;
			renderer.sprite = arrayToUse[idx];
			idxInCurrentArray = idx;
		}

		IEnumerator TirePlayer(int minigameRecord)
		{
			activePlayer.plm.GetModifier().AddModifier("staminaRise", staminaMod);
			float totalTime = baseTirednessTime + (bonusTirednessEffect * minigameRecord);
			float cooldown = totalTime;
			gauge = Singleton<CoreGameManager>.Instance.GetHud(activePlayer.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, totalTime);
			while (cooldown > 0f)
			{
				if (activePlayer.plm.Entity.InternalMovement == Vector3.zero) // Only when player is NOT moving, rest
				{
					cooldown -= Time.deltaTime * ec.EnvironmentTimeScale;
				}
				else // If player is literally moving, tire it
				{
					cooldown += Time.deltaTime * ec.EnvironmentTimeScale * tirednessIncreaseSpeed * activePlayer.plm.Entity.InternalMovement.magnitude;
					if (cooldown > totalTime)
						cooldown = totalTime;
				}

				gauge.SetValue(totalTime, cooldown);
				yield return null;
			}
			gauge.Deactivate();
			activePlayer.plm.GetModifier().RemoveModifier(staminaMod);
		}

		IEnumerator MakePlayerRun()
		{
			var attr = activePlayer.GetAttribute();

			attr.AddAttribute(Storage.ATTR_FREEZE_STAMINA_UPDATE_TAG);
			attr.AddAttribute(Storage.ATTR_FREEZE_PLAYER_MOVEMENT_TAG);

			float modifiedStaminaDrop = activePlayer.plm.staminaDrop * (1 + (punishment_staminaFactorPerStreak * Streaks));

			while (activePlayer && activePlayer.plm.stamina > 0f)
			{
				float invertRun = activePlayer.plm.runSpeed * (activePlayer.reversed ? -1 : 1);
				activePlayer.plm.Entity.UpdateInternalMovement(activePlayer.transform.forward * invertRun * activePlayer.PlayerTimeScale);
				if (activePlayer.plm.Entity.Velocity != Vector3.zero) // Checks velocity to be sure player is actually running
				{
					activePlayer.plm.stamina = Mathf.Max(
							activePlayer.plm.stamina - modifiedStaminaDrop * Time.deltaTime * activePlayer.PlayerTimeScale,
							0f
						);
				}
				yield return null;
			}

			if (activePlayer)
			{
				attr.RemoveAttribute(Storage.ATTR_FREEZE_STAMINA_UPDATE_TAG);
				attr.RemoveAttribute(Storage.ATTR_FREEZE_PLAYER_MOVEMENT_TAG);
			}
		}

		IEnumerator Punched(Entity entity)
		{
			var mod = new MovementModifier(Vector3.zero, 0.35f / (1 + minigameRecord));
			entity.ExternalActivity.moveMods.Add(mod);
			float cool = punchCooldown;
			while (cool > 0f)
			{
				cool -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}
			entity.ExternalActivity.moveMods.Remove(mod);
			yield break;
		}


		[SerializeField]
		internal Sprite[] idleSprs, clapSprs, classSprs, disappointedSprs, crazySprs, chasingSprs;

		[SerializeField]
		internal Sprite[] idleSprsTalking, clapSprsTalking, classSprsTalking, disappointedSprsTalking, crazySprsTalking, chasingSprsTalking;
		[SerializeField]
		internal Sprite[] secretHappySprs, secretHappyTalkingSprs, secretAngrySwingingSprs, secretAngrySwingingTalkingSprs;

		[SerializeField]
		internal SoundObject[] audIdle, audClaps, audNotice, audPraise, audDisappointed, audAngry, audChaseAngry, audCaught, audStep, audAngryCaught, audPunchResponse;

		[SerializeField]
		internal SoundObject audCatch, audDismissed, audInstructions, audReady, audBounceBall, audThrow, audPunch, audTakeBack, audOkayThen, audHey;

		[SerializeField]
		internal PropagatedAudioManager audMan, bounceAudMan, clapAudMan;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal PickableBasketball basketPre;

		[SerializeField]
		internal float punchCooldown = 5f, baseTirednessTime = 5f, bonusTirednessEffect = 5f, minWaitTime = 35f, maxWaitTime = 50f, teleportpBackDelay = 1.25f,
		minigame_randomBasketballRange = 5f, minigame_baseBasketballSpeed = 20f, tirednessIncreaseSpeed = 0.05f;

		[SerializeField]
		internal int baseStaminaLoss = 50, staminaStreakBonus = 10;
		[SerializeField]
		internal float normSpeed = 14f, chaseSpeed = 21f, angryChaseSpeed = 22.5f;
		[SerializeField]
		[Range(0f, 1f)]
		internal float basketBallBounce_minSlowDownFactor = 0.85f, basketBallBounce_maxSlowDownFactor = 0.95f, secretChance = 0.04f,
			punishment_staminaFactorPerStreak = 0.65f;
		[SerializeField]
		internal ItemObject basketballItem;

		[SerializeField]
		internal RoomCategory expectedCategory;
		[SerializeField]
		internal Sprite gaugeSprite;
		PickableBasketball basketball;
		HudGauge gauge;
		bool _step = false;
		Sprite[] currentArrayInUsage;
		int idxInCurrentArray, clapIndex = 0;

		Vector3 selfPreviousSpot;
		readonly internal MovementModifier playerMoveMod = new(Vector3.zero, 0.85f), dribbleMoveMod = new(Vector3.zero, 0f);

		int succeededMinigames = 0;
		int minigameRecord = 0;
		PlayerManager activePlayer;
		readonly ValueModifier staminaMod = new(0f);

		public RoomController Home { get; private set; }
		public DribbleGymFunction GymFunction => Home.functionObject.GetComponent<DribbleGymFunction>();
		public int Streaks => minigameRecord;
		public float WaitTime => Random.Range(minWaitTime, maxWaitTime);
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

	internal class DribbleWanderStateBase(Dribble dr, float currentSpeedSet) : DribbleStateBase(dr)
	{
		readonly protected float currentSpeed = currentSpeedSet;
		public override void Enter()
		{
			base.Enter();
			dr.ApplyArray(dr.idleSprs, 0);
			dr.Navigator.maxSpeed = currentSpeed;
			dr.Navigator.SetSpeed(currentSpeed);
			dr.Entity.OnTeleport += Teleport;
		}
		public override void Exit()
		{
			base.Exit();
			dr.Entity.OnTeleport -= Teleport;
		}
		public override void Update()
		{
			base.Update();
			float mag = dr.Navigator.Velocity.magnitude;
			if (Time.timeScale > 0f && !skipStep && mag > 0.1f * Time.deltaTime)
			{
				stepDelay -= mag;
				if (stepDelay <= 0f)
				{
					stepDelay += 5f;
					step = !step;
					if (step)
						dr.Bounce(currentSpeed);
					dr.ApplyArray(dr.idleSprs, step ? 1 : 0);
				}
			}
			skipStep = false;
		}

		void Teleport(Vector3 pos) =>
			skipStep = true;

		float stepDelay = 5f;
		bool step = false;
		bool skipStep = false;
	}

	internal class Dribble_Idle(Dribble dr, float cooldown = 0f) : DribbleWanderStateBase(dr, dr.normSpeed)
	{
		float cooldown = cooldown;

		public override void Enter()
		{
			base.Enter();
			dr.Navigator.Am.moveMods.Remove(dr.dribbleMoveMod);
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

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (!player.Tagged && cooldown <= 0f)
				dr.behaviorStateMachine.ChangeState(new Dribble_NoticeChase(dr, player, false));
		}

		float sayCooldown = Random.Range(15f, 30f);
	}

	internal class Dribble_NoticeChase(Dribble dr, PlayerManager player, bool sightedEarlier) : DribbleWanderStateBase(dr, dr.chaseSpeed)
	{
		readonly PlayerManager player = player;
		NavigationState_TargetPlayer state;
		readonly bool sightedEarlier = sightedEarlier;
		public override void Enter()
		{
			base.Enter();

			dr.NoticeNoise(sightedEarlier);
			dr.Navigator.Am.moveMods.Remove(dr.dribbleMoveMod);

			state = new NavigationState_TargetPlayer(dr, 63, player.transform.position, true);
			ChangeNavigationState(state);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == this.player)
				state.UpdatePosition(player.transform.position);
		}

		public override void OnStateTriggerEnter(Collider other, bool validCollision)
		{
			base.OnStateTriggerEnter(other, validCollision);
			if (other.gameObject == player.gameObject)
			{
				if (player.Tagged || !validCollision)
				{
					dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, 5f));
					return;
				}
				dr.behaviorStateMachine.ChangeState(new Dribble_Inform(dr, player));
			}
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
			ChangeNavigationState(new NavigationState_DoNothing(dr, 0));
			dr.ComingNoise();
			dr.StartCoroutine(WaitForInform());
		}

		IEnumerator WaitForInform()
		{
			dr.ApplyArray(dr.clapSprs, 0);
			dr.Navigator.Am.moveMods.Add(dr.dribbleMoveMod);
			player.Am.moveMods.Add(dr.playerMoveMod);
			while (dr.audMan.QueuedAudioIsPlaying)
			{
				player.transform.RotateSmoothlyToNextPoint(dr.transform.position, 0.25f);
				if (!player ||
				player.plm.Entity.overridden)// If it is overridden, it isn't available
				{
					dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr));
					yield break;
				}

				if (!dr.looker.PlayerInSight(player)) // If player is not in sight, he needs to get him again
				{
					player.Am.moveMods.Remove(dr.playerMoveMod);
					dr.behaviorStateMachine.ChangeState(new Dribble_NoticeChase(dr, player, true));
					yield break;
				}
				yield return null;
			}
			player.Am.moveMods.Remove(dr.playerMoveMod);
			dr.KickEveryoneOutOfRoom();
			float cool = Random.Range(0.25f, 0.75f);
			while (cool > 0f)
			{
				player.transform.RotateSmoothlyToNextPoint(dr.transform.position, 0.8f);
				cool -= Time.deltaTime;
				yield return null;
			}
			dr.ApplyArray(dr.clapSprs, 1);
			dr.Clap();
			dr.TeleportToClass(player);

			for (int i = 0; i < 3; i++) // Frame delay
				yield return null;

			dr.ApplyArray(dr.clapSprs, 0);
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
				dr.MinigameEnd(true, player);
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
			dr.ApplyArray(dr.classSprs, 0);
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
			dr.ApplyArray(dr.classSprs, 1);

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
			dr.ApplyArray(dr.classSprs, 2);
			dr.ThrowBasketball(player);

			yield break;
		}
	}

	internal class Dribble_MinigameSucceed(Dribble dr, PlayerManager pm) : DribbleStateBase(dr)
	{
		readonly PlayerManager player = pm;
		float frame = 0f;
		bool clapped = false;
		readonly bool secretHappySprite = Random.value <= dr.secretChance;
		float cooldown = 5f;
		public override void Enter()
		{
			base.Enter();
			dr.UpdateMinigameCounter();
			dr.audMan.FlushQueue(true);
			dr.audMan.QueueRandomAudio(dr.audPraise);
		}
		public override void Update()
		{
			base.Update();
			if (!secretHappySprite)
			{
				frame += Time.deltaTime * dr.TimeScale * 11f;
				frame %= dr.clapSprs.Length;
				if (!clapped)
				{
					if (frame > 1f)
					{
						dr.Clap();
						clapped = true;
					}
				}
				else if (frame < 1f)
					clapped = false;

				dr.ApplyArray(dr.clapSprs, Mathf.FloorToInt(frame));
			}
			else
			{
				dr.ApplyArray(dr.secretHappySprs, 0); // single frame lol
			}

			cooldown -= Time.deltaTime * dr.TimeScale;
			if (cooldown < 0f)
			{
				dr.ApplyArray(dr.idleSprs, 0);
				dr.behaviorStateMachine.ChangeState(new Dribble_GetPlayerToTeleportBack(dr, player));
			}
		}
		public override void Exit()
		{
			base.Exit();
			dr.Navigator.Am.moveMods.Remove(dr.dribbleMoveMod);
			dr.UnlockEverything();
		}
	}

	internal class Dribble_Disappointed(Dribble dr, PlayerManager pm) : DribbleStateBase(dr)
	{
		readonly PlayerManager pm = pm;
		float frame = 0f;
		float cooldown = 2.5f;
		public override void Enter()
		{
			base.Enter();
			dr.audMan.FlushQueue(true);
			dr.Disappointed();
			dr.UpdateMinigameCounter();
		}

		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 6f;
			frame %= dr.disappointedSprs.Length;
			dr.ApplyArray(dr.disappointedSprs, Mathf.FloorToInt(frame));
			if (!dr.audMan.QueuedAudioIsPlaying)
			{
				cooldown -= Time.deltaTime * dr.TimeScale;
				if (cooldown < 0f)
				{
					dr.audMan.FlushQueue(true);
					dr.audMan.PlaySingle(dr.audDismissed);
					dr.ApplyArray(dr.idleSprs, 0);
					dr.behaviorStateMachine.ChangeState(new Dribble_GetPlayerToTeleportBack(dr, pm));
				}
			}

		}
	}

	internal class Dribble_AngrySwingingBase(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		readonly bool secretAngrySprite = Random.value <= dr.secretChance;
		Sprite[] activeSpriteSet = null;

		public override void Enter()
		{
			base.Enter();
			activeSpriteSet = secretAngrySprite ? dr.secretAngrySwingingSprs : dr.crazySprs;
		}
		public override void Update()
		{
			base.Update();
			if (activeSpriteSet == null) return;

			frame += Time.deltaTime * dr.TimeScale * 11f;
			frame %= activeSpriteSet.Length;

			dr.ApplyArray(activeSpriteSet, Mathf.FloorToInt(frame));
		}
	}

	internal class Dribble_MinigameFail(Dribble dr, PlayerManager pm) : Dribble_AngrySwingingBase(dr)
	{
		readonly PlayerManager pm = pm;
		public override void Enter()
		{
			base.Enter();
			dr.ec.MakeNoise(dr.transform.position, 36);
			dr.AngryNoise(dr.ec.CellFromPosition(pm.transform.position).room != dr.Home); // If player is outside, he screams smth else
			dr.UpdateMinigameCounter();
		}
		public override void Update()
		{
			base.Update();
			if (!dr.audMan.AnyAudioIsPlaying)
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
			dr.ApplyArray(dr.chasingSprs, 0);
			dr.Navigator.Am.moveMods.Remove(dr.dribbleMoveMod);
			dr.Navigator.maxSpeed = dr.angryChaseSpeed;
			dr.Navigator.SetSpeed(dr.angryChaseSpeed);
			state = new NavigationState_TargetPlayer(dr, 63, pm.transform.position);
			ChangeNavigationState(state);
			dr.Navigator.passableObstacles.Add(PassableObstacle.Bully);

			dr.Entity.OnTeleport += Teleport;
		}

		public override void Exit()
		{
			base.Exit();
			dr.Navigator.passableObstacles.Remove(PassableObstacle.Bully);
			dr.Entity.OnTeleport -= Teleport;
			ChangeNavigationState(new NavigationState_DoNothing(dr, 0));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();

			ChangeNavigationState(new NavigationState_WanderRandom(dr, 0));
		}

		public override void OnStateTriggerEnter(Collider other, bool validCollision)
		{
			base.OnStateTriggerEnter(other, validCollision);
			if (other.isTrigger && validCollision && other.CompareTag("NPC"))
			{
				var e = other.GetComponent<Entity>();
				if (e)
					dr.PunchNPC(e);
			}
		}

		public override void OnStateTriggerStay(Collider other, bool validCollision)
		{
			base.OnStateTriggerStay(other, validCollision);
			if (other.isTrigger && other.CompareTag("Player"))
			{
				var pm = other.GetComponent<PlayerManager>();
				if (pm && pm == this.pm)
					dr.behaviorStateMachine.ChangeState(new Dribble_ForceRun(dr, pm, !validCollision));
			}
		}

		public override void Hear(GameObject source, Vector3 position, int value)
		{
			base.Hear(source, position, value);
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
			if (Time.timeScale > 0f && !stopStep && mag > 0.1f * Time.deltaTime)
			{
				stepDelay -= mag;
				if (stepDelay <= 0f)
				{
					stepDelay += 5.6f;
					dr.Step();
					idx = 1 - idx;
					dr.ApplyArray(dr.chasingSprs, idx);
				}
			}
			stopStep = false;
		}

		void Teleport(Vector3 v) =>
			stopStep = true;

		bool stopStep = false;
	}

	internal class Dribble_ForceRun(Dribble dr, PlayerManager pm, bool failPickup) : Dribble_AngrySwingingBase(dr)
	{
		readonly private PlayerManager pm = pm;
		private readonly int minigameRecord = dr.Streaks;
		readonly bool failedPunishment = failPickup;
		float endCooldown = 5f;

		public override void Enter()
		{
			base.Enter();

			// Setup player
			pm.Teleport(dr.Home.RandomEntitySafeCellNoGarbage().CenterWorldPosition);

			if (pm.plm.stamina < pm.plm.staminaMax)
				pm.plm.AddStamina(pm.plm.staminaMax - pm.plm.stamina, true); // Fills up stamina to the current max it has

			// Setup Dribble for crazy
			if (!failedPunishment)
			{
				dr.Navigator.Am.moveMods.Add(dr.dribbleMoveMod);
				dr.PunishPlayerToRun();
			}

			dr.Entity.Teleport(dr.ec.RealRoomMid(dr.Home));
			dr.audMan.FlushQueue(true);
			dr.audMan.QueueRandomAudio(dr.audAngryCaught);
			dr.ec.MakeNoise(dr.transform.position, 39);

			dr.ResetMinigameRecord();
		}

		public override void Update()
		{
			base.Update();

			if (failedPunishment)
			{
				endCooldown -= Time.deltaTime * dr.TimeScale;
				if (endCooldown <= 0f)
					DribbleGoesIdle();
				return;
			}

			if (!pm || pm.plm.stamina <= 0f)
				DribbleGoesIdle();

			void DribbleGoesIdle()
			{
				dr.audMan.FlushQueue(true);
				dr.audMan.PlaySingle(dr.audDismissed);
				dr.ApplyArray(dr.idleSprs, 0);
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, dr.WaitTime));
			}
		}

		public override void Exit()
		{
			base.Exit();
			if (!failedPunishment)
				dr.FinishPunishment(minigameRecord);
		}
	}

	internal class Dribble_GetPlayerToTeleportBack(Dribble dr, PlayerManager pm) : DribbleWanderStateBase(dr, dr.chaseSpeed)
	{
		NavigationState_TargetPlayer state;
		readonly PlayerManager pm = pm;

		public override void Enter()
		{
			base.Enter();
			dr.PrepToLeaveNoise();
			state = new NavigationState_TargetPlayer(dr, 63, pm.transform.position);
			ChangeNavigationState(state);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(state);
			state.UpdatePosition(pm.transform.position);
		}

		public override void OnStateTriggerEnter(Collider other, bool validCollision)
		{
			base.OnStateTriggerEnter(other, validCollision);
			if (other.isTrigger)
			{
				if (other.CompareTag("Player"))
				{
					var pm = other.GetComponent<PlayerManager>();
					if (pm && pm == this.pm && validCollision)
						dr.behaviorStateMachine.ChangeState(new Dribble_PrepareToTeleportBack(dr, pm));
				}
			}
		}

		public override void Update()
		{
			base.Update();
			if (!dr.ec.CellFromPosition(pm.transform.position).TileMatches(dr.Home))
			{
				dr.OkThen();
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, dr.WaitTime));
			}
		}

		public override void Exit()
		{
			base.Exit();
			state.priority = 0;
		}
	}

	internal class Dribble_PrepareToTeleportBack(Dribble dr, PlayerManager pm) : DribbleStateBase(dr)
	{
		readonly private PlayerManager pm = pm;
		float cooldown = dr.teleportpBackDelay, clapDelay = 1.25f;
		bool clapped = false;

		public override void Enter()
		{
			base.Enter();
			dr.audMan.FlushQueue(true);
			dr.audMan.QueueAudio(dr.audReady);
			dr.ApplyArray(dr.clapSprs, 0);

			dr.Navigator.Am.moveMods.Add(dr.dribbleMoveMod);
			pm.Am.moveMods.Add(dr.playerMoveMod);
		}

		public override void Update()
		{
			base.Update();

			if (cooldown > 0f)
			{
				cooldown -= dr.TimeScale * Time.deltaTime;
			}
			else if (!clapped)
			{
				dr.ApplyArray(dr.clapSprs, 1);
				dr.Clap();
				clapped = true;

				dr.TeleportBack(pm);
			}

			if (clapped)
			{
				clapDelay -= Time.deltaTime * dr.TimeScale;
				if (clapDelay <= 0f)
				{
					dr.audMan.FlushQueue(true);
					dr.audMan.QueueAudio(dr.audDismissed);
					dr.ApplyArray(dr.idleSprs, 0);
					dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, dr.WaitTime));
				}
			}
		}

		public override void Exit()
		{
			base.Exit();
			dr.Navigator.Am.moveMods.Remove(dr.dribbleMoveMod);
			pm.Am.moveMods.Remove(dr.playerMoveMod);
		}
	}
}
