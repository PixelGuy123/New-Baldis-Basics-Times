using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using System.Linq;
using UnityEngine;
namespace BBTimes.CustomContent.NPCs
{
	public class PranTheDancer : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<PropagatedAudioManager>();
			musicAudMan = gameObject.CreatePropagatedAudioManager(162, 210);
			audGrab = BBTimesManager.man.Get<SoundObject>("audGenericGrab");
			audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");
			audLetGo = this.GetSound("Pran_LetGo.wav", "Vfx_Pran_LetGo", SoundType.Voice, new(1f, 0.6f, 0.2f));
			audMusic = this.GetSound("Pran_IdleDance.wav", "Vfx_Pran_CoolestMusic", SoundType.Voice, new(1f, 0.6f, 0.2f));
			audSpinningMusic = this.GetSound("Pran_SpinDance.wav", "Vfx_Pran_CoolestMusic", SoundType.Voice, new(1f, 0.6f, 0.2f));
			audIdle = [
				this.GetSound("Pran_idle1.wav", "Vfx_Pran_Idle1", SoundType.Voice, new(1f, 0.6f, 0.2f)),
				this.GetSound("Pran_idle2.wav", "Vfx_Pran_Idle2", SoundType.Voice, new(1f, 0.6f, 0.2f))
				];
			audMeGrab = [
				this.GetSound("Pran_Grab1.wav", "Vfx_Pran_Grab1", SoundType.Voice, new(1f, 0.6f, 0.2f)),
				this.GetSound("Pran_Grab2.wav", "Vfx_Pran_Grab2", SoundType.Voice, new(1f, 0.6f, 0.2f))
				];
			musicAudMan.AddStartingAudiosToAudioManager(true, audMusic);
			var sprites = this.GetSpriteSheet(4, 4, 25f, "pran.png");
			spriteRenderer[0].sprite = sprites[0];
			dancing1 = [.. sprites.Take(8)];
			dancing2 = [.. sprites.Skip(8).Take(8)];
			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.renderers = [spriteRenderer[0]];
			animComp.speed = 16f;
			animComp.animation = dancing1;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }
		// --------------------------------------------------
		[SerializeField]
		internal AnimationComponent animComp;
		[SerializeField]
		internal Sprite[] dancing1, dancing2;
		[SerializeField]
		internal PropagatedAudioManager audMan, musicAudMan;
		[SerializeField]
		internal SoundObject audMusic, audSpinningMusic, audLetGo, audGrab, audThrow;
		[SerializeField]
		internal SoundObject[] audIdle, audMeGrab;
		[SerializeField]
		internal float idleChance = 0.6f;
		public void PlayIdleMusic()
		{
			musicAudMan.FlushQueue(true);
			musicAudMan.maintainLoop = true;
			musicAudMan.SetLoop(true);
			musicAudMan.QueueAudio(audMusic);
		}
		public void PlaySpinningMusic()
		{
			musicAudMan.FlushQueue(true);
			musicAudMan.QueueAudio(audSpinningMusic);
		}
		public void PlayIdle() =>
			audMan.QueueRandomAudio(audIdle);
		public void PlayGrab()
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(audGrab);
			audMan.PlayRandomAudio(audMeGrab);
		}
		public void ThrowEntity(Entity e, Vector3 dir)
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(audThrow);
			audMan.PlaySingle(audLetGo);
			e.AddForce(new(dir, 125f, -67f));
		}
		public void SpinningDance() =>
			animComp.animation = dancing2;
		public void DanceForward() =>
			animComp.animation = dancing1;
		public override void Initialize()
		{
			base.Initialize();
			animComp.Initialize(ec);
			behaviorStateMachine.ChangeState(new Pran_Wondering(this));
		}
		public void NormalSpeed()
		{
			navigator.maxSpeed = 14f;
			navigator.SetSpeed(navigator.maxSpeed);
		}
	}
	internal class Pran_StateBase(PranTheDancer danc) : NpcState(danc)
	{
		protected PranTheDancer pran = danc;
	}
	internal class Pran_Wondering(PranTheDancer pran, float cooldown = 0f) : Pran_StateBase(pran)
	{
		float idleCooldown = 5f, cooldown = cooldown;
		public override void Enter()
		{
			base.Enter();
			pran.DanceForward();
			pran.NormalSpeed();
			pran.PlayIdleMusic();
			ChangeNavigationState(new NavigationState_WanderRandom(pran, 0));
		}
		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (cooldown <= 0f && other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
				pran.behaviorStateMachine.ChangeState(new Pran_DanceWithMe(pran, other.GetComponent<Entity>()));
		}
		public override void Update()
		{
			base.Update();
			idleCooldown -= pran.TimeScale * Time.deltaTime;
			if (idleCooldown < 0f)
			{
				idleCooldown += 9f;
				if (Random.value <= pran.idleChance)
					pran.PlayIdle();
			}
			if (cooldown > 0f)
				cooldown -= pran.TimeScale * Time.deltaTime;
		}
	}
	internal class Pran_DanceWithMe(PranTheDancer pran, Entity target) : Pran_StateBase(pran)
	{
		readonly Entity target = target;
		Vector3 throwDir, rotatingReference;
		readonly MovementModifier stayMod = new(Vector3.zero, 0.5f);
		public override void Enter()
		{
			base.Enter();
			pran.Navigator.maxSpeed = 0;
			pran.Navigator.SetSpeed(0f);
			pran.PlayGrab();
			pran.PlaySpinningMusic();
			pran.SpinningDance();
			throwDir = (pran.Navigator.NextPoint - pran.transform.position).normalized;
			rotatingReference = pran.transform.forward;
			target.ExternalActivity.moveMods.Add(stayMod);
		}
		public override void Update()
		{
			base.Update();
			if (!target || (!pran.musicAudMan.QueuedAudioIsPlaying && !pran.audMan.AnyAudioIsPlaying))
			{
				if (target)
				{
					pran.ThrowEntity(target, throwDir);
					target.ExternalActivity.moveMods.Remove(stayMod);
				}
				pran.behaviorStateMachine.ChangeState(new Pran_Wondering(pran, 15f));
				return;
			}
			rotatingReference = rotatingReference.RotateAroundAxis(Vector3.up, Time.deltaTime * pran.TimeScale * 254f);
			var dist = rotatingReference * 5f + pran.transform.position - target.transform.position;
			stayMod.movementAddend = dist * 215f * Time.deltaTime * pran.TimeScale;
			if (dist.magnitude > 100)
			{
				target?.ExternalActivity.moveMods.Remove(stayMod);
				pran.behaviorStateMachine.ChangeState(new Pran_Wondering(pran));
			}
		}
	}
}