using UnityEngine;

namespace BBTimes.CustomContent
{
    public class VisualAttacher : MonoBehaviour
    {
        public void AttachTo(Transform obj, Vector3 offset, bool selfDestructWhenUnavailable)
        {
            this.offset = offset;
            target = obj;
			destroyMyselfWhenUnavailable = selfDestructWhenUnavailable;
		}
        public void AttachTo(Transform obj, bool selfDestructWhenUnavailable) => AttachTo(obj, offset, selfDestructWhenUnavailable);

		public void SetOwnerRefToSelfDestruct(GameObject ownerRef)
		{
			hasOwnerReference = true;
			OwnerReference = ownerRef;
		}

        void Update()
        {
			if (hasOwnerReference && !OwnerReference)
			{
				Destroy(gameObject);
				return;
			}

			if (!target)
			{
				if (destroyMyselfWhenUnavailable)
					Destroy(gameObject);
				return;
			}

            transform.position = target.position + offset;
        }

        Transform target;
        Vector3 offset = Vector3.zero;
		bool hasOwnerReference = false, destroyMyselfWhenUnavailable = false;
        public Transform AttachedObject => target;
		public GameObject OwnerReference { get; private set; }
        public Vector3 Offset => offset;
    }
}
