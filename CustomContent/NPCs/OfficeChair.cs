using BBTimes.CustomComponents.CustomDatas;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace BBTimes.CustomContent.NPCs
{
    public class OfficeChair : NPC // Npc here
    {
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


        public void SetEnabled(bool active) => spriteRenderer[0].sprite = GetComponent<OfficeChairCustomData>().storedSprites[active ? 0 : 1];

        const float normSpeed = 30f;

        const float awaitCooldown = 40f;

		internal OfficeChair_FindOffice bringingState;

		public override void Despawn()
		{
			bringingState?.CancelTargetGrab();
			base.Despawn();
		}


	}

    internal class OfficeChair_StateBase(OfficeChair office) : NpcState(office) // A default npc state
    {
        protected OfficeChair chair = office;

        protected OfficeChairCustomData dat = office.GetComponent<OfficeChairCustomData>();

        protected PropagatedAudioManager man = office.GetComponent<PropagatedAudioManager>();
    }

    internal class OfficeChair_FindOffice(OfficeChair office, bool useCurrent, float cooldown = -1f, Entity target = null) : OfficeChair_StateBase(office) // A basic moving npc state
    {
        readonly bool useCurrent = useCurrent;

        Entity target = target;

        float entityBaseHeight = 0f;

        readonly float waitCooldown = cooldown;
        public override void Enter() // Basically go to a random spot
        {
            var room = chair.ec.CellFromPosition(chair.transform.position).room;
            List<Cell> cells = useCurrent ? room.GetTilesOfShape([TileShape.Single], true) : GetRandomOffice(room);
            if (cells.Count == 0)
                cells = useCurrent ? room.AllEntitySafeCellsNoGarbage() : GetRandomOffice(room, true);
#if CHEAT
            Debug.Log($"(Office Chair): {cells.Count} < Amount of tiles in the array");
            Debug.Log($"(Office Chair): {cells.Count} < Amount of tiles in the array");
#endif
            ChangeNavigationState(new NavigationState_TargetPosition(chair, 64, cells[Random.Range(0, cells.Count)].FloorWorldPosition));
            man.QueueAudio(dat.soundObjects[0], true);
            man.SetLoop(true);
			chair.bringingState = this;

            if (target != null)
            {
                entityBaseHeight = target.Height;
                SetTarget(false);
                target.Teleport(chair.transform.position);
                target.SetHeight(entityBaseHeight + heightOffset);
            }

            base.Enter();
        }

        public override void DestinationEmpty() // Destination empty (means it got to its location), now just wait idle
        {
            base.DestinationEmpty();
            if (!initialized) return;

            man.FlushQueue(true);
            if (target != null)
            {
                target.SetHeight(entityBaseHeight);
                SetTarget(true);

                chair.SetEnabled(false);
                target = null;
            }
            chair.behaviorStateMachine.ChangeState(new OfficeChair_WaitForCollision(chair, waitCooldown));
        }

        public override void Update()
        {
            if (target == null) return;
            
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
            rooms.RemoveAll(x => x == room || x.category != RoomCategory.Office && x.category != RoomCategory.Faculty);

#if CHEAT
            Debug.Log($"(Office Chair): Original amount of rooms found: {chair.ec.rooms.Count}");
            Debug.Log($"(Office Chair): Amount of rooms found: {rooms.Count}");
#endif
            if (rooms.Count == 0) // Just for pre-caution.... even though this might not even happen
                return [];

            return allTiles ? rooms[Random.Range(0, rooms.Count)].AllEntitySafeCellsNoGarbage() : rooms[Random.Range(0, rooms.Count)].GetTilesOfShape([TileShape.Single], true);
        }

        void SetTarget(bool active)
        {
            target.SetFrozen(!active);
            target.SetTrigger(active);
            if (target.CompareTag("Player"))
                target.GetComponent<PlayerManager>()?.Hide(!active);
            else
                target.GetComponent<NPC>()?.DisableCollision(!active);
        }

		public void CancelTargetGrab()
		{
			target.SetHeight(entityBaseHeight);
			SetTarget(true);
			target = null;
		}

        const float heightOffset = 3f;
    }

    internal class OfficeChair_WaitForCollision(OfficeChair office, float waitCooldown) : OfficeChair_StateBase(office)
    {
        float cooldown = waitCooldown;

        public override void Enter()
        {
			ChangeNavigationState(new NavigationState_DoNothing(chair, 64));
            base.Enter();
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


    }
}
