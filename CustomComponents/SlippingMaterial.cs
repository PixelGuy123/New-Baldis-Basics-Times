using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class SlippingMaterial : MonoBehaviour
	{
		public void SetAnOwner(GameObject owner) =>
			this.owner = owner;
		private void OnTriggerEnter(Collider other)
		{
			if (other.gameObject == owner) return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				var pm = other.GetComponent<PlayerManager>();
				if (e && e.Grounded && !float.IsNaN(e.Velocity.x) && (!pm || !pm.GetAttribute().HasAttribute("boots")))
				{
					e.AddForce(new(e.Velocity.normalized, force + e.Velocity.magnitude, e.Velocity.magnitude + (-force * antiForceReduceFactor)));
					if (audSlip)
						audMan.PlaySingle(audSlip);
				}
			}
		}

		[SerializeField]
		internal float force = 45f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float antiForceReduceFactor = 0.8f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSlip;

		GameObject owner;
	}
}
