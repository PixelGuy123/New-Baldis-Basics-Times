using UnityEngine;

namespace BBTimes.CustomComponents;

public class RunLineMarker : MonoBehaviour { } // used for Dribble's room

public class BasketballHoopMarker : MonoBehaviour // used in Dribble's minigame for basketballs
{
    public Vector3 localHoopPosition = Vector3.zero;
    public AudioManager audMan;
    public SoundObject audGoal;
}