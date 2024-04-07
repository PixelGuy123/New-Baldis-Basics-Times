using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class GottaSweepComponent : MonoBehaviour
	{
		[SerializeField]
		public SoundObject aud_sweep;

		public bool active;

		public float cooldown;
	}
}
