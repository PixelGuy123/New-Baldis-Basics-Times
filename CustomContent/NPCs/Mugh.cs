﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Bindings;

namespace BBTimes.CustomContent.NPCs
{
	public class Mugh : NPC, IItemAcceptor
	{
		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new Mugh_Wandering(this));
			navigator.Am.moveMods.Add(walkMod);
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			float spd = Mathf.Abs(Mathf.Sin(Time.fixedTime * navigator.speed * TimeScale * slownessWalkFactor));
			if (spd > 0.5f)
			{
				if (!isWalking)
				{
					isWalking = true;
					walkAudMan.PlaySingle(audWalk);
				}
			}
			else if (spd < 0.5f && isWalking)
				isWalking = false;

			walkMod.movementMultiplier = spd;
		}

		public void SeeYouNoise()
		{
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audFindPlayer);
		}

		public void SadState()
		{
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audLostPlayer);
			renderer.sprite = sadSprite;
		}

		public void HugState() =>
			renderer.sprite = hugSprite;
		

		public void NormalState() =>
			renderer.sprite = normSprite;

		public void DeadState() =>
			StartCoroutine(DeadSequence());

		IEnumerator DeadSequence()
		{
			renderer.sprite = holeSprite;
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audGetHit);
			while (audMan.AnyAudioIsPlaying) yield return null;

			float delay = 1f;
			while (delay > 0f)
			{
				delay -= TimeScale * Time.deltaTime;
				yield return null;
			}

			audMan.PlaySingle(audDie);
			renderer.sprite = deadSprite;
			yield break;
		}

		public void ReviveNoise() =>
			audMan.PlaySingle(audRevive);

		public bool ItemFits(Items itm) =>
			hittableItms.Contains(itm);
		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			pm.RuleBreak("Bullying", 3f);
			behaviorStateMachine.ChangeState(new Mugh_DieSadMoment(this));
		}
		


		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite normSprite, hugSprite, sadSprite;

		[SerializeField]
		internal Sprite holeSprite, deadSprite;

		[SerializeField]
		internal AudioManager audMan, walkAudMan;

		[SerializeField]
		internal SoundObject[] audFindPlayer;

		[SerializeField]
		internal SoundObject[] audLostPlayer;

		[SerializeField]
		internal SoundObject[] audGetHit;

		[SerializeField]
		internal SoundObject audWalk, audDie, audRevive;

		[SerializeField]
		[Range(0.0f, 1.0f)]
		internal float slownessWalkFactor = 0.1f;

		readonly MovementModifier walkMod = new(Vector3.zero, 1f);

		bool isWalking = true;

		readonly static HashSet<Items> hittableItms = [];

		public static void AddHittableItem(Items itm) =>
			hittableItms.Add(itm);
	}

	internal class Mugh_StateBase(Mugh mu) : NpcState(mu)
	{
		protected Mugh mu = mu;
	}

	internal class Mugh_Wandering(Mugh mu, float cooldown = 0f, bool sad = false) : Mugh_StateBase(mu)
	{
		float cooldown = cooldown;
		readonly bool sad = sad;
		public override void Enter()
		{
			base.Enter();
			mu.Navigator.maxSpeed = 12f;
			mu.Navigator.SetSpeed(12f);
			ChangeNavigationState(new NavigationState_WanderRandom(mu, 0));
			if (sad)
				mu.SadState();
		}

		public override void Update()
		{
			base.Update();
			cooldown -= mu.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				mu.NormalState();
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (cooldown <= 0f && !player.Tagged)
				mu.behaviorStateMachine.ChangeState(new Mugh_FollowPlayer(mu, player));
		}
	}

	internal class Mugh_FollowPlayer(Mugh mu, PlayerManager pm) : Mugh_StateBase(mu)
	{
		NavigationState_TargetPlayer state;
		readonly PlayerManager pm = pm;
		public override void Enter()
		{
			base.Enter();
			mu.SeeYouNoise();
			mu.Navigator.maxSpeed = 36f;
			mu.Navigator.SetSpeed(36f);
			state = new(mu, 63, pm.transform.position, true);
			ChangeNavigationState(state);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (player == pm)
				state.UpdatePosition(player.transform.position);
			
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == pm.gameObject)
				mu.behaviorStateMachine.ChangeState(new Mugh_HugPlayer(mu, pm));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			state.priority = 0;
			mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu));
		}

		public override void Exit()
		{
			base.Exit();
			state.priority = 0;
		}
	}

	internal class Mugh_HugPlayer(Mugh mu, PlayerManager pm) : Mugh_StateBase(mu)
	{
		readonly PlayerManager pm = pm;
		readonly MovementModifier hugMod = new(Vector3.zero, 0.72f);

		public override void Enter()
		{
			base.Enter();
			mu.Navigator.maxSpeed = 0;
			mu.Navigator.SetSpeed(0);
			mu.HugState();
			pm.Am.moveMods.Add(hugMod);
		}

		public override void Update()
		{
			base.Update();
			if (!pm)
			{
				mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu, 30f));
				return;
			}
			var dist = mu.transform.position - pm.transform.position;
			hugMod.movementAddend = dist * 115f * Time.deltaTime * mu.TimeScale;
			
		}

		public override void OnStateTriggerExit(Collider other)
		{
			base.OnStateTriggerExit(other);
			if (other.gameObject == pm.gameObject)
				mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu, 30f, true));
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (player == pm)
				mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu, 30f, true));
		}

		public override void Exit()
		{
			base.Exit();
			pm?.Am.moveMods.Remove(hugMod);
		}
	}

	internal class Mugh_DieSadMoment(Mugh mu) : Mugh_StateBase(mu)
	{
		float reviveCooldown = 25f;
		bool reviving = false;
		public override void Enter()
		{
			base.Enter();
			mu.Navigator.SetSpeed(0f);
			mu.Navigator.maxSpeed = 0f;
			mu.DeadState();
		}

		public override void Update()
		{
			base.Update();
			reviveCooldown -= mu.TimeScale * Time.deltaTime;
			if (reviveCooldown < 0f)
			{
				if (!reviving)
				{
					reviving = true;
					mu.ReviveNoise();
					return;
				}
				if (!mu.audMan.AnyAudioIsPlaying)
				{
					mu.NormalState();
					mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu, 5f));
				}
			}
		}
	}
}