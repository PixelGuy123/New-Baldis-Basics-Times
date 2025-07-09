using System.Collections;
using BBTimes.CustomComponents;
using UnityEngine;

namespace BBTimes.CustomContent.Misc
{
	public class SchoolFire : AnimationComponent
	{
		internal IEnumerator Spawn(Vector3 ogScale, float smoothness = 5f)
		{
			float scale = 0;
			Vector3 pos = transform.position;
			while (scale < ogScale.x)
			{
				scale += (ogScale.x - scale) / smoothness * Time.deltaTime * ec.EnvironmentTimeScale;
				transform.localScale = Vector3.one * scale;
				pos.y = (4 * transform.localScale.y) + 0.28f;
				transform.position = pos;
				yield return null;
			}
			transform.localScale = ogScale;

			yield break;
		}
	}
}
