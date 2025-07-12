using UnityEngine;

namespace BBTimes.CustomComponents
{
    public class CustomPickupBob : MonoBehaviour
    {
        private void Update()
        {
            val += Time.deltaTime;
            transform.localPosition = Vector3.up * (Mathf.Sin(val * speed) * 0.5f);
        }

        private void OnEnable()
        {
            val = PickupBobValuePatch.activeBob.val; // stay consistent
            transform.localPosition = Vector3.up * (Mathf.Sin(val * speed) * 0.5f);
        }

        public float speed = 5f;
        private float val;
    }
}
