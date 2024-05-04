using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomContent.NPCs
{
	public class Pix : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = 14f;
			navigator.SetSpeed(14f);
			behaviorStateMachine.ChangeState(new Pix_Wandering(this, 0f));
		}

		public void SetReadyToShoot()
		{
			currentState = 3;
			rotator.targetSprite = idleShootingSprites[0];
			rotator.BypassRotation(true);
			audMan.PlayRandomAudio(audReady);
			navigator.Am.moveMods.Add(moveMod);
		}

		public void DoneShootingState(bool failed)
		{
			audMan.PlayRandomAudio(failed ? audAngry : audHappy);
			currentState = failed ? 1u : 2u;
			navigator.Am.moveMods.Remove(moveMod);
			rotator.BypassRotation(false);
		}

		public void SetToNormalState() =>
			currentState = 0;
		

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			switch (currentState)
			{
				case 3: // do nothing lol
					return;
				case 2:
					frame += walkingSpeed * TimeScale * Time.deltaTime;
					frame %= happySprites.Length;
					rotator.targetSprite = happySprites[Mathf.FloorToInt(frame)];
					return;
				case 1:
					frame += walkingSpeed * TimeScale * Time.deltaTime;
					frame %= angrySprites.Length;
					rotator.targetSprite = angrySprites[Mathf.FloorToInt(frame)];
					return;
				default:
					frame += walkingSpeed * TimeScale * Time.deltaTime;
					frame %= normalSprites.Length;
					rotator.targetSprite = normalSprites[Mathf.FloorToInt(frame)];
					return;
			}
		}

		public void InitiateShooting(PlayerManager target)
		{
			SetReadyToShoot();
			StartCoroutine(Shooting(target));
		}

		void Shoot(PlayerManager player)
		{
			beams++;
			StartCoroutine(ShootingAnimation());
			var laser = Instantiate(laserPre);
			laser.pix = this;
			laser.targetPlayer = player;
			laser.gameObject.SetActive(true);
			audMan.PlaySingle(audShoot);
		}

		IEnumerator Shooting(PlayerManager player)
		{
			hasFailed = true;
			beams = 0;

			for (int i = 0; i < 3; i++)
			{
				float cooldown = 1f;

				while (audMan.AnyAudioIsPlaying)
					yield return null; // Wait til it is done

				while (cooldown > 0f)
				{
					cooldown -= TimeScale * Time.deltaTime;
					yield return null;
				}

				Shoot(player);
				yield return null;
			}

			while (beams > 0)
				yield return null;

			DoneShootingState(hasFailed);
			behaviorStateMachine.ChangeState(new Pix_Wandering(this, 20f));

			yield break;
		}

		IEnumerator ShootingAnimation()
		{
			rotator.targetSprite = idleShootingSprites[1];
			int frame = 0;
			while (frame++ < idleSpeed)
				yield return null;

			rotator.targetSprite = idleShootingSprites[0];
			yield break;
		}

		internal void SetAsSuccess() => hasFailed = false;

		internal void DecrementBeamCount() => beams = Mathf.Max(0, beams - 1);

		bool hasFailed = true;
		int beams = 0;

		[SerializeField]
		internal AnimatedSpriteRotator rotator;

		[SerializeField]
		internal Sprite[] idleShootingSprites, normalSprites, angrySprites, happySprites;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject[] audReady, audAngry, audHappy;

		[SerializeField]
		internal SoundObject audShoot;

		[SerializeField]
		internal PixLaserBeam laserPre;

		uint currentState = 0; // 0 = normal, 1 = angry, 2 = happy, 3 = idle

		float frame = 0f;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		const float idleSpeed = 10f, walkingSpeed = 5f;
	}

	internal class Pix_StateBase(Pix pix) : NpcState(pix)
	{
		protected Pix pix = pix;
	}

	internal class Pix_Wandering : Pix_StateBase
	{
		internal Pix_Wandering(Pix pix, float cooldown) : base(pix)
		{
			this.cooldown = cooldown;
			requireNormalState = cooldown > 0f;
		}
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(pix, 0));
		}

		public override void Update()
		{
			base.Update();
			if (!requireNormalState) return;

			cooldown -= pix.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
			{
				pix.SetToNormalState();
				requireNormalState = false;
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			if (requireNormalState) return;

			base.PlayerInSight(player);
			if (!player.Tagged)
				pix.behaviorStateMachine.ChangeState(new Pix_PrepShoot(pix, player));
		}

		float cooldown;

		bool requireNormalState;
	}

	internal class Pix_PrepShoot(Pix pix, PlayerManager player) : Pix_StateBase(pix)
	{
		public override void Enter()
		{
			base.Enter();
			pix.Navigator.maxSpeed = 26f;
			pix.Navigator.SetSpeed(26f);
			var cell = pix.ec.CellFromPosition(pix.transform.position);
			ChangeNavigationState(new NavigationState_TargetPosition(pix, 63, cell.open ? cell.FloorWorldPosition : pix.Navigator.NextPoint));
			
		}
		public override void DestinationEmpty()
		{

			if (pix.looker.PlayerInSight() && !player.Tagged)
			{
				base.DestinationEmpty();
				pix.InitiateShooting(player);
				pix.behaviorStateMachine.ChangeState(new Pix_StateBase(pix)); // Who will change state now is Pix himself
				return;
			}

			pix.behaviorStateMachine.ChangeState(new Pix_Wandering(pix, 0f));
		}

		public override void Exit()
		{
			base.Exit();
			pix.Navigator.maxSpeed = 14f;
			pix.Navigator.SetSpeed(14f);
		}

		readonly protected PlayerManager player = player;
	}
}
