using UnityEngine;

namespace BBTimes.CustomComponents;

public class OffsettledPickupBob : MonoBehaviour
{
    [SerializeField]
    internal float offset = 1f;

    private void Update()
    {
        transform.localPosition = new Vector3(0f, PickupBobValue.bobVal + offset, 0f);
    }

    private void OnEnable()
    {
        transform.localPosition = new Vector3(0f, PickupBobValue.bobVal + offset, 0f);
    }
}