using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Dribble : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = normSpeed;
			navigator.SetSpeed(normSpeed);
			behaviorStateMachine.ChangeState(new Dribble_Idle(this));
			Home = ec.CellFromPosition(transform.position).room;
			basketball = Instantiate(basketPre);
			basketball.Initialize(this);
		}

		internal void Bounce() =>
			bounceAudMan.PlaySingle(audBounceBall);

		internal void IdleNoise() =>
			audMan.PlayRandomAudio(audIdle);

		internal void NoticeNoise() =>
			audMan.PlayRandomAudio(audNotice);

		internal void ComingNoise() =>
			audMan.PlayRandomAudio(audCaught);
		internal void Disappointed() =>
			audMan.PlayRandomAudio(audDisappointed);
		internal void AngryNoise(bool chasing) =>
			audMan.PlayRandomAudio(chasing ? audChaseAngry : audAngry);

		internal void Clap() =>
			audMan.PlaySingle(audClap);

		internal void TeleportToClass(PlayerManager pm)
		{
			navigator.Entity.Teleport(ec.RealRoomMid(Home));
			pm.Teleport(Home.RandomEntitySafeCellNoGarbage().CenterWorldPosition);
			Physics.SyncTransforms();
			pm.transform.LookAt(transform);
		}

		internal void ThrowBasketball(PlayerManager pm)
		{
			Vector3 rot = (pm.transform.position - transform.position).normalized;
			basketball.Throw(rot, transform.position + (rot.ZeroOutY() * 1.5f), pm);
		}

		internal void MinigameEnd(bool failed, PlayerManager player)
		{
			if (behaviorStateMachine.CurrentState is Dribble_Chase) return; // When he's already chasing, this serves no purpose
			if (!failed)
			{
				Singleton<CoreGameManager>.Instance.AddPoints(100, player.playerNumber, true);
				behaviorStateMachine.ChangeState(new Dribble_MinigameSucceed(this));
				return;
			}
			behaviorStateMachine.ChangeState(new Dribble_MinigameFail(this, player));
		}

		internal void DisappointDribble() =>
			behaviorStateMachine.ChangeState(new Dribble_Disappointed(this));
		internal void Step()
		{
			_step = !_step;
			bounceAudMan.PlaySingle(_step ? audStep[0] : audStep[1]);
		}
		internal void PunchNPC(Entity entity)
		{
			bounceAudMan.PlaySingle(audPunch);
			audMan.PlayRandomAudio(audPunchResponse);
			float f = Random.Range(9f, 12f);
			entity.AddForce(new Force((Random.value < 0.5f ? transform.right : -transform.right) * 2.5f, f, -(f - 2.5f)));
			entity.StartCoroutine(Punched(entity));
		}

		IEnumerator Punched(Entity entity)
		{
			entity.ExternalActivity.moveMods.Add(punchMod);
			float cool = 5f;
			while (cool > 0f)
			{
				cool -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}
			entity.ExternalActivity.moveMods.Remove(punchMod);
			yield break;
		}

		public override void Despawn()
		{
			base.Despawn();
			Destroy(basketball);
		}


		[SerializeField]
		internal Sprite[] idleSprs, clapSprs, classSprs, disappointedSprs, crazySprs, chasingSprs;

		[SerializeField]
		internal SoundObject[] audIdle, audNotice, audPraise, audDisappointed, audAngry, audChaseAngry, audCaught, audStep, audAngryCaught, audPunchResponse;

		[SerializeField]
		internal SoundObject audCatch, audClap, audDismissed, audInstructions, audReady, audBounceBall, audThrow, audPunch;

		[SerializeField]
		internal PropagatedAudioManager audMan, bounceAudMan;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal PickableBasketball basketPre;

		PickableBasketball basketball;
		bool _step = false;


		readonly internal TimeScaleModifier introMod = new(0f, 0f, 0f);

		readonly internal MovementModifier moveMod = new(Vector3.zero, 0f), punchMod = new(Vector3.zero, 0.35f);

		internal float normSpeed = 14f, chaseSpeed = 21f, angryChaseSpeed = 22.5f;

		internal RoomController Home { get; private set; }
	}

	internal class DribbleStateBase(Dribble dr) : NpcState(dr)
	{
		readonly protected Dribble dr = dr;
	}

	internal class DribbleWanderStateBase(Dribble dr) : DribbleStateBase(dr)
	{
		public override void Update()
		{
			base.Update();
			float mag = dr.Navigator.Velocity.magnitude;
			if (Time.timeScale > 0f && mag > 0.1f * Time.deltaTime)
			{
				stepDelay -= mag;
				if (stepDelay <= 0f)
				{
					stepDelay += 5f;
					step = !step;
					if (step)
						dr.Bounce();
					dr.renderer.sprite = dr.idleSprs[step ? 1 : 0];
				}
			}
		}

		float stepDelay = 5f;
		bool step = false;
	}

	internal class Dribble_Idle(Dribble dr, float cooldown = 0f) : DribbleWanderStateBase(dr)
	{
		float cooldown = cooldown;

		public override void Enter()
		{
			base.Enter();
			dr.Navigator.maxSpeed = dr.normSpeed;
			dr.Navigator.SetSpeed(dr.normSpeed);
			dr.Navigator.Am.moveMods.Remove(dr.moveMod);
			ChangeNavigationState(new NavigationState_WanderRandom(dr, 0));
		}

		public override void Update()
		{
			base.Update();
			sayCooldown -= Time.deltaTime * dr.TimeScale;
			if (sayCooldown <= 0f)
			{
				sayCooldown += Random.Range(15f, 30f);
				if (Random.value > 0.7f)
					dr.IdleNoise();
			}

			if (cooldown > 0f)
				cooldown -= Time.deltaTime * dr.TimeScale;
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			if (!player.Tagged && cooldown <= 0f)
				dr.behaviorStateMachine.ChangeState(new Dribble_NoticeChase(dr, player));
		}

		float sayCooldown = Random.Range(15f, 30f);
	}

	internal class Dribble_NoticeChase(Dribble dr, PlayerManager player) : DribbleWanderStateBase(dr)
	{
		readonly PlayerManager player = player;
		NavigationState_TargetPlayer state;
		public override void Enter()
		{
			base.Enter();
			dr.NoticeNoise();
			dr.Navigator.maxSpeed = dr.chaseSpeed;
			dr.Navigator.SetSpeed(dr.chaseSpeed);
			state = new NavigationState_TargetPlayer(dr, 63, player.transform.position, true);
			ChangeNavigationState(state);
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			state.UpdatePosition(player.transform.position);
			if (player.Tagged && player == this.player)
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr));
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == player.gameObject)
				dr.behaviorStateMachine.ChangeState(new Dribble_Inform(dr, player));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			state.priority = 0;
			dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr));
		}

		public override void Exit()
		{
			base.Exit();
			state.priority = 0;
		}
	}

	internal class Dribble_Inform(Dribble dr, PlayerManager player) : DribbleStateBase(dr)
	{
		readonly PlayerManager player = player;
		public override void Enter()
		{
			base.Enter();
			dr.ec.AddTimeScale(dr.introMod);
			dr.ComingNoise();
			dr.StartCoroutine(WaitForInform());
		}

		IEnumerator WaitForInform()
		{
			dr.renderer.sprite = dr.clapSprs[0];
			dr.Navigator.Am.moveMods.Add(dr.moveMod);
			player.plm.am.moveMods.Add(dr.moveMod);
			while (dr.audMan.QueuedAudioIsPlaying)
			{
				player.transform.RotateSmoothlyToNextPoint(dr.transform.position, 0.8f);
				yield return null;
			}
			player.plm.am.moveMods.Remove(dr.moveMod);
			float cool = Random.Range(1f, 1.5f);
			while (cool > 0f)
			{
				player.transform.RotateSmoothlyToNextPoint(dr.transform.position, 0.8f);
				cool -= Time.deltaTime;
				yield return null;
			}
			dr.renderer.sprite = dr.clapSprs[1];
			dr.Clap();
			dr.TeleportToClass(player);
			dr.ec.RemoveTimeScale(dr.introMod);

			for (int i = 0; i < 3; i++) // Frame delay
				yield return null;

			dr.renderer.sprite = dr.clapSprs[0];
			dr.behaviorStateMachine.ChangeState(new Dribble_ClassTime(dr, player));

			yield break;
		}


	}

	internal class Dribble_ClassTime(Dribble dr, PlayerManager pm) : DribbleStateBase(dr)
	{
		readonly PlayerManager player = pm;
		Coroutine classEnum;

		public override void Enter()
		{
			base.Enter();
			classEnum = dr.StartCoroutine(ClassTime());
		}

		public override void Update()
		{
			base.Update();
			if (dr.ec.CellFromPosition(player.transform.position).room != dr.Home)
			{
				dr.audMan.FlushQueue(true);
				dr.bounceAudMan.FlushQueue(true);
				dr.behaviorStateMachine.ChangeState(new Dribble_Chase(dr, player));
				dr.StopCoroutine(classEnum);
			}
		}

		IEnumerator ClassTime()
		{
			float cool = Random.Range(1.5f, 2f);

			while (cool > 0f)
			{
				cool -= Time.deltaTime;
				yield return null;
			}
			dr.renderer.sprite = dr.classSprs[0];
			dr.audMan.QueueAudio(dr.audInstructions);
			while (dr.audMan.QueuedAudioIsPlaying)
				yield return null;
			cool = 0.5f;
			while (cool > 0f)
			{
				cool -= dr.TimeScale * Time.deltaTime;
				yield return null;
			}

			dr.audMan.QueueAudio(dr.audReady);
			dr.renderer.sprite = dr.classSprs[1];

			while (dr.audMan.QueuedAudioIsPlaying)
				yield return null;

			cool = Random.Range(1.5f, 2.5f);
			while (cool > 0f)
			{
				cool -= dr.TimeScale * Time.deltaTime;
				yield return null;
			}

			dr.bounceAudMan.PlaySingle(dr.audThrow);
			dr.audMan.QueueAudio(dr.audCatch);
			dr.renderer.sprite = dr.classSprs[2];
			dr.ThrowBasketball(player);

			yield break;
		}
	}

	internal class Dribble_MinigameSucceed(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		bool clapped = false;
		float cooldown = 5f;
		public override void Enter()
		{
			base.Enter();
			dr.bounceAudMan.FlushQueue(true);
			dr.bounceAudMan.QueueRandomAudio(dr.audPraise);
		}
		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 11f;
			frame %= dr.clapSprs.Length;
			if (!clapped)
			{
				if (frame < 1f)
				{
					dr.audMan.PlaySingle(dr.audClap);
					clapped = true;
				}
			}
			else if (frame > 1f)
				clapped = false;

			dr.renderer.sprite = dr.clapSprs[Mathf.FloorToInt(frame)];

			cooldown -= Time.deltaTime * dr.TimeScale;
			if (cooldown < 0f)
			{
				dr.audMan.FlushQueue(true);
				dr.audMan.PlaySingle(dr.audDismissed);
				dr.renderer.sprite = dr.idleSprs[0];
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, Random.Range(15f, 30f)));
			}
		}
	}

	internal class Dribble_Disappointed(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		float cooldown = 2.5f;
		public override void Enter()
		{
			base.Enter();
			dr.audMan.FlushQueue(true);
			dr.Disappointed();
		}

		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 6f;
			frame %= dr.disappointedSprs.Length;
			dr.renderer.sprite = dr.disappointedSprs[Mathf.FloorToInt(frame)];
			if (!dr.audMan.QueuedAudioIsPlaying)
			{
				cooldown -= Time.deltaTime * dr.TimeScale;
				if (cooldown < 0f)
				{
					dr.audMan.FlushQueue(true);
					dr.audMan.PlaySingle(dr.audDismissed);
					dr.renderer.sprite = dr.idleSprs[0];
					dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, Random.Range(15f, 30f)));
				}
			}

		}
	}

	internal class Dribble_AngrySwingingBase(Dribble dr) : DribbleStateBase(dr)
	{
		float frame = 0f;
		public override void Update()
		{
			base.Update();
			frame += Time.deltaTime * dr.TimeScale * 11f;
			frame %= dr.crazySprs.Length;

			dr.renderer.sprite = dr.crazySprs[Mathf.FloorToInt(frame)];
		}
	}

	internal class Dribble_MinigameFail(Dribble dr, PlayerManager pm) : Dribble_AngrySwingingBase(dr)
	{
		readonly PlayerManager pm = pm;
		public override void Enter()
		{
			base.Enter();
			dr.ec.MakeNoise(dr.transform.position, 36);
			dr.AngryNoise(false);
		}
		public override void Update()
		{
			base.Update();
			if (dr.ec.CellFromPosition(pm.transform.position).room != dr.Home)
				dr.behaviorStateMachine.ChangeState(new Dribble_Chase(dr, pm));
		}


	}

	internal class Dribble_Chase(Dribble dr, PlayerManager pm) : DribbleStateBase(dr) // Pretty much what dr reflex does, but not squish
	{
		NavigationState_TargetPlayer state;
		readonly PlayerManager pm = pm;
		float stepDelay = 0f;
		int idx = 0;

		public override void Enter()
		{
			base.Enter();
			dr.renderer.sprite = dr.chasingSprs[0];
			dr.Navigator.Am.moveMods.Remove(dr.moveMod);
			dr.Navigator.maxSpeed = dr.angryChaseSpeed;
			dr.Navigator.SetSpeed(dr.angryChaseSpeed);
			dr.AngryNoise(true);
			state = new NavigationState_TargetPlayer(dr, 63, pm.transform.position);
			ChangeNavigationState(state);
			dr.Navigator.passableObstacles.Add(PassableObstacle.Bully);
		}

		public override void Exit()
		{
			base.Exit();
			dr.Navigator.passableObstacles.Remove(PassableObstacle.Bully);
			ChangeNavigationState(new NavigationState_DoNothing(dr, 0));
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (dr.behaviorStateMachine.CurrentNavigationState == state && Random.value > 0.6f)
				dr.AngryNoise(true);
			
			ChangeNavigationState(new NavigationState_WanderRandom(dr, 0));
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.isTrigger)
			{
				if (other.CompareTag("Player"))
				{
					var pm = other.GetComponent<PlayerManager>();
					if (pm && pm == this.pm)
						dr.behaviorStateMachine.ChangeState(new Dribble_ForceRun(dr, pm));
				}
				else if (other.CompareTag("NPC"))
				{
					var e = other.GetComponent<Entity>();
					if (e)
						dr.PunchNPC(e);
					
				}
			}
		}

		public override void Hear(Vector3 position, int value)
		{
			base.Hear(position, value);
			if (!dr.looker.PlayerInSight())
			{
				ChangeNavigationState(state);
				state.UpdatePosition(position);
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (pm == player)
			{
				ChangeNavigationState(state);
				state.UpdatePosition(player.transform.position);
			}
		}

		public override void Update()
		{
			base.Update();
			float mag = dr.Navigator.Velocity.magnitude;
			if (Time.timeScale > 0f && mag > 0.1f * Time.deltaTime)
			{
				stepDelay -= mag;
				if (stepDelay <= 0f)
				{
					stepDelay += 5.6f;
					dr.Step();
					idx = 1 - idx;
					dr.renderer.sprite = dr.chasingSprs[idx];
				}
			}
		}
	}

	internal class Dribble_ForceRun(Dribble dr, PlayerManager pm) : Dribble_AngrySwingingBase(dr)
	{
		readonly private PlayerManager pm = pm;
		private PlayerRunCornerFunction func;

		public override void Enter()
		{
			base.Enter();
			dr.Navigator.Am.moveMods.Add(dr.moveMod);
			dr.Navigator.Entity.Teleport(dr.ec.RealRoomMid(dr.Home));
			func = dr.Home.functionObject.GetComponent<PlayerRunCornerFunction>();
			func.MakePlayerRunAround(pm);
			dr.audMan.FlushQueue(true);
			dr.audMan.QueueRandomAudio(dr.audAngryCaught);
			dr.ec.MakeNoise(dr.transform.position, 39);
		}

		public override void Update()
		{
			base.Update();

			if (!func.IsActive)
			{
				dr.audMan.PlaySingle(dr.audDismissed);
				dr.renderer.sprite = dr.idleSprs[0];
				dr.behaviorStateMachine.ChangeState(new Dribble_Idle(dr, Random.Range(15f, 30f)));
			}
		}
	}
}
