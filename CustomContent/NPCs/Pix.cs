
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class Pix : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [
				this.GetSound("Pix_Detected.wav", "Vfx_Pix_TarDet", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Prepare.wav", "Vfx_Pix_Prepare", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Stop.wav", "Vfx_Pix_Stop1", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Easy.wav", "Vfx_Pix_Ez", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Successful.wav", "Vfx_Pix_MisSuc", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Failed.wav", "Vfx_Pix_MisFail", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Grrr.wav", "Vfx_Pix_Grr", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_NextTime.wav", "Vfx_Pix_this.GetYou", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSound("Pix_Shoot.wav", "Vfx_Pix_Shoot", SoundType.Voice, new(0.6f, 0f, 0f)),
				this.GetSoundNoSub("shock.wav", SoundType.Voice)
				];

			soundObjects[2].additionalKeys = [new() { key = "Vfx_Pix_Stop2", time = 1.3f }, new() { key = "Vfx_Pix_Stop3", time = 1.724f }];

			// Setup audio
			audMan = GetComponent<PropagatedAudioManager>();
			audReady = [soundObjects[0], soundObjects[1], soundObjects[2]];
			audHappy = [soundObjects[3], soundObjects[4]];
			audAngry = [soundObjects[5], soundObjects[6], soundObjects[7]];
			audShoot = soundObjects[8];

			Sprite[] storedSprites = [.. this.GetSpriteSheet(22, 1, 12f, "pix.png"), .. this.GetSpriteSheet(2, 1, 25f, "laserBeam.png"), .. this.GetSpriteSheet(4, 1, 15f, "shock.png")];
			// setup animated sprites
			rotator = spriteRenderer[0].CreateAnimatedSpriteRotator(
				GenericExtensions.CreateRotationMap(4, storedSprites[0], storedSprites[2], storedSprites[4], storedSprites[6]), // Normal first frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[1], storedSprites[3], storedSprites[5], storedSprites[7]), // Normal second frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[8], storedSprites[10], storedSprites[4], storedSprites[12]), // Angry first frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[9], storedSprites[11], storedSprites[5], storedSprites[13]), // Angry second frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[14], storedSprites[16], storedSprites[4], storedSprites[18]), // Happy first frame of rotation map
				GenericExtensions.CreateRotationMap(4, storedSprites[15], storedSprites[17], storedSprites[5], storedSprites[19]) // Happy second frame of rotation map
				);
			normalSprites = [storedSprites[0], storedSprites[1]];
			spriteRenderer[0].sprite = normalSprites[0];
			angrySprites = [storedSprites[8], storedSprites[9]];
			happySprites = [storedSprites[14], storedSprites[15]];
			idleShootingSprites = [storedSprites[20], storedSprites[21]];

			// laser (16, 17)
			var laserRend = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[23]).AddSpriteHolder(0f, LayerStorage.standardEntities);
			var laserHolder = laserRend.transform.parent;
			laserHolder.gameObject.SetAsPrefab(true);
			laserRend.name = "PixLaserBeamRenderer";
			laserHolder.name = "PixLaserBeam";

			var laser = laserHolder.gameObject.AddComponent<PixLaserBeam>();
			laser.flyingSprites = [storedSprites[22], storedSprites[23]];
			laser.shockSprites = [storedSprites[24], storedSprites[25], storedSprites[26], storedSprites[27]];
			laser.renderer = laserRend;

			laser.entity = laserHolder.gameObject.CreateEntity(2f, 2f, laserHolder.transform).SetEntityCollisionLayerMask(LayerStorage.gumCollisionMask);
			laser.audMan = laserHolder.gameObject.CreatePropagatedAudioManager(15, 45);
			laser.audShock = soundObjects[9];
			laserPre = laser;
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
			if (failed)
				rageStreak = Mathf.Min(rageStreak + 1, 3);
			else
				rageStreak = 0;
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
			var l = Instantiate(laserPre);
			l.InitBeam(this, player);
			StartCoroutine(ShootingAnimation(l));
			

			audMan.PlaySingle(audShoot);
		}

		IEnumerator Shooting(PlayerManager player)
		{
			hasFailed = true;
			beams = 0;
			int max = 3 + (rageStreak * 3);

			for (int i = 0; i < max; i++)
			{
				float cooldown = Mathf.Max(0.25f, 2f - (max / 3f));

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

		IEnumerator ShootingAnimation(PixLaserBeam beam)
		{
			
			rotator.targetSprite = idleShootingSprites[1];
			yield return null;
			if (beam)
				beam.transform.position = transform.position; // workaround for the stupid entity thing from the game

			int frame = 0;
			while (frame++ < idleSpeed)
				yield return null;

			rotator.targetSprite = idleShootingSprites[0];
			yield break;
		}

		internal void SetAsSuccess() => hasFailed = false;

		internal void DecrementBeamCount() => beams = Mathf.Max(0, beams - 1);

		bool hasFailed = true;
		int beams = 0, rageStreak = 0;

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
			pix.Navigator.FindPath(pix.transform.position, player.transform.position);
			ChangeNavigationState(new NavigationState_TargetPosition(pix, 63, pix.Navigator.NextPoint));
			
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
