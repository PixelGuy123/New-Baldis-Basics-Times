using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class TickTock : NPC, INPCPrefab, IClickable<int>
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();
			anim = gameObject.AddComponent<AnimationComponent>();
			anim.renderers = [spriteRenderer[0]];
			anim.speed = 7;

			var sprites = this.GetSpriteSheet(6, 1, 28f, "ticktocksprite.png");
			spriteRenderer[0].sprite = sprites[0];
			sprSleeping = [sprites[0]];
			sprDinging = [sprites[1], sprites[2]];
			sprIdle = [sprites[3]];
			sprRinging = [sprites[4], sprites[5]];

			audActivate = this.GetSound("tic_wanderStart.ogg", "Vfx_TickTock_WanderStart", SoundType.Voice, Color.blue);
			audDing = this.GetSound("tic_ding.ogg", "Vfx_TickTock_Ding", SoundType.Voice, Color.blue);
			audGoSleep = this.GetSound("tic_finishRing.ogg", "Vfx_TickTock_GoSleep", SoundType.Voice, Color.blue);
			audRing = this.GetSound("tic_ring.ogg", "Vfx_TickTock_Ring", SoundType.Voice, Color.blue);
			audSleeping = this.GetSound("tic_sleepLoop.ogg", "Vfx_TickTock_Sleeping", SoundType.Voice, Color.blue);

			anim.animation = sprSleeping;

			var myCol = (CapsuleCollider)baseTrigger[0];
			var col = this.CreateClickableLink().gameObject.AddComponent<CapsuleCollider>();
			col.isTrigger = true;
			col.height = myCol.height;
			col.direction = myCol.direction;
			col.radius = myCol.radius;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSleeping, audGoSleep, audActivate, audRing, audDing;

		[SerializeField]
		internal Sprite[] sprSleeping, sprDinging, sprIdle, sprRinging;

		[SerializeField]
		internal AnimationComponent anim;

		public void Sleep()
		{
			anim.animation = sprSleeping;

			audMan.FlushQueue(true);
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audSleeping);

			navigator.maxSpeed = 0;
			navigator.SetSpeed(0);
			sleeping = true;
		}
		public void WannaSleep() => audMan.QueueAudio(audGoSleep);
		public void DingPhase()
		{
			sleeping = false;
			audMan.FlushQueue(true);
			anim.animation = sprDinging;
		}

		public void Ding() => audMan.PlaySingle(audDing);
		public void IdleGo() =>
			anim.animation = sprIdle;
		
		public void Ring()
		{
			anim.animation = sprRinging;
			audMan.FlushQueue(true);
			audMan.QueueAudio(audRing);
			ec.MakeNoise(transform.position, 99);
		}

		public override void Initialize()
		{
			base.Initialize();
			anim.Initialize(ec);
			behaviorStateMachine.ChangeState(new TickTock_Sleeping(this));
		}

		public void Clicked(int player)
		{
			if (!sleeping) return;
			behaviorStateMachine.ChangeState(new TickTock_Ding(this));
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => !sleeping;
		public bool ClickableRequiresNormalHeight() => false;

		public Cell GetRoomToGo()
		{
			audMan.PlaySingle(audActivate);
			var excludingRoom = ec.CellFromPosition(transform.position).room;
			var rooms = new List<RoomController>(ec.rooms);
			if (rooms.Count > 1)
				rooms.Remove(excludingRoom);
			if (rooms.Count == 0)
				goto justInCase;
			
			List<Cell> spots = [];
			while (spots.Count == 0)
			{
				if (rooms.Count == 0)
					goto justInCase;

				int i = Random.Range(0, rooms.Count);
				spots = rooms[i].AllEntitySafeCellsNoGarbage();
				rooms.RemoveAt(i);
			}

			return spots[Random.Range(0, spots.Count)];

			justInCase:
			var noGarb = ec.mainHall.AllTilesNoGarbage(false, false);
			return noGarb[Random.Range(0, noGarb.Count)]; // Just in case
		}

		bool sleeping = false;
    }

	internal class TickTock_StateBase(TickTock tic) : NpcState(tic)
	{
		protected TickTock tic = tic;
	}

	internal class TickTock_Sleeping(TickTock tic) : TickTock_StateBase(tic)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(tic, 0));
			tic.Sleep();
		}
	}

	internal class TickTock_Ding(TickTock tic) : TickTock_StateBase(tic)
	{
		float dingCooldown = 0f;
		int dings = 0;
		readonly int maxDings = Random.Range(3, 5);
		public override void Enter()
		{
			base.Enter();
			tic.DingPhase();
		}

		public override void Update()
		{
			base.Update();
			dingCooldown -= tic.TimeScale * Time.deltaTime;
			if (dingCooldown <= 0f)
			{
				dingCooldown += 0.9f;
				if (++dings > maxDings)
				{
					tic.behaviorStateMachine.ChangeState(new TickTock_GoToARoom(tic));
					return;
				}
				tic.Ding();
			}
		}
	}

	internal class TickTock_GoToARoom(TickTock tic) : TickTock_StateBase(tic)
	{
		NavigationState_TargetPosition pos;
		Cell target;
		public override void Enter() 
		{
			tic.Navigator.maxSpeed = 22f;
			tic.Navigator.SetSpeed(22f);
			target = tic.GetRoomToGo();
			tic.IdleGo();
			pos = new(tic, 64, target.FloorWorldPosition);
			ChangeNavigationState(pos);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (tic.ec.CellFromPosition(tic.transform.position) != target)
				ChangeNavigationState(pos);
			else
			{
				pos.priority = 0;
				tic.behaviorStateMachine.ChangeState(new TickTock_WaitForRing(tic));
			}
		}
	}

	internal class TickTock_WaitForRing(TickTock tic) : TickTock_StateBase(tic)
	{
		float tickTockCooldown = 5f;
		public override void Enter()
		{
			base.Enter();
			tic.Navigator.maxSpeed = 0;
			tic.Navigator.SetSpeed(0);
			ChangeNavigationState(new NavigationState_DoNothing(tic, 0));
		}

		public override void Update()
		{
			base.Update();
			tickTockCooldown -= tic.TimeScale * Time.deltaTime;
			if (tickTockCooldown <= 0f)
				tic.behaviorStateMachine.ChangeState(new TickTock_Ring(tic));
		}
	}

	internal class TickTock_Ring(TickTock tic) : TickTock_StateBase(tic)
	{
		public override void Enter()
		{
			base.Enter();
			tic.Ring();
		}

		public override void Update()
		{
			base.Update();
			if (!tic.audMan.QueuedAudioIsPlaying)
				tic.behaviorStateMachine.ChangeState(new TickTock_AboutToSleep(tic));
		}
	}

	internal class TickTock_AboutToSleep(TickTock tic) : TickTock_StateBase(tic)
	{
		public override void Enter()
		{
			base.Enter();
			tic.IdleGo();
			tic.WannaSleep();
		}

		public override void Update()
		{
			base.Update();
			if (!tic.audMan.QueuedAudioIsPlaying)
				tic.behaviorStateMachine.ChangeState(new TickTock_Sleeping(tic));
		}
	}
}
