using PixelInternalAPI.Classes;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class BasicLookerInstance(Transform origin)
	{
		public BasicLookerInstance(Transform origin, LayerMask mask) : this(origin) =>
			_mask = mask;
		

		public bool Raycast(Transform target, float rayDistance)
		{
			var offset = target.position - origin.position;
			if (offset.magnitude > rayDistance || _mask != (_mask | (1 << target.gameObject.layer)))
				return false;

			ray.origin = origin.position;
			ray.direction = offset;

			if (Physics.Raycast(ray, out hit, rayDistance, _mask, QueryTriggerInteraction.Ignore))
				return hit.transform == target;
			return false;
		}

		readonly Transform origin = origin;

		readonly LayerMask _mask = LayerStorage.principalLookerMask;

		Ray ray = new();

		RaycastHit hit;
	}
}
