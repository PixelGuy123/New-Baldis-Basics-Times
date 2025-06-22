
using System.Collections.Generic;
using UnityEngine;
using BBTimes.Manager;

namespace BBTimes.CustomContent.Builders
{
    class Structure_DoorLockerSmth : StructureBuilder
    {
        public GameObject Prefab;

        public Dictionary<RoomController, List<GameObject>> lightsRooms;
        public List<RoomController> PossibleRooms;
        public List<RoomController> LockedRooms;
        public override void Generate(LevelGenerator lg, System.Random rng)
        {
            base.Generate(lg, rng);

            PossibleRooms = [.. ec.rooms];
            PossibleRooms.RemoveAll(x => x.category != RoomCategory.Class);
            // Remove one random room
            LockedRooms = [.. PossibleRooms];
            lightsRooms = new Dictionary<RoomController, List<GameObject>>();
            LockedRooms.RemoveAt(UnityEngine.Random.Range(0, LockedRooms.Count));
            foreach (var room in PossibleRooms)
            {
                List<GameObject> lights = new List<GameObject>();
                foreach (StandardDoor door in room.doors)
                {
                    if (door.GetType() != typeof(StandardDoor))
                        continue;
                    var light = Instantiate(
                        Prefab,
                        door.doors[0].transform.position,
                        door.transform.rotation);
                    light.transform.position += light.transform.forward * 0.01f;
                    lights.Add(light);

                    light = Instantiate(
                        Prefab,
                        door.doors[0].transform.position,
                        door.transform.rotation);
                    light.transform.position += -light.transform.forward * 0.01f;
                    lights.Add(light);

                }
                lightsRooms.Add(room, lights);
                var handle = room.gameObject.AddComponent<DoorLockerSmthRoomHandler>();
                handle.room = room;
                handle.doorLocker = this;
                
            }
        }
    }

    class DoorLockerSmthRoomHandler : MonoBehaviour
    {

        public RoomController room;
        public Structure_DoorLockerSmth doorLocker;

        public bool isLocked => doorLocker.LockedRooms.Contains(room);

        public bool isCollected => room.activity.notebook.collected;

        public List<GameObject> lights;

        public bool IsAlreadyCollected = false;
        float timeBeforeUnlock = 5f;

        private void Start()
        {
            doorLocker.lightsRooms.TryGetValue(room, out lights);
            foreach (var light in lights)
            {
                if (isLocked)
                {
                    light.GetComponent<SpriteRenderer>().sprite = BBTimesManager.levelTypeAssetManager.Get<Sprite>("spr_ClassStandard_Off");
                }
                else
                {
                    light.GetComponent<SpriteRenderer>().sprite = BBTimesManager.levelTypeAssetManager.Get<Sprite>("spr_ClassStandard_On");
                }
            }

        }

        void Update()
        {
            
            if (isLocked)
            {
                foreach (var door in room.doors)
                {
                    door.Lock(true);
                    door.Shut();
                }
                foreach (var light in lights)
                {
                    light.GetComponent<SpriteRenderer>().sprite = BBTimesManager.levelTypeAssetManager.Get<Sprite>("spr_ClassStandard_Off");
                }
            }
            else
            {
                foreach (var door in room.doors)
                {
                    door.Unlock();

                }
                foreach (var light in lights)
                {
                    light.GetComponent<SpriteRenderer>().sprite = BBTimesManager.levelTypeAssetManager.Get<Sprite>("spr_ClassStandard_On");
                }
            }
            if (isCollected)
            {
                timeBeforeUnlock -= Time.deltaTime;
            }

            if (timeBeforeUnlock < 0 && !IsAlreadyCollected)
            {
                IsAlreadyCollected = true;
                doorLocker.LockedRooms.Remove(doorLocker.LockedRooms[UnityEngine.Random.Range(0, doorLocker.LockedRooms.Count)]);
            }
        }

        
    }
}