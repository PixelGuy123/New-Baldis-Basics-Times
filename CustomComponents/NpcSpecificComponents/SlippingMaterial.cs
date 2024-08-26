using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class SlippingMaterial : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				var pm = other.GetComponent<PlayerManager>();
				if (e && e.Grounded && (!pm || !pm.GetAttribute().HasAttribute("boots")))
				{
					e.AddForce(new(e.Velocity.normalized, force, -force * antiForceReduceFactor));
					if (audSlip)
						audMan.PlaySingle(audSlip);
				}
			}
		}

		[SerializeField]
		internal float force = 20f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float antiForceReduceFactor = 0.95f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSlip;
	}
}
