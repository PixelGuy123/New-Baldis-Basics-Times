using UnityEngine;

namespace BBTimes.CustomComponents;

public class BasketballHoopMarker : MonoBehaviour // used in Dribble's minigame for basketballs
{
    public Vector3 localHoopPosition = Vector3.zero;
    public AudioManager audMan;
    public SoundObject audGoal;
}