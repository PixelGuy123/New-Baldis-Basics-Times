using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class DetentionBot : NPC, INPCPrefab, IItemAcceptor
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			renderer = spriteRenderer[0];
			var sprites = this.GetSpriteSheet(2, 1, 38f, "DetentionBot.png");
			sprIdle = sprites[0];
			sprCarrying = sprites[1];
			renderer.sprite = sprIdle;

			audWhistle = this.GetSound("DetentionBotIdle.wav", "Vfx_DetentionBot_Whistle", SoundType.Voice, new(0.65f, 0.7f, 0.65f));
			audFindTroubleMaker = this.GetSound("bot_spotted.wav", "Vfx_DetentionBot_FoundTroubleMaker", SoundType.Voice, new(0.65f, 0.7f, 0.65f));
			audCarryTroubleMaker = this.GetSound("bot_carrying.wav", "Vfx_DetentionBot_CarryTroubleMaker", SoundType.Voice, new(0.65f, 0.7f, 0.65f));

			audTroubleMakerEscaping = this.GetSound("bot_lookingFor.wav", "Vfx_DetentionBot_LoseTroubleMaker_1", SoundType.Voice, new(0.65f, 0.7f, 0.65f));
			audTroubleMakerEscaping.additionalKeys = [new() { key = "Vfx_DetentionBot_LoseTroubleMaker_2", time = 3.084f }];

			audLectureTroubleMaker = this.GetSound("bot_notdare.wav", "Vfx_DetentionBot_LectureTroubleMaker_1", SoundType.Voice, new(0.65f, 0.7f, 0.65f));
			audLectureTroubleMaker.additionalKeys = [new() { key = "Vfx_DetentionBot_LectureTroubleMaker_2", time = 1.148f }];
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			for (int i = 0; i < ec.rooms.Count; i++)
				if (ec.rooms[i].category == RoomCategory.Office)
					offices.Add(ec.rooms[i]);

			timeInSight = new float[Singleton<CoreGameManager>.Instance.TotalPlayers];
			behaviorStateMachine.ChangeState(new DetentionBot_Wandering(this));
		}

		public void ObservePlayer(PlayerManager player)
		{
			if (player.Disobeying && !player.Tagged)
			{
				timeInSight[player.playerNumber] += Time.deltaTime * TimeScale;
				if (timeInSight[player.playerNumber] >= player.GuiltySensitivity)
				{
					// Go after player here
					TroubleMakerDetected();
					behaviorStateMachine.ChangeState(new DetentionBot_GoAfterPlayer(this, player));
				}
			}
			else
				LoseTrackOfPlayer(player);
		}

		public void LoseTrackOfPlayer(PlayerManager player) =>
			timeInSight[player.playerNumber] = 0f;

		public void WhistleChance()
		{
			if (Random.value < whistleChance && !audMan.QueuedAudioIsPlaying)
				audMan.PlaySingle(audWhistle);
		}

		public void MoveOrNot(bool move)
		{
			navigator.maxSpeed = move ? 23.5f : 0f;
			navigator.SetSpeed(navigator.maxSpeed);
		}
		public void Wander() =>
			renderer.sprite = sprIdle;
		public void AlarmTroubleMakerEscaping()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(audTroubleMakerEscaping);
		}
		public bool Drag(Entity entity)
		{
			float num = Mathf.Abs((transform.position - entity.transform.position).magnitude);
			if (num > dragBreakDistance)
			{
				moveMod.movementAddend = Vector3.zero;
				moveMod.movementMultiplier = 1f;
				return false;
			}
			moveMod.movementAddend = (transform.position - entity.transform.position).normalized * (dragSpeed + num) * TimeScale;
			moveMod.movementMultiplier = dragMultiplier;
			return true;
		}
		public void ReleaseOrNot(Entity entity, bool release)
		{
			if (release)
				entity.ExternalActivity.moveMods.Remove(moveMod);
			else
				entity.ExternalActivity.moveMods.Add(moveMod);
		}
		public void Lecture()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(audLectureTroubleMaker);
		}
		public void TroubleMakerDetected()
		{
			audMan.FlushQueue(true);
			audMan.QueueAudio(audFindTroubleMaker);
			IsAngryAtSomeone = true;
		}
		public void CarryingTroubleMaker()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(audCarryTroubleMaker);
			renderer.sprite = sprCarrying;
		}
		public bool ItemFits(Items itm) =>
			IsAngryAtSomeone && behaviorStateMachine.CurrentState is DetentionBot_GoAfterPlayer && disablingItems.Contains(itm);

		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			potentialNPCs.Clear();
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i].isActiveAndEnabled && ec.Npcs[i] != this)
					potentialNPCs.Add(ec.Npcs[i]);
			}
			if (potentialNPCs.Count != 0)
				behaviorStateMachine.ChangeState(new DetentionBot_GoAfterNPC(this, potentialNPCs[Random.Range(0, potentialNPCs.Count)]));
			else
				behaviorStateMachine.ChangeState(new DetentionBot_Wandering(this));
		}



		[SerializeField]
		internal Sprite sprIdle, sprCarrying;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audFindTroubleMaker, audCarryTroubleMaker, audTroubleMakerEscaping, audLectureTroubleMaker, audWhistle;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal float whistleChance = 0.08f, dragBreakDistance = 20f, dragMultiplier = 0.15f, dragSpeed = 14f;

		readonly MovementModifier moveMod = new(Vector3.zero, 1f);

		private float[] timeInSight;

		readonly List<RoomController> offices = [];
		readonly List<NPC> potentialNPCs = [];
		public RoomController PickRandomOffice => offices[Random.Range(0, offices.Count)];

		readonly static HashSet<Items> disablingItems = [Items.Scissors];
		public static void AddDisablingItem(Items item) => disablingItems.Add(item);
		public bool IsAngryAtSomeone { get; internal set; } = false;

	}


	internal class DetentionBot_StateBase(DetentionBot bot) : NpcState(bot)
	{
		protected DetentionBot bot = bot;
		public override void DoorHit(StandardDoor door)
		{
			base.DoorHit(door);
			door.OpenTimedWithKey(door.DefaultTime, false);
		}

		public override void OnRoomExit(RoomController room)
		{
			base.OnRoomExit(room);
			if (room.ec.timeOut && room.type == RoomType.Room && !room.HasIncompleteActivity)
			{
				room.SetPower(false);
			}
		}
	}

	internal class DetentionBot_Wandering(DetentionBot bot) : DetentionBot_StateBase(bot)
	{
		float whistleTime = 1f;
		public override void Enter()
		{
			base.Enter();
			bot.MoveOrNot(true);
			bot.Wander();
			bot.IsAngryAtSomeone = false;
			ChangeNavigationState(new NavigationState_WanderRounds(bot, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			bot.ObservePlayer(player);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			bot.LoseTrackOfPlayer(player);
		}

		public override void Update()
		{
			base.Update();
			if (!bot.Blinded)
			{
				foreach (NPC npc in bot.ec.Npcs)
				{
					if (npc != bot && npc.Disobeying && bot.looker.RaycastNPC(npc))
					{
						bot.TroubleMakerDetected();
						bot.behaviorStateMachine.ChangeState(new DetentionBot_GoAfterNPC(bot, npc));
						break;
					}
				}
			}
			whistleTime -= Time.deltaTime * bot.TimeScale;
			if (whistleTime <= 0f)
			{
				bot.WhistleChance();
				whistleTime = 1f;
			}
		}
	}

	internal class DetentionBot_GoAfterPlayer(DetentionBot bot, PlayerManager target) : DetentionBot_StateBase(bot)
	{
		readonly PlayerManager pm = target;

		NavigationState_TargetPlayer navPm;

		public override void Enter()
		{
			base.Enter();
			navPm = new NavigationState_TargetPlayer(bot, 64, pm.transform.position);
			bot.LoseTrackOfPlayer(pm);
			bot.Wander();
			ChangeNavigationState(navPm);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (pm == player)
			{
				ChangeNavigationState(navPm);
				navPm.UpdatePosition(player.transform.position);
			}
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(bot, 0));
		}
		public override void Update()
		{
			base.Update();
			if (!pm)
				bot.behaviorStateMachine.ChangeState(new DetentionBot_Wandering(bot));
		}

		public override void OnStateTriggerStay(Collider other)
		{
			base.OnStateTriggerStay(other);
			if (other.gameObject == pm.gameObject)
				bot.behaviorStateMachine.ChangeState(new DetentionBot_CarryEntity(bot, pm.plm.Entity, pm));
		}

		public override void Exit()
		{
			base.Exit();
			navPm.priority = 0;
		}
	}

	internal class DetentionBot_GoAfterNPC(DetentionBot bot, NPC target) : DetentionBot_StateBase(bot)
	{
		readonly NPC troubleMaker = target;

		NavigationState_TargetPosition navPm;

		public override void Enter()
		{
			base.Enter();
			navPm = new NavigationState_TargetPosition(bot, 64, troubleMaker.transform.position);
			bot.Wander();
			ChangeNavigationState(navPm);
		}
		public override void Update()
		{
			base.Update();
			if (!troubleMaker)
			{
				Debug.LogWarning("DETENTION BOT IGNORED DESTROYED TROUBLE MAKER");
				bot.behaviorStateMachine.ChangeState(new DetentionBot_Wandering(bot));
			}
			else
				navPm.UpdatePosition(troubleMaker.transform.position);
		}

		public override void OnStateTriggerStay(Collider other)
		{
			base.OnStateTriggerStay(other);
			if (other.gameObject == troubleMaker.gameObject)
				bot.behaviorStateMachine.ChangeState(new DetentionBot_CarryEntity(bot, troubleMaker.Navigator.Entity, npc: troubleMaker));
		}

		public override void Exit()
		{
			base.Exit();
			navPm.priority = 0;
		}
	}

	internal class DetentionBot_CarryEntity(DetentionBot bot, Entity entity, PlayerManager pm = null, NPC npc = null) : DetentionBot_StateBase(bot)
	{
		NavigationState_TargetPosition tarPos;
		readonly RoomController office = bot.PickRandomOffice;
		readonly Entity entity = entity;
		readonly PlayerManager pm = pm;
		readonly NPC tarNpc = npc;

		public override void Enter()
		{
			base.Enter();
			bot.CarryingTroubleMaker();
			bot.ReleaseOrNot(entity, false);
			tarPos = new NavigationState_TargetPosition(bot, 63, office.RandomEntitySafeCellNoGarbage().FloorWorldPosition);
			ChangeNavigationState(tarPos);

			if (tarNpc && !tarNpc.Navigator.isActiveAndEnabled) // If not enabled, it implies Detention Bot will be permanently stuck, so it'll forcefully teleport the entity
			{
				bot.Navigator.Entity.Teleport(tarPos.destination);
				entity.Teleport(tarPos.destination);
			}
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (!bot.ec.CellFromPosition(bot.transform.position).TileMatches(office))
				ChangeNavigationState(tarPos);
			else
			{
				entity.Teleport(bot.transform.position);
				tarNpc?.SentToDetention();
				bot.behaviorStateMachine.ChangeState(new DetentionBot_Lecture(bot));
			}

		}
		public override void Update()
		{
			base.Update();
			if (!entity)
			{
				bot.behaviorStateMachine.ChangeState(new DetentionBot_Wandering(bot));
				return;
			}


			if (!bot.Drag(entity))
			{
				bot.AlarmTroubleMakerEscaping();
				bot.behaviorStateMachine.ChangeState(pm ? new DetentionBot_GoAfterPlayer(bot, pm) : new DetentionBot_GoAfterNPC(bot, tarNpc));
			}
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
			if (entity)
				bot.ReleaseOrNot(entity, true);
		}
	}

	internal class DetentionBot_Lecture(DetentionBot bot) : DetentionBot_StateBase(bot)
	{
		float delay = 3f;
		public override void Enter()
		{
			base.Enter();
			bot.Lecture();
			ChangeNavigationState(new NavigationState_TargetPosition(bot, 0, bot.ec.CellFromPosition(bot.transform.position).room.RandomEntitySafeCellNoGarbage().FloorWorldPosition));
			bot.MoveOrNot(true);
			bot.Wander();
			bot.IsAngryAtSomeone = false;
		}

		public override void Update()
		{
			base.Update();
			if (delay <= 0f)
			{
				if (!bot.audMan.QueuedAudioIsPlaying)
					bot.behaviorStateMachine.ChangeState(new DetentionBot_Wandering(bot));
			}
			else
				delay -= bot.TimeScale * Time.deltaTime;
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_DoNothing(bot, 0));
			bot.MoveOrNot(false);
		}
	}
}
