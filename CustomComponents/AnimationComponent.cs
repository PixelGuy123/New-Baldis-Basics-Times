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
							lastFrameMode = false;
							Pause(true);
							frame = animation.Length - 1;
						}
						else
							frame %= animation.Length;
					}
				}
				ChangeRendererSpritesTo(animation[Mathf.FloorToInt(frame)]);
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

		public void ChangeRendererSpritesTo(Sprite sprite)
		{
			if (renderers != null)
				for (int i = 0; i < renderers.Length; i++)
					renderers[i].sprite = sprite;

			if (rotators != null)
				for (int i = 0; i < rotators.Length; i++)
					rotators[i].targetSprite = sprite;
		}

		public void StopLastFrameMode() =>
			lastFrameMode = true;
		public void ResetFrame() =>
			ResetFrame(false);
		public void ResetFrame(bool resetPause)
		{
			frame = 0f;
			if (resetPause)
			{
				lastFrameMode = false;
				pause = 0;
			}
		}

		public bool Paused => pause != 0;
		public bool LastFrameMode => lastFrameMode;

		protected EnvironmentController ec;

		[SerializeField]
		[Range(0.1f, 50f)]
		internal float speed = 5;

		[SerializeField]
		public Sprite[] animation;

		[SerializeField]
		internal SpriteRenderer[] renderers;

		[SerializeField]
		internal AnimatedSpriteRotator[] rotators;

		int pause = 0;

		bool lastFrameMode = false;

		float frame = 0f;
	}
}
