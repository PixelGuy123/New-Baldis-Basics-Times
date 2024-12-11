using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Quiker : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			audMan = GetComponent<AudioManager>();

			audFlying = this.GetSound("Quiker_Sound.wav", "Vfx_Quiker_Noises", SoundType.Effect, new(0.1f, 0.1f, 0.1f));
			audBlind = this.GetSoundNoSub("Quiker_Caught.wav", SoundType.Effect);

			flyingAudMan = gameObject.CreatePropagatedAudioManager(175f, 255f);

			anim = gameObject.AddComponent<AnimationComponent>();
			anim.renderers = [spriteRenderer[0]];
			anim.speed = 12;
			anim.animation = this.GetSpriteSheet(2, 2, 25f, "quirker.png");
			renderer = spriteRenderer[0].transform;
			spriteRenderer[0].sprite = anim.animation[0];



			var system = new GameObject("QuikerParticles").AddComponent<ParticleSystem>();
			system.transform.SetParent(transform);
			system.transform.localPosition = Vector3.zero;
			system.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = this.GetTexture("shadw.png") };

			var main = system.main;
			main.gravityModifierMultiplier = 0f;
			main.startLifetimeMultiplier = 0.6f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(1f, 2f);

			var emission = system.emission;
			emission.rateOverTimeMultiplier = 16f;

			var vel = system.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.World;
			vel.x = new(-8f, 8f);
			vel.y = new(-8f, 8f);
			vel.z = new(-8f, 8f);

			var an = system.textureSheetAnimation;
			an.enabled = true;
			an.numTilesX = 2;
			an.numTilesY = 2;
			an.animation = ParticleSystemAnimationType.WholeSheet;
			an.fps = 13f;
			an.timeMode = ParticleSystemAnimationTimeMode.FPS;
			an.cycleCount = 1;

			parts = system;
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
		internal AudioManager audMan, flyingAudMan;

		[SerializeField]
		internal SoundObject audFlying, audBlind;

		[SerializeField]
		internal AnimationComponent anim;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal ParticleSystem parts;

		public override void Initialize()
		{
			base.Initialize();
			anim.Initialize(ec);
			rendererPos = renderer.localPosition;
			behaviorStateMachine.ChangeState(new Quiker_Active(this));
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (!active)
			{
				if (renderer.localPosition.y > -10f)
					renderer.localPosition += Vector3.down * Time.deltaTime * TimeScale * 16f;
				return;
			}

			if (offset.y < 0f)
			{
				offset += Vector3.up * 16f;
				if (offset.y > 0f)
					offset = Vector3.zero;
			}

			renderer.localPosition = offset + rendererPos + Vector3.up * Mathf.Cos(Time.fixedTime * TimeScale * 2.5f) * 0.65f;
		}

		public void SetFloatingActive(bool active)
		{
			this.active = active;

			if (!active)
			{
				flyingAudMan.FlushQueue(true);
				offset = Vector3.zero;
			}

			for (int i = 0; i < baseTrigger.Length; i++)
				baseTrigger[i].enabled = active;

			var main = parts.emission;
			main.enabled = active;
		}

		public void Noises()
		{
			flyingAudMan.FlushQueue(true);
			flyingAudMan.maintainLoop = true;
			flyingAudMan.SetLoop(true);
			flyingAudMan.QueueAudio(audFlying);
		}

		public override void Despawn()
		{
			base.Despawn();
			foreach (var npc in affectedNpcs)
			{
				if (npc.Key)
				{
					npc.Key.GetNPCContainer().RemoveLookerMod(npc.Value);
					npc.Key.Navigator.Am.moveMods.Remove(moveMod);
				}
			}

			for (int i = 0; i < affectedPlayers.Count; i++)
			{
				if (affectedPlayers[i].Key.Key)
				{
					if (affectedPlayers[i].Key.Value == this)
					{
						affectedPlayers[i].Key.Key.Am.moveMods.Remove(affectedPlayers[i].Value.Key);
						ec.RemoveFog(affectedPlayers[i].Value.Value.Key);
						affectedPlayers.RemoveAt(i--);
					}
				}
				else
					affectedPlayers.RemoveAt(i--);
			}
		}

		public void BlindNPC(NPC npc)
		{
			audMan.PlaySingle(audBlind);
			StartCoroutine(AffectNPC(npc));
		}
		public void BlindPlayer(PlayerManager player)
		{
			audMan.PlaySingle(audBlind);
			affectedPlayers.Add(new(new(player, this), new(moveMod, new(fog, StartCoroutine(AffectPlayer(player))))));
		}
		

		IEnumerator AffectNPC(NPC npc)
		{
			var cont = npc.GetNPCContainer();
			var att = new ValueModifier(0);
			cont.AddLookerMod(att);
			npc.Navigator.Am.moveMods.Add(moveMod);
			affectedNpcs.Add(npc, att);

			float delay = 15f;
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			npc.Navigator.Am.moveMods.Remove(moveMod);
			affectedNpcs.Remove(npc);
			cont.RemoveLookerMod(att);

			yield break;
		}

		IEnumerator AffectPlayer(PlayerManager pm)
		{
			pm.Am.moveMods.Add(moveMod);
			ec.AddFog(fog);

			float delay = 15f;
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			pm.Am.moveMods.Remove(moveMod);
			affectedPlayers.RemoveAll(x => x.Key.Key == pm);
			ec.RemoveFog(fog);

			yield break;
		}

		Vector3 rendererPos, offset;
		readonly Dictionary<NPC, ValueModifier> affectedNpcs = [];
		internal static readonly List<KeyValuePair<KeyValuePair<PlayerManager, Quiker>, KeyValuePair<MovementModifier, KeyValuePair<Fog, Coroutine>>>> affectedPlayers = []; // yes
		bool active = true;

		public bool AffectedNPC(NPC npc) => affectedNpcs.ContainsKey(npc);

		readonly MovementModifier moveMod = new(Vector3.zero, 0.45f);
		readonly Fog fog = new() { color = Color.black, maxDist = 1000f, startDist = 0, strength = 45f };
	}

	internal class Quiker_StateBase(Quiker qu) : NpcState(qu)
	{
		protected Quiker qu = qu;
	}

	internal class Quiker_Active(Quiker qu) : Quiker_StateBase(qu)
	{
		float activeCooldown = Random.Range(45f, 60f);
		NavigationState_WanderRandom nav;
		public override void Enter()
		{
			base.Enter();
			qu.Noises();
			qu.SetFloatingActive(true);
			qu.Navigator.maxSpeed = 45f;
			qu.Navigator.SetSpeed(45f);
			nav = new NavigationState_WanderRandom(qu, 64);
			ChangeNavigationState(nav);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.isTrigger)
			{
				if (other.CompareTag("Player"))
				{
					var pm = other.GetComponent<PlayerManager>();
					if (pm && Quiker.affectedPlayers.FindIndex(x => x.Key.Key == pm) == -1)
					{
						pm.plm.Entity.AddForce(new(Quaternion.AngleAxis(Random.Range(-0.7854f, 0.7854f), Vector3.up) * (qu.Navigator.NextPoint - qu.transform.position).normalized, 
							qu.Navigator.Speed, -qu.Navigator.Speed * 0.85f)); // 0.7854 radians = 45° degrees
						qu.BlindPlayer(pm);
					}
					return;
				}
				if (other.CompareTag("NPC"))
				{
					var npc = other.GetComponent<NPC>();
					if (npc && !qu.AffectedNPC(npc))
					{
						npc.Navigator.Entity.AddForce(new(Quaternion.AngleAxis(Random.Range(-0.7854f, 0.7854f), Vector3.up) * (qu.Navigator.NextPoint - qu.transform.position).normalized,
							qu.Navigator.Speed, -qu.Navigator.Speed * 0.85f)); // 0.7854 radians = 45° degrees
						qu.BlindNPC(npc);
					}
				}
			}
		}

		public override void Update()
		{
			base.Update();
			activeCooldown -= qu.TimeScale * Time.deltaTime;
			if (activeCooldown <= 0f)
				qu.behaviorStateMachine.ChangeState(new Quiker_HideBelow(qu));
		}

		public override void Exit()
		{
			base.Exit();
			nav.priority = 0;
		}
	}

	internal class Quiker_HideBelow(Quiker qu) : Quiker_StateBase(qu)
	{
		float waitCooldown = Random.Range(45f, 70f);

		public override void Enter()
		{
			base.Enter();
			qu.SetFloatingActive(false);
			qu.Navigator.SetSpeed(0f);
			qu.Navigator.maxSpeed = 0f;
			ChangeNavigationState(new NavigationState_DoNothing(qu, 0));
		}

		public override void Update()
		{
			base.Update();
			waitCooldown -= qu.TimeScale * Time.deltaTime;
			if (waitCooldown <= 0f)
			{
				var cells = qu.ec.mainHall.GetTilesOfShape(TileShapeMask.Corner | TileShapeMask.End, true);
				qu.Navigator.Entity.Teleport(cells[Random.Range(0, cells.Count)].FloorWorldPosition);
				qu.behaviorStateMachine.ChangeState(new Quiker_Active(qu));
			}
		}
	}
}
