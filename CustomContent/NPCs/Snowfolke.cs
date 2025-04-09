using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Snowfolke : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(30f, "snowFlake.png");
			audMan = GetComponent<PropagatedAudioManager>();
			audThrow = this.GetSound("snw_throwsfx.wav", "Vfx_Snowfolke_ThrowSfx", SoundType.Voice, audMan.subtitleColor);
			audThrowVoice = this.GetSound("snw_throw.wav", "Vfx_Snowfolke_Throw", SoundType.Voice, audMan.subtitleColor);
			audIdle = [
				this.GetSound("snw_idle1.wav", "Vfx_Snowfolke_Idle1", SoundType.Voice, audMan.subtitleColor),
				this.GetSound("snw_idle2.wav", "Vfx_Snowfolke_Idle2", SoundType.Voice, audMan.subtitleColor)
				];
			audBefThrow = [
				this.GetSound("snw_beforethrow1.wav", "Vfx_Snowfolke_BeforeThrow1", SoundType.Voice, audMan.subtitleColor),
				this.GetSound("snw_beforethrow2.wav", "Vfx_Snowfolke_BeforeThrow2", SoundType.Voice, audMan.subtitleColor)
				];

			floatingRenderer = spriteRenderer[0].transform;

			snowPre = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(37f, "snowflakethingy.png"))
				.AddSpriteHolder(out var snowBallRenderer, 0f, LayerStorage.standardEntities)
				.gameObject.SetAsPrefab(true)
				.AddComponent<Snowflake>();
			snowPre.name = "Snowflake";
			snowBallRenderer.name = "SnowFlakeSprite";

			snowPre.entity = snowPre.gameObject.CreateEntity(1f, 1f, snowBallRenderer.transform);
			snowPre.entity.SetGrounded(false);
			snowPre.audMan = snowPre.gameObject.CreatePropagatedAudioManager(35f, 55f);

			snowPre.audHit = this.GetSound("snw_freeze.wav", "Vfx_Snowfolke_Hit", SoundType.Effect, audMan.subtitleColor);

			snowPre.renderer = snowBallRenderer.gameObject;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string Category => "npcs";
		
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();
			rendererPos = floatingRenderer.transform.localPosition;
			behaviorStateMachine.ChangeState(new Snowfolke_Wander(this));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			floatingRenderer.transform.localPosition = rendererPos + rendererOffset + (Vector3.up * Mathf.Cos(Time.fixedTime * TimeScale * 0.75f * navigator.speed) * 0.88f);
		}

		public void Walk(bool walk)
		{
			float speed = walk ? 15f : 0f;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}

		public void PrepareToShoot()
		{
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audBefThrow);
		}

		public void Idle()
		{
			if (!audMan.QueuedAudioIsPlaying && Random.value <= idleChance)
				audMan.PlayRandomAudio(audIdle);
		}
		

		public void ShootAllDirs()
		{
			if (shootAwaitCor != null)
				StopCoroutine(shootAwaitCor);
			shootAwaitCor = StartCoroutine(ShootAwait());
		}

		IEnumerator ShootAwait()
		{
			float delay = delayBeforeHit, force = 0f, adder = 1f / delay;

			while (delay > 0f)
			{
				delay -= TimeScale * Time.deltaTime;

				force += adder * TimeScale * Time.deltaTime;
				if (force > maxShakeForce)
					force = maxShakeForce;

				rendererOffset = new(Random.Range(-force, force) * Time.timeScale, Random.Range(-force, force) * Time.timeScale, Random.Range(-force, force) * Time.timeScale);


				yield return null;
			}

			rendererOffset = Vector3.zero;
			Vector3 dir = Vector3.forward;
			float rotOffset = 360f / dirsToThrow;
			audMan.PlaySingle(audThrow);
			audMan.PlaySingle(audThrowVoice);

			for (int i = 0; i < dirsToThrow; i++)
			{
				Instantiate(snowPre).Spawn(gameObject, transform.position, dir, throwSpeed, ec);
				dir = dir.RotateAroundAxis(Vector3.up, rotOffset);
			}

			behaviorStateMachine.ChangeState(new Snowfolke_Wander(this));
		}

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrowVoice, audThrow;

		[SerializeField]
		internal SoundObject[] audBefThrow, audIdle;

		[SerializeField]
		internal Transform floatingRenderer;

		[SerializeField]
		internal Snowflake snowPre;

		[SerializeField]
		internal float throwSpeed = 30f, minCooldownPerThrow = 20f, maxCooldownPerThrow = 45f, delayBeforeHit = 5f, maxShakeForce = 1.5f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float idleChance = 0.0025f;

		[SerializeField]
		internal int dirsToThrow = 8;

		public float CooldownForThrow => Random.Range(minCooldownPerThrow, maxCooldownPerThrow);

		public bool AmIOnAGoodSpotToShoot
		{
			get
			{
				var cell = ec.CellFromPosition(transform.position);
				return cell.shape == TileShapeMask.Corner || cell.shape == TileShapeMask.Single || cell.open;
			}
		}

		Coroutine shootAwaitCor;
		Vector3 rendererPos, rendererOffset = Vector3.zero;
	}

	internal class Snowfolke_StateBase(Snowfolke w) : NpcState(w)
	{
		protected Snowfolke w = w;
	}

	internal class Snowfolke_Wander(Snowfolke w) : Snowfolke_StateBase(w)
	{
		float cooldown = w.CooldownForThrow;
		public override void Enter()
		{
			base.Enter();
			w.Walk(true);
			ChangeNavigationState(new NavigationState_WanderRandom(w, 0));
		}

		public override void Update()
		{
			base.Update();

			w.Idle();
			if (cooldown > 0f)
				cooldown -= w.TimeScale * Time.deltaTime;
			else if (w.AmIOnAGoodSpotToShoot)
				w.behaviorStateMachine.ChangeState(new Snowfolke_PrepareShoot(w));
		}
	}

	internal class Snowfolke_PrepareShoot(Snowfolke w) : Snowfolke_StateBase(w)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(w, 0));
			w.PrepareToShoot();
			w.Walk(false);
			w.ShootAllDirs();
		}
	}
}
