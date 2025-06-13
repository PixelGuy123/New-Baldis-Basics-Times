using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions;

public class RoomBaseFunction : RoomFunction
{
    public override void Initialize(RoomController room)
    {
        base.Initialize(room);

        roomBase.SetParent(room.objectObject.transform);
        roomBase.localPosition = Vector3.zero;
    }

    public override void OnGenerationFinished()
    {
        base.OnGenerationFinished();

        if (centerOfRoom)
            roomBase.position = room.ec.RealRoomMid(room);
    }

    [SerializeField]
    internal Transform roomBase;

    [SerializeField]
    internal bool centerOfRoom = true;
}