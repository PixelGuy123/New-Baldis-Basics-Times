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

			if (offset.magnitude > rayDistance)
				return false;

			int hitCount = Physics.RaycastNonAlloc(new Ray(origin.position, offset.normalized), hits, rayDistance, _mask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < hitCount; i++)
				if (hits[i].transform.CompareTag("Player") && !hits[i].transform.CompareTag("NPC") && hits[i].transform != target)
					return false;

			return true;
		}

		readonly Transform origin = origin;

		readonly LayerMask _mask = LayerStorage.principalLookerMask;

		RaycastHit[] hits = new RaycastHit[32];
	}
}
