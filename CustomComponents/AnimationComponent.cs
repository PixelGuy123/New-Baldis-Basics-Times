using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class AnimationComponent : MonoBehaviour
	{
		void Update()
		{
			if (ec)
			{
				if (!Paused)
				{
					frame += speed * ec.EnvironmentTimeScale * Time.deltaTime;
					if (frame >= animation.Length)
					{
						if (lastFrameMode)
						{
							StopLastFrameMode();
							Pause(true);
							frame = animation.Length - 1;
						}
						else
							frame %= animation.Length;
					}
				}
				renderer.sprite = animation[Mathf.FloorToInt(frame)];
			}
		}

		public void Initialize(EnvironmentController ec) =>
			this.ec = ec;

		public void Pause(bool pause)
		{
			if (pause)
				this.pause++;
			else
				this.pause = Mathf.Max(0, this.pause - 1);
		}

		public void StopLastFrameMode() =>
			lastFrameMode = true;
		
		public void ResetFrame() => frame = 0f;

		public bool Paused => pause != 0;
		public bool LastFrameMode => lastFrameMode;

		protected EnvironmentController ec;

		[SerializeField]
		[Range(0.1f, float.MaxValue)]
		internal float speed = 5;

		[SerializeField]
		public Sprite[] animation;

		[SerializeField]
		internal SpriteRenderer renderer;

		int pause = 0;

		bool lastFrameMode = false;

		float frame = 0f;
	}
}
