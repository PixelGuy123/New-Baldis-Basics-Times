using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Superintendent : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			strikeSprites = this.GetSpriteSheet(4, 1, 46f, "Superintendent.png");
			spriteRenderer[0].sprite = strikeSprites[0];

			audMan = GetComponent<AudioManager>();
			audBaldiOverHere = this.GetSound("Superintendent.wav", "Vfx_SI_BaldiHere_1", SoundType.Voice, new(0f, 0f, 0.796875f));
			audBaldiOverHere.additionalKeys = [
				new() { key = "Vfx_SI_BaldiHere_2", time = 0.449f },
				new() { key = "Vfx_SI_BaldiHere_3", time = 1.084f },
				new() { key = "Vfx_SI_BaldiHere_4", time = 1.515f }
				];
			audHere = this.GetSound("Superintendent_Hey.wav", "Vfx_SI_Hey_1", SoundType.Voice, new(0f, 0f, 0.796875f));
			audHere.additionalKeys = [
				new() { key = "Vfx_SI_Hey_2", time = 0.431f },
				];

			renderer = spriteRenderer[0];
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

			roomMap = new DijkstraMap(ec, PathType.Nav, int.MaxValue, transform);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new Superintendent_WanderAround(this));
		}
		public void StopOrNot(bool stop)
		{
			navigator.maxSpeed = stop ? 0f : speed;
			navigator.SetSpeed(stop ? 0f : speed);
		}
		public void Angry()
		{
			navigator.maxSpeed = angrySpeed;
			navigator.SetSpeed(angrySpeed);
		}
		public void Callout(bool actualCallout, Vector3 position)
		{
			audMan.PlaySingle(actualCallout ? audBaldiOverHere : audHere);
			if (actualCallout)
				ec.MakeNoise(position, noiseVal);
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
			if (entity.InteractionDisabled || !entity.InBounds || num > dragBreakDistance)
				return false;

			entity.Teleport(transform.position);
			return true;
		}

		public bool TryOverrideEntity(Entity entity)
		{
			if (entity.Override(overrider))
			{
				overrider.SetInteractionState(false);
				overrider.SetVisible(false);
				overrider.SetBlinded(true);
				overrider.SetHeight(5.75f); // Being grabbed lol
				return true;
			}

			return false;
		}

		public bool GetAStrikeAndTellIfItIsAngry()
		{
			bool angry = false;
			if (++strikeVal >= strikeSprites.Length - 1)
			{
				strikeVal = strikeSprites.Length - 1;
				angry = true;
			}

			renderer.sprite = strikeSprites[strikeVal];

			return angry;
		}

		public void FlushStrikeVal()
		{
			strikeVal = 0;
			renderer.sprite = strikeSprites[strikeVal];
		}

		public void Release() =>
			overrider.Release();

		readonly EntityOverrider overrider = new();


		readonly List<RoomController> _potentialRooms = [];
		DijkstraMap roomMap;
		readonly HashSet<NPC> sawNpcs = [];
		public void AddNpc(NPC npc) => sawNpcs.Add(npc);
		public bool HasNpc(NPC npc) => sawNpcs.Contains(npc);
		public int NpcsSighted => sawNpcs.Count;
		public void ClearNpcs() => sawNpcs.Clear();
		public void RemoveNpc(NPC npc) => sawNpcs.Remove(npc);

		[SerializeField]
		internal SoundObject audBaldiOverHere, audHere;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] strikeSprites;

		[SerializeField]
		internal float dragBreakDistance = 20f, dragMultiplier = 0.15f, dragSpeed = 25f, maxCooldownAfterDuty = 15f, lockTime = 5f, maxNoticeCooldown = 2.25f, speed = 30f, angrySpeed = 55f;

		int strikeVal = 0;
		const int noiseVal = 107;

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

		bool Active => cooldown <= 0f;
		bool NpcDetected => s.NpcsSighted != 0;

		bool playerDetected = false, stopOrNot = false;

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (Active)
				noticeCooldown = s.maxNoticeCooldown;

		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (!Active)
				return;

			if (!player.Tagged && !Superintendent.allowedRooms.Contains(player.plm.Entity.CurrentRoom.category))
			{
				playerDetected = true;
				noticeCooldown -= s.TimeScale * Time.deltaTime;
				if (noticeCooldown <= 0f)
				{
					cooldown = s.maxCooldownAfterDuty;
					noticeCooldown = s.maxNoticeCooldown;
					bool angry = s.GetAStrikeAndTellIfItIsAngry();
					s.Callout(angry, player.transform.position);


					if (angry)
					{
						s.StopOrNot(false);
						s.behaviorStateMachine.ChangeState(new Superintendent_TargetPlayer(s, player));
					}
				}
				return;
			}
			playerDetected = false;

		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (Active && noticeCooldown < s.maxNoticeCooldown)
			{
				noticeCooldown = s.maxNoticeCooldown;
			}

			playerDetected = false;
		}


		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRounds(s, 0));

			if (hasCooldown)
				s.FlushStrikeVal();
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(s, 0));
		}

		public override void Update()
		{
			base.Update();

			bool canStop = Active && (playerDetected || NpcDetected);
			if (canStop != stopOrNot)
			{
				stopOrNot = canStop;
				s.StopOrNot(canStop);
			}

			if (!Active)
			{
				if (cooldown > 0f)
					cooldown -= s.TimeScale * Time.deltaTime;
				return;
			}


			if (!s.Blinded)
			{
				foreach (NPC npc in s.ec.Npcs)
				{
					if (npc != s && npc.Navigator.isActiveAndEnabled)
					{
						var meta = npc.GetMeta();
						if ((meta == null || meta.tags.Contains("student")) && npc.Navigator.Entity.CurrentRoom && !Superintendent.allowedRooms.Contains(npc.Navigator.Entity.CurrentRoom.category) && s.looker.RaycastNPC(npc))
						{
							s.AddNpc(npc);

							noticeCooldown -= s.TimeScale * Time.deltaTime;
							if (noticeCooldown <= 0f)
							{
								cooldown = s.maxCooldownAfterDuty;
								noticeCooldown = s.maxNoticeCooldown;
								bool angry = s.GetAStrikeAndTellIfItIsAngry();
								s.Callout(angry, npc.transform.position);
								s.ClearNpcs();

								if (angry)
								{
									s.StopOrNot(false);
									s.behaviorStateMachine.ChangeState(new Superintendent_TargetNPC(s, npc));
								}
							}
							break;

						}
						else if (s.HasNpc(npc))
						{
							s.RemoveNpc(npc);
							noticeCooldown = s.maxNoticeCooldown;
						}
					}
				}
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
			s.Angry();
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
			if (other.gameObject == pm.gameObject && s.TryOverrideEntity(pm.plm.Entity))
				s.behaviorStateMachine.ChangeState(new Superintendent_DragEntityToClass(s, pm.plm.Entity, pm));

		}

		public override void Exit()
		{
			base.Exit();
			tarPlayer.priority = 0;
		}
	}

	internal class Superintendent_TargetNPC(Superintendent s, NPC target) : Superintendent_Statebase(s)
	{
		readonly NPC target = target;
		NavigationState_TargetPlayer tarNpc;
		public override void Enter()
		{
			base.Enter();
			s.Angry();
			tarNpc = new(s, 63, target.transform.position);
			ChangeNavigationState(tarNpc);
		}

		public override void Update()
		{
			base.Update();
			if (target)
				tarNpc.UpdatePosition(target.transform.position);
			else
				s.behaviorStateMachine.ChangeState(new Superintendent_WanderAround(s));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == target)
			{
				ChangeNavigationState(tarNpc);
				tarNpc.UpdatePosition(player.transform.position);
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
			if (other.gameObject == target.gameObject && s.TryOverrideEntity(target.Navigator.Entity))
				s.behaviorStateMachine.ChangeState(new Superintendent_DragEntityToClass(s, target.Navigator.Entity, npc: target));

		}

		public override void Exit()
		{
			base.Exit();
			tarNpc.priority = 0;
		}
	}

	internal class Superintendent_DragEntityToClass(Superintendent s, Entity e, PlayerManager pm = null, NPC npc = null) : Superintendent_Statebase(s)
	{
		readonly Entity e = e;
		readonly PlayerManager pm = pm;
		readonly NPC tarNpc = npc;

		readonly RoomController room = s.GetCandidateRoom();
		NavigationState_TargetPosition tarPos;

		public override void Enter()
		{
			base.Enter();
			s.Angry();
			tarPos = new(s, 63, room.RandomEventSafeCellNoGarbage().FloorWorldPosition);
			ChangeNavigationState(tarPos);
		}

		public override void Update()
		{
			base.Update();
			if (!s.Drag(e))
			{
				if (pm)
					s.behaviorStateMachine.ChangeState(new Superintendent_TargetPlayer(s, pm));
				else
					s.behaviorStateMachine.ChangeState(new Superintendent_TargetNPC(s, tarNpc));
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
				e.Teleport(tarPos.destination);
				s.behaviorStateMachine.ChangeState(new Superintendent_WanderAround(s, true));
			}
			else
				ChangeNavigationState(tarPos);
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
			s.Release();
			s.StopOrNot(false);
		}
	}
}
