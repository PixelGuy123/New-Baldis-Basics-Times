using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class NoseMan : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();
			renderer = spriteRenderer[0];
			var sprs = this.GetSpriteSheet(2, 1, 25f, "noseMan.png");
			sprNormal = sprs[0];
			sprSneeze = sprs[1];
			renderer.sprite = sprNormal;

			audSeekingAttention = new SoundObject[6];
			var speechColor = new Color(0.99609375f, 0.84765625f, 0.69921875f);
			for (int i = 0; i < audSeekingAttention.Length; i++)
				audSeekingAttention[i] = this.GetSound($"attention{i + 1}.mp3", $"Vfx_NOSE_Attention{i + 1}", SoundType.Voice, speechColor);
			audGaveUp = new SoundObject[2];
			for (int i = 0; i < audGaveUp.Length; i++)
				audGaveUp[i] = this.GetSound($"gaveup{i + 1}.mp3", $"Vfx_NOSE_GiveUp{i + 1}", SoundType.Voice, speechColor);
			audSneeze = this.GetSound("sneeze.mp3", "Vfx_NOSE_Sneeze", SoundType.Voice, speechColor);
			audReward = this.GetSound("reward.mp3", "Vfx_NOSE_Reward", SoundType.Voice, speechColor);
			audPostSneeze = this.GetSound("postSneeze.mp3", "Vfx_NOSE_PostSneeze", SoundType.Voice, speechColor);
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";

		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject[] audSeekingAttention, audGaveUp;

		[SerializeField]
		internal SoundObject audSneeze, audReward, audPostSneeze;

		[SerializeField]
		internal Sprite sprNormal, sprSneeze;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal float seekAttentionDelay = 2f, patienceCooldown = 30f, delayBeforeNextAnnoyance = 60f, sneezeForce = 75f, distanceFromPlayer = 15f, sneezeDistanceFactor = 0.06f;

		[SerializeField]
		internal int minYtpAmount = 5, maxYtpAmount = 20;

		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new NoseMan_Wander(this));
		}
		public void Walk(bool walk)
		{
			float speed = walk ? 29f : 0f;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}
		public override void Despawn()
		{
			base.Despawn();
			lastPm?.Am.moveMods.Remove(freezeMod);
		}
		public void Hide(bool hide) =>
			renderer.enabled = !hide;
		public void Annoy()
		{
			if (!audMan.QueuedAudioIsPlaying)
				audMan.QueueRandomAudio(audSeekingAttention);
		}
		public void GiveUp()
		{
			audMan.FlushQueue(true);
			audMan.PlayRandomAudio(audGaveUp);
		}

		public bool IsCloseEnoughToPlayer(PlayerManager pm) =>
			Vector3.Distance(pm.transform.position, transform.position) <= distanceFromPlayer;

		public void SneezePlayer(PlayerManager pm)
		{
			Walk(false);
			renderer.sprite = sprNormal;
			if (sneezeCor != null)
				StopCoroutine(sneezeCor);
			sneezeCor = StartCoroutine(SneezeSequence(pm));
		}

		void SneezeEveryoneAround()
		{
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i] != this && ec.Npcs[i].Navigator.isActiveAndEnabled)
				{
					Vector3 offset = ec.Npcs[i].transform.position - transform.position;
					float force = sneezeForce - offset.magnitude * sneezeDistanceFactor;

					if (force >= 5f)
						ec.Npcs[i].Navigator.Entity.AddForce(new(offset.normalized, force, -force * 0.21f));
				}
			}

			for (int i = 0; i < Singleton<CoreGameManager>.Instance.setPlayers; i++)
			{
				var player = Singleton<CoreGameManager>.Instance.GetPlayer(i);
				Vector3 offset = player.transform.position - transform.position;
				float force = sneezeForce - offset.magnitude * sneezeDistanceFactor;

				if (force >= 5f)
					player.plm.Entity.AddForce(new(offset.normalized, force, -force * 0.21f));

			}
		}

		IEnumerator SneezeSequence(PlayerManager pm)
		{
			lastPm = pm;
			pm.Am.moveMods.Add(freezeMod);
			audMan.FlushQueue(true);
			audMan.QueueAudio(audReward);
			Singleton<CoreGameManager>.Instance.AddPoints(Random.Range(minYtpAmount, maxYtpAmount), pm.playerNumber, true);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audSneeze);
			renderer.sprite = sprSneeze;

			pm.Am.moveMods.Remove(freezeMod);
			float verySmallDelay = 0.595f;
			while (verySmallDelay > 0f)
			{
				verySmallDelay -= TimeScale * Time.deltaTime;
				yield return null;
			}

			SneezeEveryoneAround();

			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			renderer.sprite = sprNormal;
			audMan.QueueAudio(audPostSneeze);
			Walk(true);
		}

		Coroutine sneezeCor;
		PlayerManager lastPm;
		readonly MovementModifier freezeMod = new(Vector3.zero, 0f);
	}

	internal class NoseMan_StateBase(NoseMan nos) : NpcState(nos)
	{
		protected NoseMan nos = nos;
	}

	internal class NoseMan_Wander(NoseMan nos) : NoseMan_StateBase(nos)
	{
		public override void Enter()
		{
			base.Enter();
			nos.Walk(true);
			ChangeNavigationState(new NavigationState_WanderRandom(nos, 0));
		}


		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (!player.Tagged)
				nos.behaviorStateMachine.ChangeState(new NoseMan_WaitForPlayerToDisappear(nos, player));
		}
	}

	internal class NoseMan_WaitForPlayerToDisappear(NoseMan nos, PlayerManager target) : NoseMan_StateBase(nos)
	{
		public override void Enter()
		{
			base.Enter();
			nos.Hide(true);
			nos.Navigator.Entity.Teleport(target.transform.position);
		}

		public override void Update()
		{
			base.Update();
			if (canAppear && !nos.IsCloseEnoughToPlayer(target))
			{
				nos.Hide(false);
				nos.behaviorStateMachine.ChangeState(new NoseMan_AnnoyPlayer(nos, target));
			}
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (player == target)
			{
				canAppear = false;
				frameDelay -= nos.TimeScale * Time.deltaTime;
				if (frameDelay <= 0)
				{
					frameDelay = 1f;
					nos.Navigator.Entity.Teleport(target.transform.position);
				}
			}
		}

		public override void Unsighted()
		{
			base.Unsighted();
			canAppear = true;
		}

		bool canAppear = false;
		float frameDelay = 1f;

	}

	internal class NoseMan_FollowPlayerByDistance(NoseMan nos, PlayerManager target) : NoseMan_StateBase(nos)
	{
		readonly protected PlayerManager pm = target;
		protected bool closeEnough = false;

		NavigationState_TargetPosition tarPos; // Target position so Gravity doesn't matter (it indeed doesn't)
		public override void Enter()
		{
			base.Enter();
			tarPos = new(nos, 64, pm.transform.position);
			ChangeNavigationState(tarPos);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			ChangeNavigationState(tarPos);
		}

		public override void Update()
		{
			base.Update();
			tarPos.UpdatePosition(pm.transform.position);
			closeEnough = nos.IsCloseEnoughToPlayer(pm);
			nos.Walk(!closeEnough);
		}

		public override void Exit()
		{
			base.Exit();
			tarPos.priority = 0;
		}
	}

	internal class NoseMan_AnnoyPlayer(NoseMan nos, PlayerManager target) : NoseMan_FollowPlayerByDistance(nos, target)
	{
		float annoyanceCooldown = nos.patienceCooldown, seekAttentionDelay = 0f;
		readonly float distanceToGiveUp = nos.distanceFromPlayer * 4f;

		public override void Update()
		{
			base.Update();
			annoyanceCooldown -= nos.TimeScale * Time.deltaTime;
			if (annoyanceCooldown <= 0f || Vector3.Distance(pm.transform.position, nos.transform.position) > distanceToGiveUp)
			{
				nos.GiveUp();
				nos.Walk(true);
				nos.behaviorStateMachine.ChangeState(new NoseMan_LeavePlayerAlone(nos, pm));
				return;
			}

			seekAttentionDelay -= nos.TimeScale * Time.deltaTime;
			if (seekAttentionDelay <= 0f)
			{
				seekAttentionDelay += nos.seekAttentionDelay;
				nos.Annoy();
			}
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);
			if (closeEnough && player == pm)
			{
				nos.SneezePlayer(player);
				nos.behaviorStateMachine.ChangeState(new NoseMan_LeavePlayerAlone(nos, pm));
			}
		}
	}

	internal class NoseMan_LeavePlayerAlone(NoseMan nos, PlayerManager pm) : NoseMan_StateBase(nos)
	{
		readonly DijkstraMap dijas = new(nos.ec, PathType.Nav, int.MaxValue, pm.transform);
		float cooldown = nos.delayBeforeNextAnnoyance;
		public override void Enter()
		{
			base.Enter();
			dijas.Activate();
			ChangeNavigationState(new NavigationState_WanderFlee(nos, 0, dijas));
		}
		public override void Update()
		{
			base.Update();
			cooldown -= nos.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				nos.behaviorStateMachine.ChangeState(new NoseMan_Wander(nos));
		}

		public override void Exit()
		{
			base.Exit();
			dijas.Deactivate();
		}
	}
}
