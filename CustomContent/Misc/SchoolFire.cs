using MTM101BaldAPI.Components;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.Misc
{
	public class SchoolFire : CustomSpriteAnimator
	{
		private void Start()
		{
			SetAnimations();
			SetDefaultAnimation("default", 1f);
		}

		private void SetAnimations() =>
			animations.Add("default", anim);

		internal IEnumerator Spawn(Vector3 ogScale)
		{
			float scale = 0;
			Vector3 pos = transform.position;
			while (scale < ogScale.x)
			{
				scale += (ogScale.x - scale) / 5f * Time.deltaTime * ec.EnvironmentTimeScale;
				transform.localScale = Vector3.one * scale;
				pos.y = (4 * transform.localScale.y) + 0.28f;
				transform.position = pos;
				yield return null;
			}
			transform.localScale = ogScale;

			yield break;
		}

		internal EnvironmentController ec;

		internal static CustomAnimation<Sprite> anim; // Workaround is just a list of Sprite[]
	}
}
