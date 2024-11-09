using BBTimes.CustomComponents;
using BBTimes.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class VacuumCleaner : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			spriteRenderer[0].sprite = this.GetSprite(16.5f, "cleaner.png");
			audMan = GetComponent<PropagatedAudioManager>();
			audStartSweep = this.GetSound("cleaner_start.wav", "Vfx_VacClean_Clean", SoundType.Voice, new(0.85f, 0.85f, 0.85f));
			audSweepLoop = this.GetSound("cleaner_loop.wav", "Vfx_VacClean_Clean", SoundType.Voice, new(0.85f, 0.85f, 0.85f));
			audEndSweep = this.GetSound("cleaner_end.wav", "Vfx_VacClean_Clean", SoundType.Voice, new(0.85f, 0.85f, 0.85f));
		}

		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// stuff above^^

		public override void Initialize()
		{
			base.Initialize();
			rendererOffset = spriteRenderer[0].transform.localPosition;
			home = ec.CellFromPosition(transform.position);
			behaviorStateMachine.ChangeState(new VacuumCleaner_Wait(this));
		}

		internal void StartSweeping()
		{
			audMan.FlushQueue(true);
			
			audMan.QueueAudio(audStartSweep);
			audMan.QueueAudio(audSweepLoop);
			audMan.maintainLoop = true;
			audMan.SetLoop(true);

			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);

			sweeping = true;

		}

		internal void StopSweeping()
		{
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			if (sweeping)
			{
				audMan.FlushQueue(true);
				audMan.QueueAudio(audEndSweep);
				CleanOutMods();
			}

			sweeping = false;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (sweeping)
			{
				foreach (NPC npc in ec.Npcs)
				{
					if (npc != this && npc.Navigator.enabled)
					{
						looker.Raycast(npc.transform, Mathf.Min(
					(transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude,
					looker.distance,
					npc.ec.MaxRaycast
						), out bool flag);
						if (!actMods.ContainsKey(npc.Navigator.Am)) 
						{
							if (flag)
								SuckInMod(npc.Navigator.Am);
						}
						else if (!flag)
						{
							npc.Navigator.Am.moveMods.Remove(actMods[npc.Navigator.Am]);
							actMods.Remove(npc.Navigator.Am);
						}
					}
				}

				UpdateMods();
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (sweeping && !actMods.ContainsKey(player.Am))
				SuckInMod(player.Am);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			if (sweeping && actMods.ContainsKey(player.Am))
			{
				player.Am.moveMods.Remove(actMods[player.Am]);
				actMods.Remove(player.Am);
			}	
		}

		void SuckInMod(ActivityModifier target)
		{
			var moveMod = new MovementModifier(Vector3.zero, 0.95f);
			target.moveMods.Add(moveMod);
			actMods.Add(target, moveMod);
		}

		void UpdateMods()
		{
			foreach (var am in actMods)
			{
				if (am.Key)
				{
					if (cloggedUp)
					{
						am.Value.movementAddend = Vector3.zero;
						continue;
					}

					Vector3 offset = transform.position - am.Key.transform.position;
					am.Value.movementAddend += offset.normalized * Mathf.Max(0f, (0.2f * forceLimit) - (offset.magnitude * 0.5f));
					am.Value.movementAddend.Limit(forceLimit, forceLimit, forceLimit);
				}
			}
		}

		void CleanOutMods()
		{
			foreach (var am in actMods)
				am.Key?.moveMods.Remove(am.Value);
			actMods.Clear();
		}

		public override void Despawn()
		{
			base.Despawn();
			CleanOutMods();
		}

		public override void VirtualOnTriggerEnter(Collider other)
		{
			base.VirtualOnTriggerEnter(other);
			if (cloggedUp || !sweeping) return;

			if (other.isTrigger)
			{
				var am = other.GetComponent<Entity>();
				if (am && actMods.ContainsKey(am.ExternalActivity) && Random.value <= cloggingUpChance)
					StartCoroutine(CloggingUpSequence(am));
				
			}
		}

		IEnumerator CloggingUpSequence(Entity target)
		{
			cloggedUp = true;
			var moveMod = actMods[target.ExternalActivity];
			
			moveMod.movementMultiplier = 0f;
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			float timer = clogUpCooldown;
			while (timer > 0f)
			{
				if (Time.timeScale != 0)
					spriteRenderer[0].transform.localPosition = Random.insideUnitSphere * 2f;
				timer -= TimeScale * Time.deltaTime;
				yield return null;
			}
			spriteRenderer[0].transform.localPosition = rendererOffset;
			actMods.Remove(target.ExternalActivity);
			target.AddForce(new(Random.insideUnitSphere, clogEndForce, -clogEndForce * 0.35f));
			target.ExternalActivity.moveMods.Remove(moveMod);
			
			navigator.SetSpeed(speed);
			navigator.maxSpeed = speed;
			cloggedUp = false;
		}

		internal bool IsHome => home == ec.CellFromPosition(transform.position);
		internal float ActiveCooldown => Random.Range(minActive, maxActive);
		internal float WaitCooldown => Random.Range(minWait, maxWait);
		internal Cell home;
		Vector3 rendererOffset;

		[SerializeField]
		internal SoundObject audSweepLoop, audStartSweep, audEndSweep;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal float minActive = 25f, maxActive = 40f, minWait = 35f, maxWait = 60f, speed = 40f, forceLimit = 55f, clogUpCooldown = 3.5f, clogEndForce = 90f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float cloggingUpChance = 0.06f;

		bool sweeping = false, cloggedUp = false;
		readonly Dictionary<ActivityModifier, MovementModifier> actMods = [];
	}

	internal class VacuumCleaner_StateBase(VacuumCleaner cle) : NpcState(cle)
	{
		protected VacuumCleaner cle = cle;
	}

	internal class VacuumCleaner_Wait(VacuumCleaner cle) : VacuumCleaner_StateBase(cle)
	{
		float waitCooldown = cle.WaitCooldown;
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(cle, 0));
			cle.StopSweeping();
		}
		public override void Update()
		{
			base.Update();
			waitCooldown -= cle.TimeScale * Time.deltaTime;
			if (waitCooldown <= 0f)
				cle.behaviorStateMachine.ChangeState(new VacuumCleaner_Start(cle));
		}
	}

	internal class VacuumCleaner_Start(VacuumCleaner cle) : VacuumCleaner_StateBase(cle)
	{
		float activeCooldown = cle.ActiveCooldown;
		public override void Enter()
		{
			base.Enter();
			cle.StartSweeping();
			ChangeNavigationState(new NavigationState_WanderRandom(cle, 0));

		}
		public override void Update()
		{
			base.Update();
			activeCooldown -= cle.TimeScale * Time.deltaTime;
			if (activeCooldown <= 0f)
				cle.behaviorStateMachine.ChangeState(new VacuumCleaner_GoBack(cle));
		}
	}

	internal class VacuumCleaner_GoBack(VacuumCleaner cle) : VacuumCleaner_StateBase(cle)
	{
		NavigationState_TargetPosition tar;
		public override void Enter()
		{
			base.Enter();
			tar = new(cle, 0, cle.home.FloorWorldPosition);
			ChangeNavigationState(tar);
		}
		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			if (cle.IsHome)
			{
				cle.behaviorStateMachine.ChangeState(new VacuumCleaner_Wait(cle));
				return;
			}
			ChangeNavigationState(tar);
		}
	}
}
