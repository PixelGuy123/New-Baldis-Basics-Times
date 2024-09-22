using BBTimes.CustomComponents;
using System.Collections;
using UnityEngine;
using PixelInternalAPI.Extensions;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.NPCs
{
    public class NavigationState_FollowToSpot(NPC npc, Cell target) : NavigationState_TargetPosition(npc, 31, target.CenterWorldPosition)
	{
		readonly Cell tar = target;
		readonly MovementModifier moveMod = new(Vector3.zero, 13.33f);
		public override void Enter()
		{
			base.Enter();
			npc.Navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
			npc.Navigator.FindPath(tar.CenterWorldPosition);
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (npc.ec.CellFromPosition(npc.transform.position) != tar)
				npc.Navigator.FindPath(tar.CenterWorldPosition);
			else npc.behaviorStateMachine.RestoreNavigationState();
		}

		public override void Exit()
		{
			base.Exit();
			npc.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
		}

	}
	internal class SuperIntendentJr_StateBase(NPC npc) : NpcState(npc)
	{
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

	internal class SuperIntendentJr_Wander(NPC npc) : NpcState(npc)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
		}
	}

	internal class SuperIntendentJr_RunForNoise(NPC npc, Vector3 vec, NpcState prevState) : NpcState(npc)
	{
		readonly Cell tar = npc.ec.CellFromPosition(vec);
		public override void Enter()
		{
			base.Enter();
			npc.Navigator.Am.moveMods.Add(moveMod);
			ChangeNavigationState(new NavigationState_TargetPosition(npc, 63, vec));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (npc.ec.CellFromPosition(npc.transform.position) == tar)
			{
				npc.behaviorStateMachine.ChangeState(prevState);
				return;
			}
			ChangeNavigationState(new NavigationState_TargetPosition(npc, 63, vec));

		}

		public override void Exit()
		{
			base.Exit();
			npc.Navigator.Am.moveMods.Remove(moveMod);
		}

		readonly MovementModifier moveMod = new(Vector3.zero, 2.5f);
	}
	public class SuperIntendentJr : NPC, INPCPrefab
	{
		public void SetupPrefab() // edit me
		{
			SoundObject[] soundObjects = [
			this.GetSound("spj_principal.wav", "Vfx_Spj_Found", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			this.GetSound("spj_wonder.wav", "Vfx_Spj_Wander", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			this.GetSound("spj_step1.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			this.GetSound("spj_step2.wav", "Vfx_Spj_Step", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f)),
			this.GetSound("spj_wtfisthis.wav", "Vfx_Spj_FoundLong1", SoundType.Voice, new(0.23828125f, 0.06640625f, 0.51953125f))
			];

			soundObjects[4].additionalKeys = [
					new () { key = "Vfx_Spj_FoundLong2", time = 7.27f },
				new () { key = "Vfx_Spj_FoundLong3", time = 17.911f },
				new() { key = "Vfx_Spj_FoundLong4", time = 27.379f }
			];
			anim = this.GetSpriteSheet(2, 2, 72f, "spj.png");
			audWarn = soundObjects[0];
			audWonder = soundObjects[1];
			audStep1 = soundObjects[2];
			audStep2 = soundObjects[3];
			audLongAssInstructions = soundObjects[4];
			audMan = GetComponent<PropagatedAudioManager>();
			stepMan = gameObject.CreatePropagatedAudioManager(audMan.minDistance, audMan.maxDistance);
			renderer = spriteRenderer[0];
			spriteRenderer[0].sprite = anim[0];
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
			timeInSight = new float[players.Count];
			navigator.Entity.OnTeleport += Teleport;
			behaviorStateMachine.ChangeState(new SuperIntendentJr_Wander(this));
		}

		void CallPrincipals()
		{
			foreach (var n in ec.Npcs)
			{
				var dat = n.GetComponent<INPCPrefab>();
				if (n.Navigator.enabled && (n.Character == Character.Principal || (dat != null &&  dat.ReplacesCharacter(Character.Principal))))
					n.behaviorStateMachine.ChangeNavigationState(new NavigationState_FollowToSpot(n, ec.CellFromPosition(transform.position)));
			}

			Directions.ReverseList(navigator.currentDirs);
			behaviorStateMachine.ChangeState(new SuperIntendentJr_Wander(this));
			StartCoroutine(CallOutDelay());
		}

		IEnumerator CallOutDelay()
		{
			noticeCooldown = 7f;
			callingOut = true;
			UpdateStep();
			wonder = false;
			audMan.FlushQueue(true);
			audMan.PlaySingle(Random.value <= longAssInstructionChance ? audLongAssInstructions : audWarn);
			while (audMan.AnyAudioIsPlaying) yield return null;

			callingOut = false;
			wonder = true;
			UpdateStep();

			while (noticeCooldown > 0f)
			{
				noticeCooldown -= TimeScale * Time.deltaTime;
				yield return null;
			}

			yield break;
		}

		void UpdateStep() =>
			renderer.sprite = step ? anim[callingOut ? 2 : 0] : anim[callingOut ? 3 : 1];

		void Wander()
		{
			if (wonder && Random.value >= 0.75f)
				audMan.QueueAudio(audWonder);
		}

		public override void Hear(Vector3 position, int value)
		{
			base.Hear(position, value);
			if (value >= 78 && value <= 120)
				behaviorStateMachine.ChangeState(new SuperIntendentJr_RunForNoise(this, position, behaviorStateMachine.CurrentState));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (Time.timeScale > 0f && !stopStep && navigator.Velocity.magnitude > 0.1f * Time.deltaTime)
			{
				stepDelay -= navigator.Velocity.magnitude;
				if (stepDelay <= 0f)
				{
					stepDelay += stepMax;
					step = !step;
					stepMan.PlaySingle(step ? audStep1 : audStep2);
					UpdateStep();
				}
			}

			stopStep = false;

			if (noticeCooldown <= 0f)
			{
				foreach (NPC npc in ec.Npcs)
				{
					if (npc.Disobeying)
					{
						looker.Raycast(npc.transform, Mathf.Min((transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude, looker.distance, npc.ec.MaxRaycast), out bool flag);
						if (flag)
						{
							npc.SetGuilt(brokenRuleTimer, npc.BrokenRule);
							CallPrincipals();
							break;
						}
					}
				}
			}

			wanderCool -= TimeScale * Time.deltaTime;
			if (wanderCool < 0f)
			{
				Wander();
				wanderCool += 15f;
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (noticeCooldown > 0f) return;

			if (player.Disobeying && !player.Tagged)
			{
				timeInSight[player.playerNumber] += Time.deltaTime * TimeScale;
				if (timeInSight[player.playerNumber] >= player.GuiltySensitivity)
				{
					player.RuleBreak(player.ruleBreak, brokenRuleTimer, 0.1f);
					CallPrincipals();
					timeInSight[player.playerNumber] = 0f;
				}
			}
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			timeInSight[player.playerNumber] = 0f;
		}

		void Teleport(Vector3 vec) =>
			stopStep = true;

		float[] timeInSight;
		private float noticeCooldown = 0f;
		private float wanderCool = 15f;
		private readonly float longAssInstructionChance = 0.01f;
		[SerializeField]
		internal PropagatedAudioManager audMan, stepMan;

		[SerializeField]
		internal SoundObject audStep1, audStep2, audWarn, audWonder, audLongAssInstructions;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] anim;

		bool callingOut = false, step = false, wonder = true, stopStep = false;

		float stepDelay = stepMax;

		const float stepMax = 6f, brokenRuleTimer = 5f;
	}
}
