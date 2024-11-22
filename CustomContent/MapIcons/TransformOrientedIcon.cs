using UnityEngine;

namespace BBTimes.CustomContent.MapIcons
{
	public class TransformOrientedIcon : MapIcon
	{
		void LateUpdate()
		{
			if (target)
				transform.rotation = Quaternion.Euler(0f, 0f, invertRotation ? -target.rotation.eulerAngles.y : target.rotation.eulerAngles.y);
		}

		[SerializeField]
		public bool invertRotation = false;
	}
}
