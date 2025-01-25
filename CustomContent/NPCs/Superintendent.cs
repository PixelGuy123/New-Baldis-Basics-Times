using BBTimes.Extensions;
using BBTimes.CustomComponents;
using PixelInternalAPI.Classes;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class Superintendent : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(46f, "Superintendent.png");
			audMan = GetComponent<AudioManager>();
			audOverHere = this.GetSound("Superintendent.wav", "Vfx_SI_BaldiHere", SoundType.Voice, new(0f, 0f, 0.796875f));
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();

			roomMap = new DijkstraMap(ec, PathType.Nav, transform);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new Superintendent_WanderAround(this));
		}
		public void StopOrNot(bool stop)
		{
			navigator.maxSpeed = stop ? 0f : speed;
			navigator.SetSpeed(stop ? 0f : speed);
		}
		public void CalloutBaldi(PlayerManager p)
		{
			audMan.PlaySingle(audOverHere);
			ec.MakeNoise(p.transform.position, noiseVal);
		}
		public RoomController GetCandidateRoom()
		{
			var currentRoom = ec.CellFromPosition(transform.position).room;

			_potentialRooms.Clear();
			_potentialRooms.AddRange(ec.rooms);

			for (int i = 0; i < _potentialRooms.Count; i++)
			{
				if (!allowedRooms.Contains(_potentialRooms[i].category) || _potentialRooms[i] == currentRoom)
					_potentialRooms.RemoveAt(i--);
			}

			if (_potentialRooms.Count == 0)
				Debug.LogWarning("SUPERINTENDENT DIDN\'T FOUND A SPOT TO PUT PLAYER ON, USING HALLWAY");
			else
				roomMap.Calculate();

			RoomController roomToChoose = ec.mainHall; // mainHall by default
			int distance = -1;

			for (int i = 0; i < _potentialRooms.Count; i++)
			{
				foreach (var door in _potentialRooms[i].doors)
				{
					int num = roomMap.Value(door.aTile.position);
					if (distance == -1 || num <= distance)
					{
						distance = num;
						roomToChoose = _potentialRooms[i];
					}
				}
			}

			return roomToChoose;
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


		readonly List<RoomController> _potentialRooms = [];
		DijkstraMap roomMap;
		readonly MovementModifier moveMod = new(Vector3.zero, 1f);

		[SerializeField]
		internal SoundObject audOverHere;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal float dragBreakDistance = 20f, dragMultiplier = 0.15f, dragSpeed = 25f, maxCooldownAfterDuty = 25f, lockTime = 5f, maxNoticeCooldown = 2.25f;

		const int noiseVal = 107;
		const float speed = 30f;

		public static void AddAllowedRoom(RoomCategory room) => allowedRooms.Add(room);

		internal static readonly HashSet<RoomCategory> allowedRooms = [RoomCategory.Class, RoomCategory.Office, RoomCategory.Special, RoomCategory.FieldTrip, RoomCategory.Mystery];
	}

	internal class Superintendent_Statebase(Superintendent s) : NpcState(s)
	{
		readonly protected Superintendent s = s;
	}

	internal class Superintendent_WanderAround(Superintendent s, bool hasCooldown = false) : Superintendent_Statebase(s)
	{
		float cooldown = hasCooldown ? s.maxCooldownAfterDuty : 0f;

		float noticeCooldown = s.maxNoticeCooldown;

		bool active = true;

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (active)
				noticeCooldown = s.maxNoticeCooldown;
			
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (!active)
				return;

			if (!player.Tagged && !Superintendent.allowedRooms.Contains(player.plm.Entity.CurrentRoom.category))
			{
				s.StopOrNot(true);
				noticeCooldown -= s.TimeScale * Time.deltaTime;
				if (noticeCooldown <= 0f)
				{
					s.behaviorStateMachine.ChangeState(new Superintendent_TargetPlayer(s, player));
					s.CalloutBaldi(player);
					active = false;
				}
				return;
			}
			if (noticeCooldown < s.maxNoticeCooldown)
			{
				noticeCooldown = s.maxNoticeCooldown;
			}

		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (active && noticeCooldown < s.maxNoticeCooldown)
			{
				noticeCooldown = s.maxNoticeCooldown;
				s.StopOrNot(false);
			}
		}


		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRounds(s, 0));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(s, 0));
		}

		public override void Update()
		{
			if (active) return;

			base.Update();

			if (cooldown > 0f)
				cooldown -= s.TimeScale * Time.deltaTime;
			else
			{
				active = true;
				noticeCooldown = s.maxNoticeCooldown;
			}
		}

		
	}

	internal class Superintendent_TargetPlayer(Superintendent s, PlayerManager pm) : Superintendent_Statebase(s)
	{
		readonly PlayerManager pm = pm;
		NavigationState_TargetPlayer tarPlayer;
		public override void Enter()
		{
			base.Enter();
			s.StopOrNot(false);
			tarPlayer = new(s, 63, pm.transform.position);
			ChangeNavigationState(tarPlayer);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == pm)
			{
				ChangeNavigationState(tarPlayer);
				tarPlayer.UpdatePosition(player.transform.position);
			}
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(s, 0));
		}

		public override void OnStateTriggerStay(Collider other)
		{
			base.OnStateTriggerStay(other);
			if (other.gameObject == pm.gameObject)
			{
				s.behaviorStateMachine.ChangeState(new Superintendent_DragPlayerToClass(s, pm));
			}
		}

		public override void Exit()
		{
			base.Exit();
			tarPlayer.priority = 0;
		}
	}

	internal class Superintendent_DragPlayerToClass(Superintendent s, PlayerManager pm) : Superintendent_Statebase(s)
	{
		readonly PlayerManager pm = pm;
		readonly RoomController room = s.GetCandidateRoom();
		NavigationState_TargetPosition tarPos;

		public override void Enter()
		{
			base.Enter();
			s.ReleaseOrNot(pm.plm.Entity, false);
			tarPos = new(s, 63, room.RandomEventSafeCellNoGarbage().FloorWorldPosition);
			ChangeNavigationState(tarPos);
		}

		public override void Update()
		{
			base.Update();
			if (!s.Drag(pm.plm.Entity))
			{
				s.behaviorStateMachine.ChangeState(new Superintendent_TargetPlayer(s, pm));
			}
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (s.ec.CellFromPosition(s.transform.position).TileMatches(room))
			{
				for (int i = 0; i < room.doors.Count; i++)
				{
					room.doors[i].Shut();
					room.doors[i].LockTimed(s.lockTime);
				}
				pm.Teleport(tarPos.destination);
				s.behaviorStateMachine.ChangeState(new Superintendent_WanderAround(s, true));
			}
			else
				ChangeNavigationState(tarPos);
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
			s.ReleaseOrNot(pm.plm.Entity, true);
		}
	}
}
