using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class Leapy : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			
			audMan = GetComponent<PropagatedAudioManager>();
			audJump = this.GetSound("leapy_jump.wav", "Vfx_Leapy_Leap", SoundType.Effect, new Color(0f, 0.3984f, 0f));
			audStomp = this.GetSound("leapy_stomp.wav", "Vfx_Leapy_Stomp", SoundType.Effect, new Color(0f, 0.3984f, 0f));

			renderer = spriteRenderer[0];
			var sprs = this.GetSpriteSheet(3, 1, 25f, "leapy.png");
			spriteRenderer[0].sprite = sprs[0];
			sprIdle = sprs[0];
			sprPrepare = sprs[1];
			sprJump = sprs[2];
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string Category => "npcs";
		
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		
		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new Leapy_Idle(this));
			behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0)); // Note: navigation states should be changed after setting a behavior state
		}

		public override void Despawn()
		{
			base.Despawn();
			for (int i = 0; i < affectedEntities.Count; i++)
			{
				if (affectedEntities[i])
				{
					affectedEntities[i].Unsquish();
					affectedEntities[i].ExternalActivity.moveMods.Remove(affectedMoveMod);
				}
			}
		}

		internal void Idle() =>
			renderer.sprite = sprIdle;

		internal void PrepareSprite() =>
			renderer.sprite = sprPrepare;

		internal void Jump()
		{
			currentJumpDistance = Random.Range(minJumpDistance, maxJumpDistance + 1);
			renderer.sprite = sprJump;
			audMan.PlaySingle(audJump);
		}

		internal void StopMe(bool stop)
		{
			if (stop)
				Navigator.Entity.ExternalActivity.moveMods.Add(myMoveMod);
			else
				Navigator.Entity.ExternalActivity.moveMods.Remove(myMoveMod);
		}

		internal void Stomp(Entity e)
		{
			if (affectedEntities.Contains(e)) return;

			audMan.PlaySingle(audStomp);
			affectedEntities.Add(e);
			e.ExternalActivity.moveMods.Add(affectedMoveMod);
			e.Squish(squishTime);
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			for (int i = 0; i < affectedEntities.Count; i++)
			{
				if (affectedEntities[i] && !affectedEntities[i].Squished)
				{
					affectedEntities[i].ExternalActivity.moveMods.Remove(affectedMoveMod);
					affectedEntities.RemoveAt(i--);
				}
			}
		}

		public override float DistanceCheck(float val) =>
			val * currentJumpDistance;


		float currentJumpDistance = 1f;


		[SerializeField]
		internal SpriteRenderer renderer;
		[SerializeField]
		internal SoundObject audJump, audStomp;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Sprite sprIdle, sprPrepare, sprJump;

		[SerializeField]
		internal int minJumpDistance = 1, maxJumpDistance = 3;

		[SerializeField]
		internal float squishTime = 10f, minIdleTime = 0.5f, maxIdleTime = 0.8f, leapingInAirTimer = 0.5f, prepJumpCooldown = 0.3f, speed = 20f;

		readonly MovementModifier myMoveMod = new(Vector3.zero, 0f), affectedMoveMod = new(Vector3.zero, 0.3f);

		readonly List<Entity> affectedEntities = [];
	}

	internal class Leapy_StateBase(Leapy le) : NpcState(le)
	{
		protected Leapy le = le;
	}

	internal class Leapy_Idle(Leapy le) : Leapy_StateBase(le)
	{
		public override void Enter()
		{
			base.Enter();
			le.Idle();
			le.Navigator.maxSpeed = 0f;
			le.Navigator.SetSpeed(0f);
		}

		public override void Initialize()
		{
			base.Initialize();
			le.StopMe(true);
		}

		public override void Update()
		{
			base.Update();
			idleCool -= le.TimeScale * Time.deltaTime;
			if (idleCool <= 0f)
				le.behaviorStateMachine.ChangeState(new Leapy_PrepareJump(le));
		}

		float idleCool = Random.Range(le.minIdleTime, le.maxIdleTime);
		
	}

	internal class Leapy_PrepareJump(Leapy le) : Leapy_StateBase(le)
	{
		float prepCool = le.prepJumpCooldown;
		public override void Enter()
		{
			base.Enter();
			le.PrepareSprite();
		}

		public override void Update()
		{
			base.Update();
			prepCool -= le.TimeScale * Time.deltaTime;
			if (prepCool <= 0f)
				le.behaviorStateMachine.ChangeState(new Leapy_Jump(le));
		}
	}

	internal class Leapy_Jump(Leapy le) : Leapy_StateBase(le)
	{
		float jumpCool = le.leapingInAirTimer;
		public override void Enter()
		{
			base.Enter();
			le.Navigator.maxSpeed = le.speed;
			le.Navigator.SetSpeed(le.speed);
			le.Jump();
		}
		public override void Initialize()
		{
			base.Initialize();
			le.StopMe(false);
		}

		public override void Update()
		{
			base.Update();
			jumpCool -= le.TimeScale * Time.deltaTime;
			if (jumpCool <= 0f)
				le.behaviorStateMachine.ChangeState(new Leapy_Idle(le));
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
					le.Stomp(e);
			}
		}
	}
}
