using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class Hallucinations : MonoBehaviour
	{
		public void AttachToPlayer(PlayerManager pm)
		{
			if (initialized) return;
			timeAlive = lifeTime;
			target = pm;
			ec = pm.ec;
			initialized = true;
			StartCoroutine(Hallucinating());
		}

		IEnumerator Hallucinating()
		{
			Color alpha = renderer.color;
			alpha.a = 0;
			renderer.color = alpha;
			audMan.QueueAudio(audLoop);
			audMan.SetLoop(true);
			audMan.maintainLoop = true;
			yield return null;

			while (true)
			{
				if (!target)
					Destroy(gameObject);

				transform.position = target.transform.position + new Vector3(Random.Range(-16f, 16f), 0f, Random.Range(-16f, 16f));
				audMan.PlaySingle(audSpawn);
				while (true)
				{
					alpha.a += ec.EnvironmentTimeScale * Time.deltaTime;
					if (alpha.a >= 1f)
					{
						alpha.a = 1f;
						break;
					}
					renderer.color = alpha;
					yield return null;
				}

				renderer.color = alpha;
				delayAround = delayAroundThePlayer;

				while (delayAround > 0f)
				{
					delayAround -= ec.EnvironmentTimeScale * Time.deltaTime;
					yield return null;
				}

				while (true)
				{
					alpha.a -= ec.EnvironmentTimeScale * Time.deltaTime;
					if (alpha.a <= 0f)
					{
						alpha.a = 0f;
						break;
					}
					renderer.color = alpha;
					yield return null;
				}
				renderer.color = alpha;

				if (timeAlive < 0f)
					Destroy(gameObject);

				float del = 1f;
				while (del > 0f)
				{
					del -= ec.EnvironmentTimeScale * Time.deltaTime;
					yield return null;
				}

				yield return null;
			}
		}

		void Update()
		{
			if (!initialized) return;

			timeAlive -= ec.EnvironmentTimeScale * Time.deltaTime;
		}

		EnvironmentController ec;
		PlayerManager target;
		bool initialized = false;
		float timeAlive, delayAround;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal float lifeTime = 45f, delayAroundThePlayer = 3f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audSpawn, audLoop;
	}
}
