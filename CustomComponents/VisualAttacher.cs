using UnityEngine;

namespace BBTimes.CustomContent
{
    public class VisualAttacher : MonoBehaviour
    {
        public void AttachTo(Transform obj, Vector3 offset)
        {
            this.offset = offset;
            target = obj;
        }
        public void AttachTo(Transform obj) => target = obj;

        void Update()
        {
            if (!target) return;
            transform.position = target.position + offset;
        }

        Transform target;
        Vector3 offset = Vector3.zero;
        public Transform AttachedObject => target;
        public Vector3 Offset => offset;
    }
}
