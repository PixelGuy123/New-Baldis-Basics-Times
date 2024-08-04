using BBTimes.CustomComponents.CustomDatas;
using System.Collections;
using UnityEngine;

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

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
		}
	}
	public class SuperIntendentJr : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			timeInSight = new float[players.Count];
			behaviorStateMachine.ChangeState(new SuperIntendentJr_StateBase(this));
		}

		void CallPrincipals()
		{
			foreach (var n in ec.Npcs)
				if (n.Navigator.enabled && (n.Character == Character.Principal || (n.GetComponent<CustomNPCData>()?.ReplacesCharacter(Character.Principal) ?? false)))
					n.behaviorStateMachine.ChangeNavigationState(new NavigationState_FollowToSpot(n, ec.CellFromPosition(transform.position)));

			Directions.ReverseList(navigator.currentDirs);
			behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));
			StartCoroutine(CallOutDelay());
		}

		IEnumerator CallOutDelay()
		{
			noticeCooldown = 30f;
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

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (Time.timeScale > 0f && navigator.Velocity.magnitude > 0.1f * Time.deltaTime)
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

			if (noticeCooldown <= 0f)
			{
				foreach (NPC npc in ec.Npcs)
				{
					if (npc.Disobeying)
					{
						looker.Raycast(npc.transform, Mathf.Min((transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude,looker.distance,npc.ec.MaxRaycast), out bool flag);
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

		float[] timeInSight;

		float noticeCooldown = 0f, wanderCool = 15f, longAssInstructionChance = 0.01f;

		[SerializeField]
		internal PropagatedAudioManager audMan, stepMan;

		[SerializeField]
		internal SoundObject audStep1, audStep2, audWarn, audWonder, audLongAssInstructions;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] anim;

		bool callingOut = false, step = false, wonder = true;

		float stepDelay = stepMax;

		const float stepMax = 6f, brokenRuleTimer = 5f;
	}
}
