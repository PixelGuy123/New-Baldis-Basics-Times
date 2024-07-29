using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class StarObject : MonoBehaviour
	{
		Entity e;
		public void SetTarget(Entity e)
		{
			this.e = e;
			transform.SetParent(e.transform);
		}

		private void Update()
		{
			if (!e) return;
			transform.localPosition = Vector3.up * (e.InternalHeight - 1.5f);
		}
	}
}
