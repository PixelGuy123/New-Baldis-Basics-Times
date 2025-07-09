using UnityEngine;

namespace BBTimes.CustomComponents;

public class FacultyDoorOpener : MonoBehaviour
{
    void Awake()
    {
        door = GetComponent<FacultyOnlyDoor>();
        foreach (var collider in door.GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }

    void Update()
    {
        if (!door)
            return;
        door.alarmSounded = true;
        door.playerDetected = false;
        door.time = door.timeToCloseSetting;
    }

    FacultyOnlyDoor door;
}