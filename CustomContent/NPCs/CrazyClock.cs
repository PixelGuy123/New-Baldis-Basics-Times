using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using BBTimes.CustomComponents;
using HarmonyLib;
using PixelInternalAPI.Extensions;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.NPCs
{
    public class CrazyClock : NPC, INPCPrefab // He's not very well coded because I removed the CustomClockData reference from it, but I'll leave it like that
	{
		public void SetupPrefab()
		{
			SoundObject[] audios = [this.GetSound("clock_tick.wav", "Vfx_CC_Tick", SoundType.Voice, Color.yellow),
		this.GetSound("clock_tack.wav", "Vfx_CC_Tack", SoundType.Voice, Color.yellow),
		this.GetSound("clock_Scream.wav", "Vfx_CC_Scream", SoundType.Voice, Color.yellow),
		this.GetSound("clock_frown.wav", "Vfx_CC_Frown", SoundType.Voice, Color.yellow)];

			audTick = audios[0];
			audTack = audios[1];
			allClockAudios = audios;
			allClockSprites = this.GetSpriteSheet(5, 5, 35f, "crazyClockSheet.png");

			spriteRenderer[0].sprite = allClockSprites[0];

			spriteRenderer[0].material = new(ObjectCreationExtensions.NonBillBoardPrefab.material);
			baseTrigger.Do(x => x.enabled = false);
			audMan = GetComponent<AudioManager>();
			Navigator.enabled = false; // It's a static npc
			Navigator.Entity.SetActive(false);
			Navigator.Entity.enabled = false;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("npcs", "Textures");
		public string SoundPath => this.GenerateDataPath("npcs", "Audios");
		public NPC Npc { get; set; }
		[SerializeField] Character[] replacementNPCs; public Character[] GetReplacementNPCs() => replacementNPCs; public void SetReplacementNPCs(params Character[] chars) => replacementNPCs = chars;
		public int ReplacementWeight { get; set; }

		// ---------------------------------------------------------------

		public override void Initialize()
		{
			base.Initialize();
			foreach (var r in ec.rooms)
			{
				if (preAllowedCategories.Contains(r.category))
					availableCells.AddRange(r.GetTilesOfShape([TileShape.Corner, TileShape.Single, TileShape.End], true).Where(x => x.HasFreeWall && !x.HasAnyHardCoverage));
			}
			availableCells.AddRange(ec.mainHall.GetTilesOfShape([TileShape.Corner, TileShape.Single, TileShape.End], false).Where(x => x.HasFreeWall && !x.HasAnyHardCoverage));

			behaviorStateMachine.ChangeState(new CrazyClock_Spawn(this));
		}

		public void Tick(bool tack) => audMan.PlaySingle(!tack ? audTick : audTack);


		public static readonly List<RoomCategory> preAllowedCategories = [RoomCategory.Special];

		readonly List<Cell> availableCells = [];

		public List<Cell> Cells => availableCells;

		public void TeleportAllToMe()
		{
			var npcs = new List<NPC>(ec.Npcs);
			npcs.RemoveAll(x => x == this || !x.GetMeta().flags.HasFlag(NPCFlags.Standard) || !x.Navigator.isActiveAndEnabled);
			var cell = ec.CellFromPosition(transform.position);
				

			StartCoroutine(TeleportEachNPCRandomly(
				npcs, 
				cell,
				cell.AllOpenNavDirections.ConvertAll(x => x.ToVector3())));
		}

		IEnumerator TeleportEachNPCRandomly(List<NPC> npcs, Cell cell, List<Vector3> directionsToThrow)
		{
			float t = delayPerCharacter;
			while (npcs.Count != 0)
			{
				int idx = Random.Range(0, npcs.Count);
				if (npcs[idx])
				{
					npcs[idx].Navigator.Entity.Teleport(cell.CenterWorldPosition); // YES, CHAOS
					npcs[idx].Navigator.Entity.AddForce(new(directionsToThrow[Random.Range(0, directionsToThrow.Count)], throwSpeed, -throwSpeed * 0.95f));
				}
				npcs.RemoveAt(idx);

				t += delayPerCharacter;
				while (t > 0f)
				{
					t -= ec.EnvironmentTimeScale * Time.deltaTime;
					yield return null;
				}
				yield return null;
			}
		}

		[SerializeField]
		internal SoundObject audTick, audTack;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal Sprite[] allClockSprites;

		[SerializeField]
		internal SoundObject[] allClockAudios;

		[SerializeField]
		internal float delayPerCharacter = 0.12f, throwSpeed = 50f;
	}

	internal class CrazyClock_StateBase(CrazyClock clock) : NpcState(clock)
	{
		protected CrazyClock clock = clock;
	}

	internal class CrazyClock_Spawn(CrazyClock clock) : CrazyClock_StateBase(clock)
	{
		public override void Enter()
		{
			base.Enter();
			clock.spriteBase.SetActive(false);
			Cell cell = clock.Cells[Random.Range(0, clock.Cells.Count)];
			Direction dir = cell.RandomUncoveredDirection(new());
			clock.Navigator.Entity.Teleport(cell.CenterWorldPosition + (dir.ToVector3() * (LayerStorage.TileBaseOffset / 2 - 0.01f)));
			clock.spriteBase.transform.eulerAngles = dir.ToRotation().eulerAngles;
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			sighted++;
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			sighted--;
		}

		public override void Update()
		{
			base.Update();
			if (frameCounter > 0)
			{
				frameCounter--;
				return;
			}
			if (sighted <= 0 && !clock.looker.IsVisible)
			{
				clock.spriteBase.SetActive(true);
				clock.behaviorStateMachine.ChangeState(new CrazyClock_Active(clock));
			}
		}

		int sighted = 0;

		int frameCounter = 3; // Wait a little before spawning


	}

	internal class CrazyClock_Active(CrazyClock clock) : CrazyClock_StateBase(clock)
	{
		public override void Enter()
		{
			base.Enter();
			active = true;
		}

		public override void Update()
		{
			if (!active) return;
			base.Update();

			frame += speed * speedMultiplier * clock.TimeScale * Time.deltaTime;
			int idx = Mathf.FloorToInt(frame) % max;
				
			clock.spriteRenderer[0].sprite = clock.allClockSprites[idx + (nervous ? 0 : 4)];
			if (idx != lastIdx)
			{
				lastIdx = idx;
				clock.Tick(tick);
				tick = !tick;
			}
			if (!nervous)
			{
				cooldown -= clock.TimeScale * Time.deltaTime;
				if (cooldown <= 0f)
					clock.behaviorStateMachine.ChangeState(new CrazyClock_DespawnAndWait(clock));
				
			}
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			speedMultiplier = Mathf.Max(2f, 15f - (Vector3.Distance(player.transform.position, clock.transform.position) / 2.1f));
			if (speedMultiplier >= 10f)
				clock.behaviorStateMachine.ChangeState(new CrazyClock_Frown(clock));
		}

		public override void PlayerSighted(PlayerManager player)
		{
			base.PlayerSighted(player);
			nervous = true;
		}

		public override void PlayerLost(PlayerManager player)
		{
			base.PlayerLost(player);
			nervous = false;
			speedMultiplier = 1f;
		}

		const float speed = 0.7f;
		float speedMultiplier = 1f;
		const int max = 3;
		int lastIdx = 0;
		float frame = 0f;
		bool active = false;
		bool nervous = false;
		bool tick = false;
		float cooldown = 45f;
	}

	internal class CrazyClock_Frown(CrazyClock clock) : CrazyClock_StateBase(clock)
	{
		public override void Enter()
		{
			base.Enter();
			clock.spriteRenderer[0].sprite = clock.allClockSprites[8];
			clock.audMan.FlushQueue(true);
			clock.audMan.PlaySingle(clock.allClockAudios[3]); // frown
		}

		public override void Update()
		{
			base.Update();
			if (!clock.audMan.AnyAudioIsPlaying)
			{
				cooldown -= clock.TimeScale * Time.deltaTime;
				if (cooldown < 0f)
					clock.behaviorStateMachine.ChangeState(new CrazyClock_CRAZY(clock));
			}
		}

		float cooldown = 5f;
	}

	internal class CrazyClock_CRAZY(CrazyClock clock) : CrazyClock_StateBase(clock)
	{
		public override void Enter()
		{
			base.Enter();
			clock.audMan.PlaySingle(clock.allClockAudios[2]); // crazy noises
			clock.TeleportAllToMe();
		}

		public override void Update()
		{
			base.Update();
			if (!clock.audMan.AnyAudioIsPlaying)
				clock.behaviorStateMachine.ChangeState(new CrazyClock_DespawnAndWait(clock));
			else
			{
				frame += speed * clock.TimeScale * Time.deltaTime;
				int idx = Mathf.FloorToInt(frame);
				if (idx > 1)
				{
					idx = 0;
					frame = 0;
				}

				clock.spriteRenderer[0].sprite = clock.allClockSprites[idx + 9]; // 11
			}
			
		}

		float frame = 0f;
		const float speed = 7f;
	}

	internal class CrazyClock_DespawnAndWait(CrazyClock clock) : CrazyClock_StateBase(clock)
	{
		public override void Enter()
		{
			base.Enter();
			clock.StartCoroutine(Leave()); // leave lol
		}

		public override void Update()
		{
			if (!beginCooldown)
				return;

			base.Update();
			cooldown -= clock.TimeScale * Time.deltaTime;
			if (cooldown < 0f)
				clock.behaviorStateMachine.ChangeState(new CrazyClock_Spawn(clock));
		}

		IEnumerator Leave()
		{
			float frame = 11f;
			int idx;
			while (true)
			{
				frame += speed * clock.TimeScale * Time.deltaTime;
				idx = Mathf.FloorToInt(frame);
				if (idx < clock.allClockSprites.Length)
					clock.spriteRenderer[0].sprite = clock.allClockSprites[idx];
				else
					break;
				yield return null;
			}

			clock.spriteBase.SetActive(false);
			beginCooldown = true;

			yield break;
		}

		float cooldown = 30f;

		bool beginCooldown = false;

		const float speed = 11f;
	}
}
