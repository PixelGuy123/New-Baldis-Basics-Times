using BBTimes.CustomComponents;
using BBTimes.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BBTimes.CustomContent.NPCs
{
	public class OfficeChair : NPC, INPCPrefab, IItemAcceptor // Npc here
	{
		public void SetupPrefab()
		{
			audRoll = this.GetSound("ChairRolling.wav", "Vfx_OFC_Walk", SoundType.Voice, new(0.74609375f, 0.74609375f, 0.74609375f));

			var storedSprites = this.GetSpriteSheet(2, 1, 24f, "officeChair.png");
			spriteRenderer[0].sprite = storedSprites[0];
			sprActive = storedSprites[0];
			sprDeactive = storedSprites[1];
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

			behaviorStateMachine.ChangeState(new OfficeChair_FindOffice(this, true));

			navigator.maxSpeed = normSpeed;
			navigator.SetSpeed(normSpeed);

			var man = GetComponent<PropagatedAudioManager>();
			man.maintainLoop = true;
			man.FlushQueue(true);
		}

		public void CarryEntityAround(Entity en) =>
			behaviorStateMachine.ChangeState(new OfficeChair_FindOffice(this, false, awaitCooldown, en));


		public void SetEnabled(bool active) => spriteRenderer[0].sprite = active ? sprActive : sprDeactive;

		const float normSpeed = 50f;

		const float awaitCooldown = 40f;

		internal OfficeChair_FindOffice bringingState;

		public override void Despawn()
		{
			bringingState?.CancelTargetGrab();
			base.Despawn();
		}

		public bool ItemFits(Items itm) {
			if (behaviorStateMachine.CurrentState is OfficeChair_WaitForCollision col && col.cooldown > 0f && itemsThatFixMe.Contains(itm))
				return true;
			return false;
		}

		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			if (behaviorStateMachine.CurrentState is OfficeChair_WaitForCollision col)
				col.cooldown = -1f;
		}

		[SerializeField]
		internal Sprite sprActive, sprDeactive;

		[SerializeField]
		internal SoundObject audRoll;

		readonly static HashSet<Items> itemsThatFixMe = [];
		public static void AddFixableItem(Items i) => itemsThatFixMe.Add(i);


	}

	internal class OfficeChair_StateBase(OfficeChair office) : NpcState(office) // A default npc state
	{
		protected OfficeChair chair = office;

		protected PropagatedAudioManager man = office.GetComponent<PropagatedAudioManager>();

		public override void DoorHit(StandardDoor door)
		{
			if (door.locked)
			{
				door.Unlock();
				door.OpenTimed(5f, false);
				return;
			}
			base.DoorHit(door);
		}
	}

	internal class OfficeChair_FindOffice(OfficeChair office, bool useCurrent, float cooldown = -1f, Entity target = null) : OfficeChair_StateBase(office) // A basic moving npc state
	{
		readonly bool useCurrent = useCurrent;

		Entity target = target;

		// float entityBaseHeight = 0f;

		readonly float waitCooldown = cooldown;

		Cell targetCell;
		public override void Enter() // Basically go to a random spot
		{
			base.Enter();


			if (target)
			{
				target.Override(overrider);
				//	entityBaseHeight = target.InternalHeight;
				SetTarget(false);
				target.Teleport(chair.transform.position);
				overrider.SetHeight(target.InternalHeight + heightOffset);
			}

			var room = chair.ec.CellFromPosition(chair.transform.position).room;
			List<Cell> cells = useCurrent ? room.GetTilesOfShape(TileShapeMask.Single, true) : GetRandomOffice(room);
			if (cells.Count == 0)
				cells = useCurrent ? room.AllEntitySafeCellsNoGarbage() : GetRandomOffice(room, true);

			targetCell = cells[Random.Range(0, cells.Count)];

			ChangeNavigationState(new NavigationState_TargetPosition(chair, 64, targetCell.FloorWorldPosition));
			man.QueueAudio(chair.audRoll, true);
			man.SetLoop(true);
			chair.bringingState = this;


		}

		public override void DestinationEmpty() // Destination empty (means it got to its location), now just wait idle
		{
			base.DestinationEmpty();
			if (!initialized) return;

			if (chair.ec.CellFromPosition(chair.transform.position) != targetCell)
			{
				ChangeNavigationState(new NavigationState_TargetPosition(chair, 64, targetCell.FloorWorldPosition));
				return;
			}

			man.FlushQueue(true);
			if (target)
			{
				// target.SetHeight(entityBaseHeight);
				overrider.SetHeight(target.BaseHeight);
				SetTarget(true);

				chair.SetEnabled(false);
				overrider.Release();
				target = null;
			}
			chair.behaviorStateMachine.ChangeState(new OfficeChair_WaitForCollision(chair, waitCooldown));
		}

		public override void Update()
		{
			if (!target) return;

			if (!chair || chair.Navigator.Entity.Frozen || (chair.transform.position - target.transform.position).magnitude > 5f) // If chair ever becomes null, also stop this
			{
				CancelTargetGrab();
			}
			else
				target.Teleport(chair.transform.position);


		}

		List<Cell> GetRandomOffice(RoomController room, bool allTiles = false)
		{
			List<RoomController> rooms = new(chair.ec.rooms);
			rooms.RemoveAll(x => x == room || (x.category != RoomCategory.Office && x.category != RoomCategory.Faculty));

#if CHEAT
            Debug.Log($"(Office Chair): Original amount of rooms found: {chair.ec.rooms.Count}");
            Debug.Log($"(Office Chair): Amount of rooms found: {rooms.Count}");
#endif
			if (rooms.Count == 0) // Just for pre-caution.... even though this might not even happen
				return [];

			if (!allTiles)
			{
				List<List<Cell>> cellsToChoose = [];

				rooms.ForEach(x =>
				{
					var cells = x.AllEntitySafeCellsNoGarbage();
					if (cells.Count != 0)
						cellsToChoose.Add(cells);
				});

				return cellsToChoose[Random.Range(0, cellsToChoose.Count)];
			}
			return rooms[Random.Range(0, rooms.Count)].GetTilesOfShape(TileShapeMask.Single, true);
		}

		void SetTarget(bool active)
		{
			overrider.SetFrozen(!active);
			overrider.SetInteractionState(active);
		}

		public void CancelTargetGrab()
		{
			if (target)
			{
				//target.SetHeight(entityBaseHeight);
				overrider.SetHeight(target.InternalHeight);
				SetTarget(true);
			}
			overrider.Release();
			target = null;
		}

		const float heightOffset = 3f;

		readonly EntityOverrider overrider = new();
	}

	internal class OfficeChair_WaitForCollision(OfficeChair office, float waitCooldown) : OfficeChair_StateBase(office)
	{
		internal float cooldown = waitCooldown;

		public override void Enter()
		{
			ChangeNavigationState(new NavigationState_DoNothing(chair, 0));
			base.Enter();
			chair.Navigator.Am.moveMods.Add(moveMod);
			if (cooldown > 0f)
				chair.StartCoroutine(Cooldown());
		}

		public override void OnStateTriggerEnter(Collider other)
		{
			if (cooldown > 0f) return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				Entity component = other.GetComponent<Entity>();
				if (component != null)
					chair.CarryEntityAround(component);


			}

		}

		public override void Exit()
		{
			base.Exit();
			chair.Navigator.Am.moveMods.Remove(moveMod);
		}

		IEnumerator Cooldown()
		{
			while (cooldown > 0f)
			{
				cooldown -= Time.deltaTime * chair.TimeScale;
				yield return null;
			}
			chair.SetEnabled(true);

			yield break;
		}

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);


	}
}
