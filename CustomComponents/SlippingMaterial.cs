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
				if (SlipObject(other.gameObject, force, -force * antiForceReduceFactor ) && audSlip)
					audMan.PlaySingle(audSlip);
			}
		}

		public static bool SlipObject(GameObject other, float force, float acceleration)
		{
			var e = other.GetComponent<Entity>();

			if (e)
				return SlipEntity(e, force, acceleration);
			return false;

		}

		public static bool SlipEntity(Entity e, float force, float acceleration)
		{
			var pm = e.GetComponent<PlayerManager>();
			if (e.Grounded && !float.IsNaN(e.Velocity.x) && (!pm || !pm.GetAttribute().HasAttribute("boots")))
			{
				float maxMagnitude = Mathf.Min(100f, Mathf.Abs(e.Velocity.magnitude));
				e.AddForce(new(e.Velocity.normalized, Mathf.Abs(force + maxMagnitude), -(maxMagnitude + Mathf.Abs(acceleration))));
				return true;
			}
			return false;
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
