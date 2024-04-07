using UnityEngine;

namespace BBTimes.CustomContent.MapIcons
{
	public class SelfRotatingIcon : MapIcon
	{
		private void LateUpdate() =>
			transform.rotation = Quaternion.identity;
	}
}
