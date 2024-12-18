using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using PixelInternalAPI.Classes;
using UnityEngine;
using BBTimes.Manager;
using System.Collections;

namespace BBTimes.CustomContent.NPCs
{
    public class Winterry : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			audSpit = this.GetSound("winterrySpit.wav", "Vfx_Winterry_Spit", SoundType.Voice, Color.white);
			audBlow = this.GetSound("winterryBlowing.wav", "Vfx_Winterry_Blow", SoundType.Voice, Color.white);

			walkAnim = this.GetSpriteSheet(4, 4, 25f, "WinterryWalk.png");
			blowAnim = this.GetSpriteSheet(4, 3, 25f, "WinterryInhale.png").ExcludeNumOfSpritesFromSheet(1);
			spitAnim = this.GetSpriteSheet(4, 2, 25f, "WinterrySpit.png").ExcludeNumOfSpritesFromSheet(1);

			spriteRenderer[0].sprite = walkAnim[0];

			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = spriteRenderer;
			animComp.animation = walkAnim;
			animComp.speed = 8f;

			snowPre = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(46f, "WinterrySnowball.png"))
				.AddSpriteHolder(out var snowBallRenderer, 0f, LayerStorage.standardEntities)
				.gameObject.SetAsPrefab(true)
				.AddComponent<SnowBall>();
			snowPre.name = "Snowball";
			snowBallRenderer.name = "SnowBallSprite";

			snowPre.entity = snowPre.gameObject.CreateEntity(1f, 1f, snowBallRenderer.transform);
			snowPre.audMan = snowPre.gameObject.CreatePropagatedAudioManager(35f, 55f);
			snowPre.audHit = BBTimesManager.man.Get<SoundObject>("audGenericSnowHit");

			snowPre.renderer = snowBallRenderer.transform;
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
			animComp.Initialize(ec);
			behaviorStateMachine.ChangeState(new Winterry_Wander(this));
		}

		public void Walk(bool walk)
		{
			float speed = walk ? 12f : 0f;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}

		public void Shoot(Transform target)
		{
			if (shootAwaitCor != null)
				StopCoroutine(shootAwaitCor);
			shootAwaitCor = StartCoroutine(ShootAwait(target));
		}

		IEnumerator ShootAwait(Transform target)
		{
			animComp.speed = 12.5f;
			behaviorStateMachine.ChangeState(new Winterry_PrepareShoot(this));

			audMan.FlushQueue(true);
			audMan.QueueAudio(audBlow);

			animComp.animation = blowAnim;
			animComp.ResetFrame(true);
			animComp.StopLastFrameMode();

			while (!animComp.Paused || audMan.QueuedAudioIsPlaying)
				yield return null;

			audMan.QueueAudio(audSpit);
			animComp.animation = spitAnim;
			animComp.ResetFrame(true);
			animComp.StopLastFrameMode();
			if (target)
				Instantiate(snowPre).Spawn(gameObject, transform.position, (target.position - transform.position).normalized, Random.Range(minShootForce, maxShootForce), shootYVelocity, ec);

			while (!animComp.Paused || audMan.QueuedAudioIsPlaying)
				yield return null;

			behaviorStateMachine.ChangeState(new Winterry_DelayForNextTarget(this));
			animComp.animation = walkAnim;
			animComp.ResetFrame(true);
			animComp.speed = 8f;
		}

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audSpit, audBlow;

		[SerializeField]
		internal SnowBall snowPre;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal Sprite[] walkAnim, spitAnim, blowAnim;

		[SerializeField]
		internal float minShootForce = 25f, maxShootForce = 35f, shootYVelocity = 6f, waitNextSpitCooldown = 1.15f;

		Coroutine shootAwaitCor;
	}

	internal class Winterry_StateBase(Winterry w) : NpcState(w)
	{
		protected Winterry w = w;
	}

	internal class Winterry_Wander(Winterry w) : Winterry_StateBase(w)
	{
		public override void Enter()
		{
			base.Enter();
			w.Walk(true);
			ChangeNavigationState(new NavigationState_WanderRandom(w, 0));
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			w.Shoot(player.transform);
		}

		public override void Update()
		{
			base.Update(); 

			if (!w.Blinded)
			{
				for (int i = 0; i < w.ec.Npcs.Count; i++)
				{
					if (w != w.ec.Npcs[i] && w.ec.Npcs[i].Navigator.isActiveAndEnabled)
					{
						w.looker.Raycast(w.ec.Npcs[i].transform, Mathf.Min(w.looker.distance, w.ec.MaxRaycast), out bool flag);
						if (flag)
						{
							w.Shoot(w.ec.Npcs[i].transform);
							return;
						}
					}
				}
			}
		}
	}

	internal class Winterry_PrepareShoot(Winterry w) : Winterry_StateBase(w)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(w, 0));
			w.Walk(false);
		}
	}

	internal class Winterry_DelayForNextTarget(Winterry w) : Winterry_StateBase(w)
	{
		float cooldown = w.waitNextSpitCooldown;
		public override void Enter()
		{
			base.Enter();
			w.Walk(true);
			ChangeNavigationState(new NavigationState_WanderRandom(w, 0));
		}

		public override void Update()
		{
			base.Update();
			cooldown -= w.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				w.behaviorStateMachine.ChangeState(new Winterry_Wander(w));
		}
	}
}
