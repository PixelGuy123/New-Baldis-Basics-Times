
using System.Collections.Generic;
using UnityEngine;
using BBTimes.Manager;

namespace BBTimes.CustomContent.Builders
{
    class Structure_DoorLockerSmth : StructureBuilder
    {
        public GameObject Prefab;
        public override void Generate(LevelGenerator lg, System.Random rng)
        {
            base.Generate(lg, rng);
        
            List<RoomController> PossibleRooms = [.. ec.rooms];
            // PossibleRooms.RemoveAll(x => x.category != RoomCategory.Class);
            // Remove one random room
            List<RoomController> LockedRooms = [.. PossibleRooms];
            // LockedRooms.RemoveAt(UnityEngine.Random.Range(0, LockedRooms.Count));
            foreach (var room in PossibleRooms)
            {

                foreach (var door in room.doors)
                {
                    Instantiate(
                        Prefab,
                        door.transform.position,
                        door.transform.rotation);
                    
                }
            }
        }
    }
}