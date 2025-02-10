using BBTimes.CustomComponents;
using BBTimes.CustomComponents.NpcSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
    public class Watcher : NPC, INPCPrefab
	{
		public void SetupPrefab()
		{
			SoundObject[] soundObjects = [this.GetSound("WCH_ambience.wav", "Vfx_Wch_Idle", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f)),
		this.GetSoundNoSub("WCH_see.wav", SoundType.Effect),
		this.GetSound("WCH_angered.wav", "Vfx_Wch_Angry", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f)),
		this.GetSound("WCH_teleport.wav", "Vfx_Wch_Teleport", SoundType.Effect, new Color(0.8f, 0.8f, 0.8f)),
		this.GetSound("SHDWCH_spawn.wav", "Vfx_Wch_Spawn", SoundType.Effect, new Color(0.6f, 0.6f, 0.6f)),
		this.GetSound("SHDWCH_ambience.wav", "Vfx_Wch_Idle", SoundType.Effect, new Color(0.6f, 0.6f, 0.6f))
			];
			var storedSprites = this.GetSpriteSheet(2, 1, 35f, "watcher.png");
			spriteRenderer[0].sprite = storedSprites[0];

			audMan = GetComponent<PropagatedAudioManager>();

			audAmbience = soundObjects[0];
			audSpot = soundObjects[1];
			audAngry = soundObjects[2];
			audTeleport = soundObjects[3];

			spriteToHide = spriteRenderer[0];
			screenAudMan = gameObject.CreateAudioManager(45f, 75f).MakeAudioManagerNonPositional();

			var hallRender = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1]);
			hallRender.gameObject.layer = LayerMask.NameToLayer("Overlay");
			hallRender.name = "WatcherHallucination";
			hallRender.gameObject.ConvertToPrefab(true);

			var hall = hallRender.gameObject.AddComponent<Hallucinations>();
			hall.audMan = hall.gameObject.CreateAudioManager(15f, 25f);
			hall.audSpawn = soundObjects[4];
			hall.audLoop = soundObjects[5];
			hall.renderer = hallRender;

			hallPre = hall;
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
			audMan.maintainLoop = true;
			screenAudMan.maintainLoop = true;

			behaviorStateMachine.ChangeState(new Watcher_WaitBelow(this));
		}

		public void DespawnHallucinations(bool instaDespawn)
		{
			while (hallucinations.Count != 0)
			{
				if (instaDespawn)
					hallucinations[0]?.Despawn();
				else
					hallucinations[0]?.SetToDespawn();

				hallucinations.RemoveAt(0);
			}
		}

		public override void Despawn()
		{
			base.Despawn();
			DespawnHallucinations(true);
		}

		public void GetAngry()
		{
			navigator.maxSpeed = 500f;
			navigator.SetSpeed(0f);
			navigator.accel = 95f;
		}

		public void Hide(bool hide)
		{
			spriteToHide.enabled = !hide;
			for (int i = 0; i < baseTrigger.Length; i++)
				baseTrigger[i].enabled = !hide;

			if (hide)
			{
				audMan.FlushQueue(true);
				return;
			}
			audMan.SetLoop(true);
			audMan.QueueAudio(audAmbience);			
		}

		public void SetFrozen(bool freeze)
		{
			if (freeze)
			{
				if (!navigator.Entity.ExternalActivity.moveMods.Contains(moveMod))
					navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
			}
			else
				navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
			
		}

		public void GoToRandomSpot()
		{
			List<Cell> cells = [];
			for (int i = 0; i < ec.levelSize.x; i++)
			{
				for (int j = 0; j < ec.levelSize.z; j++)
				{
					if (!ec.cells[i, j].Null && ec.cells[i, j].room.type == RoomType.Hall && (ec.cells[i, j].shape == TileShapeMask.Corner || ec.cells[i, j].shape == TileShapeMask.End || ec.cells[i, j].shape == TileShapeMask.Single) && !ec.cells[i, j].HardCoverageFits(CellCoverage.Down | CellCoverage.Center))
						cells.Add(ec.cells[i, j]);
				}
			}

			navigator.Entity.Teleport(cells[Random.Range(0, cells.Count)].CenterWorldPosition);
			StartCoroutine(SpawnDelay());
		}

		public void TeleportPlayer(PlayerManager pm)
		{
			pm.GetCustomCam().ReverseSlideFOVAnimation(new ValueModifier(), 115f, 4f);
			List<NPC> npcs = new(ec.Npcs);
			npcs.RemoveAll(x => x == this || !x.GetMeta().flags.HasFlag(NPCFlags.Standard) || !x.Navigator.Entity || ec.CellFromPosition(x.transform.position).Null);

			if (npcs.Count != 0)
				StartCoroutine(TeleportDelay(pm, npcs[Random.Range(0, npcs.Count)]));
		}

		IEnumerator TeleportDelay(PlayerManager pm, NPC npc)
		{
			pm.plm.Entity.IgnoreEntity(npc.Navigator.Entity, true);
			Vector3 pos = npc.transform.position;
			npc.Navigator.Entity.Teleport(pm.transform.position);
			pm.plm.Entity.Teleport(pos);

			yield return null;

			pm.plm.Entity.IgnoreEntity(npc.Navigator.Entity, false);
			int halls = Random.Range(minHallucinations, maxHallucinations);
			for (int i = 0; i < halls; i++)
			{
				var hal = Instantiate(hallPre);
				hal.AttachToPlayer(pm);

				hallucinations.Add(hal);
				yield return null;
			}

			yield break;
		}

		IEnumerator SpawnDelay()
		{
			EntityOverrider overrider = new();
			navigator.Entity.Override(overrider);
			float normHeight = navigator.Entity.InternalHeight;
			overrider.SetHeight(0f);
			float curHeight = 0f;
			float tar = normHeight - 0.05f;

			while (true)
			{
				curHeight += (normHeight - curHeight) / 3f * TimeScale * Time.deltaTime * 15f;
				if (curHeight >= tar)
					break;
				
				overrider.SetHeight(curHeight);
				yield return null;
			}
			overrider.SetHeight(normHeight);
			overrider.Release();

			yield break;
		}

		[SerializeField]
		internal SpriteRenderer spriteToHide;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal AudioManager screenAudMan;

		[SerializeField]
		internal SoundObject audAmbience, audAngry, audSpot, audTeleport;

		[SerializeField]
		internal Hallucinations hallPre;

		[SerializeField]
		internal int minHallucinations = 7, maxHallucinations = 9;

		readonly List<Hallucinations> hallucinations = [];

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}

	internal class Watcher_StateBase(Watcher w) : NpcState(w)
	{
		protected Watcher w = w;
	}

	internal class Watcher_WaitBelow(Watcher w) : Watcher_StateBase(w)
	{
		float cooldown = Random.Range(20f, 40f);

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_DoNothing(w, 0));
			w.Hide(true);
		}

		public override void Update()
		{
			base.Update();
			cooldown -= w.TimeScale * Time.deltaTime;
			if (cooldown <= 0f)
				w.behaviorStateMachine.ChangeState(new Watcher_Active(w));
		}
	}

	internal class Watcher_Active(Watcher w) : Watcher_StateBase(w)
	{
		public override void Initialize()
		{
			base.Initialize();			
			w.DespawnHallucinations(false);
			w.GoToRandomSpot();
			w.SetFrozen(true);
			w.Hide(false);
			w.screenAudMan.FlushQueue(true);
			w.screenAudMan.Pause(true);
		}

		public override void Sighted()
		{
			base.Sighted();
			if (!hasPlayed)
			{
				hasPlayed = true;

				w.screenAudMan.SetLoop(true);
				w.screenAudMan.QueueAudio(w.audSpot);
			}
			w.screenAudMan.Pause(false);
			stillInSight = true;
		}

		public override void Unsighted()
		{
			base.Unsighted();
			w.screenAudMan.Pause(true);
			mod.addend = 0;
			stillInSight = false;
			if (lastSawPlayer && moveMods.TryGetValue(lastSawPlayer, out var mmod))
				mmod.movementAddend = Vector3.zero;
		}

		public override void InPlayerSight(PlayerManager player)
		{
			base.InPlayerSight(player);

			if (!moveMods.TryGetValue(player, out var moveMod))
				return;

			lastSawPlayer = player;
			spotStrength += w.TimeScale * Time.deltaTime * 6.5f;
			if (Time.timeScale > 0)
				mod.addend = spotStrength * (-1f + Random.value * 2f) * 2f;
			Vector3 distance = w.transform.position - player.transform.position;
			moveMod.movementAddend = distance.normalized * Mathf.Min(15f, distance.magnitude * 0.6f);

			player.transform.RotateSmoothlyToNextPoint(w.transform.position, 0.35f);
			if (spotStrength > strengthLimit)
			{
				player.GetCustomCam().RemoveModifier(mod);
				w.behaviorStateMachine.ChangeState(new Watcher_Attack(w, player));
			}
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);

			var moveMod = new MovementModifier(Vector3.zero, 1f);
			player.Am.moveMods.Add(moveMod);
			moveMods.Add(player, moveMod);
			
			mod.addend = 0;
			player.GetCustomCam().AddModifier(mod);
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			player.GetCustomCam().RemoveModifier(mod);
			if (moveMods.TryGetValue(player, out var modifier))
			{
				player.Am.moveMods.Remove(modifier);
				moveMods.Remove(player);
			}
		}

		public override void Update()
		{
			base.Update();
			if (stillInSight) return;

			leaveCooldown -= w.TimeScale * Time.deltaTime;
			if (leaveCooldown <= 0f)
				w.behaviorStateMachine.ChangeState(new Watcher_WaitBelow(w));
		}

		public override void Exit()
		{
			base.Exit();
			foreach (var move in moveMods)
				move.Key?.Am.moveMods.Remove(move.Value);
		}

		bool hasPlayed = false, stillInSight = false;


		float spotStrength = 0f, leaveCooldown = Random.Range(30f, 60f);
		const float strengthLimit = 12f;
		readonly ValueModifier mod = new();

		readonly Dictionary<PlayerManager, MovementModifier> moveMods = [];
		PlayerManager lastSawPlayer;

	}

	internal class Watcher_Attack(Watcher w, PlayerManager pm) : Watcher_StateBase(w)
	{
		readonly PlayerManager target = pm;
		CustomPlayerCameraComponent comp;
		NavigationState_TargetPlayer tar;

		public override void Initialize()
		{
			base.Initialize();
			comp = target.GetCustomCam();
			comp.AddModifier(mod);

			w.SetFrozen(false);
			w.GetAngry();
			w.screenAudMan.Pause(false);
			w.screenAudMan.FlushQueue(true);
			w.screenAudMan.SetLoop(true);
			w.screenAudMan.QueueAudio(w.audAngry);
			tar = new NavigationState_TargetPlayer(w, 127, target.transform.position, true);
			ChangeNavigationState(tar);
		}

		public override void Update()
		{
			base.Update();
			ChangeNavigationState(tar);
			tar.UpdatePosition(target.transform.position);
			if (Time.timeScale > 0)
				mod.addend = 25f * (-1f + Random.value * 2f);
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			base.OnStateTriggerEnter(other);
			if (other.gameObject == target.gameObject)
			{
				w.screenAudMan.FlushQueue(true);
				w.screenAudMan.PlaySingle(w.audTeleport);
				w.TeleportPlayer(target);

				w.behaviorStateMachine.ChangeState(new Watcher_WaitBelow(w));
			}
		}

		public override void Exit()
		{
			base.Exit();
			comp.RemoveModifier(mod);
		}

		readonly ValueModifier mod = new();
	}
}
