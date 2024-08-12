using BBTimes.CustomContent.CustomItems;
using UnityEngine;

namespace BBTimes.CompatibilityModule.BBPlusAnimations
{
	// ************ Soap Slipper ************
	internal class SoapSlipper : MonoBehaviour
	{
		[SerializeField]
		ITM_Soap myPeel;

		[SerializeField]
		public SpriteRenderer renderer;
		public void SetMyPeel(ITM_Soap peel) =>
			myPeel = peel;

		void Update()
		{
			if (myPeel)
			{
				if (myPeel.HoldingEntity)
				{
					var rot = Quaternion.LookRotation(myPeel.Direction).eulerAngles;
					rot.y -= 90f;
					transform.rotation = Quaternion.Euler(rot);
					transform.position = myPeel.transform.position - transform.right * 2f;
					renderer.enabled = true;
					return;
				}
				renderer.enabled = false;
			}
		}
	}
}
