using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using UnityEngine.UI;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.NPCs
{
    public class Mugh : NPC, IItemAcceptor, INPCPrefab
	{
		public void SetupPrefab()
		{

			SoundObject[] soundObjects = [
			this.GetSound("Mugh_LetsHug.wav", "Vfx_Mugh_Hug1", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_LetsBeFriends.wav", "Vfx_Mugh_Hug2", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_LoveYou.wav", "Vfx_Mugh_Hug3", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_WhyAreYouRunning.wav", "Vfx_Mugh_Left1", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_DontLeaveMe.wav", "Vfx_Mugh_Left2", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_WhatDidYouDo.wav", "Vfx_Mugh_Die1", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_Why.wav", "Vfx_Mugh_Die2", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("Mugh_Revived.wav", "Vfx_Mugh_Revive", SoundType.Voice, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("mugh_noises.wav", "Vfx_Mugh_Noise", SoundType.Effect, new(0.3671875f, 0.1640625f, 0f)),
			this.GetSound("mugh_die.wav", "Vfx_Mugh_Noise", SoundType.Effect, new(0.3671875f, 0.1640625f, 0f)),
			];

			audMan = GetComponent<PropagatedAudioManager>();
			walkAudMan = gameObject.CreatePropagatedAudioManager(65, 75);
			renderer = spriteRenderer[0];
			var storedSprites = this.GetSpriteSheet(5, 1, 55f, "muggy.png");
			normSprite = storedSprites[0];
			hugSprite = storedSprites[1];
			sadSprite = storedSprites[2];
			holeSprite = storedSprites[3];
			deadSprite = storedSprites[4];
			renderer.sprite = storedSprites[0];

			audFindPlayer = [.. soundObjects.Take(3)];
			audLostPlayer = [.. soundObjects.Skip(3).Take(2)];
			audGetHit = [.. soundObjects.Skip(5).Take(2)];
			audDie = soundObjects[9];
			audWalk = soundObjects[8];
			audRevive = soundObjects[7];

			var can = ObjectCreationExtensions.CreateCanvas();
			can.gameObject.ConvertToPrefab(false);
			can.gameObject.SetActive(false);
			can.transform.SetParent(transform);

			var img = ObjectCreationExtensions.CreateImage(can, this.GetSprite(1, "mud_screen.png"));

			mudCanvas = can;
			mudImage = img;
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

		public void HugPlayer(PlayerManager pm)
		{
			mudCanvas.gameObject.SetActive(true);
			mudCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
		}

		public void DisablePlayerHug() => mudCanvas.gameObject.SetActive(false);


		public Image MudImg => mudImage;

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

		[SerializeField]
		internal Canvas mudCanvas;

		[SerializeField]
		internal Image mudImage;

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

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (cooldown <= 0f && other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
					mu.behaviorStateMachine.ChangeState(new Mugh_HugPlayer(mu, e));
			}
		}
	}

	internal class Mugh_HugPlayer(Mugh mu, Entity pm) : Mugh_StateBase(mu)
	{
		readonly Entity pm = pm;
		readonly MovementModifier hugMod = new(Vector3.zero, 0.72f);
		float hugTolerance = 14f, hugCooldown = 12f;
		const float minHugTolerance = 3f;

		public override void Enter()
		{
			base.Enter();
			mu.Navigator.maxSpeed = 0;
			mu.Navigator.SetSpeed(0);
			mu.HugState();
			mu.SeeYouNoise();
			this.pm.ExternalActivity.moveMods.Add(hugMod);
			var pm = this.pm.GetComponent<PlayerManager>();
			if (pm)
				mu.HugPlayer(pm);
		}

		public override void Update()
		{
			base.Update();
			hugCooldown -= mu.TimeScale * Time.deltaTime;
			if (!pm || hugCooldown < 0f)
			{
				mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu, 5f));
				return;
			}
			var dist = mu.transform.position - pm.transform.position;
			hugMod.movementAddend = dist * 135f * Time.deltaTime * mu.TimeScale;

			if (dist.magnitude >= hugTolerance)
				mu.behaviorStateMachine.ChangeState(new Mugh_Wandering(mu, 30f, true));

			hugTolerance -= mu.TimeScale * Time.deltaTime * dist.magnitude * 0.5f;
			if (hugTolerance < minHugTolerance)
				hugTolerance = minHugTolerance;

			var color = mu.MudImg.color;
			color.a = 1f / (dist.magnitude * 0.45f);
			mu.MudImg.color = color;
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
			pm?.ExternalActivity.moveMods.Remove(hugMod);
			mu.DisablePlayerHug();
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
