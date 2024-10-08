﻿using BBTimes.Extensions;
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
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			behaviorStateMachine.ChangeState(new Superintendent_WanderAround(this));
		}
		public void CalloutBaldi(PlayerManager p)
		{
			audMan.PlaySingle(audOverHere);
			ec.MakeNoise(p.transform.position, noiseVal);
		}
		public void InvertDir()
		{
			Cell cell = ec.CellFromPosition(transform.position - (transform.forward * LayerStorage.TileBaseOffset));
			if (!cell.Null)
				navigator.FindPath(cell.FloorWorldPosition); // REALLY ANNOYING workaround for wander round npcs to go backwards

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}


		[SerializeField]
		internal SoundObject audOverHere;

		[SerializeField]
		internal AudioManager audMan;

		const int noiseVal = 107;
		const float speed = 30f;

		public static void AddAllowedRoom(RoomCategory room) => allowedRooms.Add(room);

		internal static readonly HashSet<RoomCategory> allowedRooms = [RoomCategory.Class, RoomCategory.Office, RoomCategory.Special, RoomCategory.FieldTrip, RoomCategory.Mystery];
	}

	internal class Superintendent_WanderAround(Superintendent s) : NpcState(s)
	{
		readonly Superintendent superintendent = s;

		const float noticeMaxCooldown = 2.5f;

		float cooldown = 0f;

		float noticeCooldown = noticeMaxCooldown;

		bool active = true;

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (active)
				noticeCooldown = noticeMaxCooldown;
			
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (!active)
				return;

			if (!player.Tagged && !Superintendent.allowedRooms.Contains(player.plm.Entity.CurrentRoom.category))
			{
				superintendent.Navigator.maxSpeed = 0f;
				noticeCooldown -= superintendent.TimeScale * Time.deltaTime;
				if (noticeCooldown <= 0f)
				{
					superintendent.CalloutBaldi(player);
					superintendent.InvertDir();
					cooldown = 25f;
					active = false;
				}
				return;
			}
			if (noticeCooldown < noticeMaxCooldown)
			{
				noticeCooldown = noticeMaxCooldown;
				superintendent.InvertDir();
			}

		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (active && noticeCooldown < noticeMaxCooldown)
			{
				noticeCooldown = noticeMaxCooldown;
				superintendent.InvertDir();
			}
		}


		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRounds(superintendent, 0));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(new NavigationState_WanderRounds(superintendent, 0));
		}

		public override void Update()
		{
			if (active) return;

			base.Update();

			if (cooldown > 0f)
				cooldown -= superintendent.TimeScale * Time.deltaTime;

			else
			{
				active = true;
				noticeCooldown = noticeMaxCooldown;
			}
		}

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
}
