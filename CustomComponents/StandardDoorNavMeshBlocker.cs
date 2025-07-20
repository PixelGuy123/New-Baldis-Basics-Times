using BBTimes.Extensions;
using UnityEngine;
using UnityEngine.AI;

namespace BBTimes.CustomComponents;

public class StandardDoorNavMeshBlocker : MonoBehaviour
{
    public static StandardDoorNavMeshBlocker AddBlockerToDoor(Door door)
    {
        var blocker = door.gameObject.AddComponent<StandardDoorNavMeshBlocker>();
        blocker.obstacle = door.gameObject.AddNavObstacle(new(0f, 5f, 5f), new(10f, 10f, 1f));
        blocker.obstacle.enabled = false;

        blocker.door = door;

        return blocker;
    }

    void Update()
    {
        if (enableObstacleBehavior)
            obstacle.enabled = door.locked;
        else
            obstacle.enabled = false;
    }
    public void EnableObstacle(bool enable) =>
        enableObstacleBehavior = enable;

    NavMeshObstacle obstacle;
    Door door;
    bool enableObstacleBehavior;
}