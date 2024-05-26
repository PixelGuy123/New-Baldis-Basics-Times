using BBTimes.CustomComponents.CustomDatas;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class NavigationState_FollowJr(NPC npc, Cell target) : NavigationState(npc, 31)
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
			if (npc.ec.CellFromPosition(npc.transform.position) != tar)
				npc.Navigator.FindPath(tar.CenterWorldPosition);
			else npc.behaviorStateMachine.RestoreNavigationState();
		}

		public void End() =>
			npc.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
	}
	public class SuperIntendentJr : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			timeInSight = new float[players.Count];
			behaviorStateMachine.ChangeState(new NpcState(this));
			behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0)); // Everything can be done from SuperIntendent
		}

		void CallPrincipals()
		{
			foreach (var n in ec.Npcs)
				if (n.Navigator.enabled && (n.Character == Character.Principal || (n.GetComponent<CustomNPCData>()?.ReplacesCharacter(Character.Principal) ?? false)))
					n.behaviorStateMachine.ChangeNavigationState(new NavigationState_FollowJr(n, ec.CellFromPosition(transform.position)));

			StartCoroutine(CallOutDelay());
		}

		IEnumerator CallOutDelay()
		{
			callingOut = true;
			audMan.PlaySingle(audWarn);
			while (audMan.AnyAudioIsPlaying) yield return null;

			callingOut = false;
			StartCoroutine(Timer());

			yield break;
		}

		IEnumerator Timer()
		{
			noticeCooldown = 30f;
			while (noticeCooldown > 0f)
			{
				noticeCooldown -= TimeScale * Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		void UpdateStep() =>
			renderer.sprite = step ? anim[callingOut ? 2 : 0] : anim[callingOut ? 3 : 1];

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
						looker.Raycast(npc.transform, Mathf.Min([(transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude,looker.distance,npc.ec.MaxRaycast]), out bool flag);
						if (flag)
						{
							npc.SetGuilt(brokenRuleTimer, npc.BrokenRule);
							CallPrincipals();
							break;
						}
					}
				}
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
				}
			}
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			timeInSight[player.playerNumber] = 0f;
		}

		float[] timeInSight;

		float noticeCooldown = 0f;

		[SerializeField]
		internal PropagatedAudioManager audMan, stepMan;

		[SerializeField]
		internal SoundObject audStep1, audStep2, audWarn, audWonder;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] anim;

		bool callingOut = false, step = false;

		float stepDelay = stepMax;

		const float stepMax = 2f, brokenRuleTimer = 5f;
	}
}
