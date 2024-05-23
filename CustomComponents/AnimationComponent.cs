using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class AnimationComponent : MonoBehaviour
	{
		void Update()
		{
			if (ec)
			{
				frame += speed * ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= animation.Length;
				renderer.sprite = animation[Mathf.FloorToInt(frame)];
			}
		}

		public void Initialize(EnvironmentController ec) =>
		this.ec = ec;

		protected EnvironmentController ec;

		[SerializeField]
		[Range(0.1f, float.MaxValue)]
		internal float speed = 5;

		[SerializeField]
		internal Sprite[] animation;

		[SerializeField]
		internal SpriteRenderer renderer;

		float frame = 0f;
	}
}
