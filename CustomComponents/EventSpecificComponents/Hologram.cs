using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public class Hologram : MonoBehaviour
	{
		public void Initialize(SpriteRenderer renderer, float spawnDelay, EnvironmentController ec, float transparency)
		{
			targetRenderer = renderer;
			this.ec = ec;
			records = [];
			records2 = [];
			block = new();
			started = true;

			var color = this.renderer.color;
			color.a = Mathf.Clamp01(transparency);
			this.renderer.color = color;

			this.renderer.sprite = renderer.sprite;
			transform.position = renderer.transform.position;

			StartCoroutine(StartDelay(spawnDelay));
		}

		IEnumerator StartDelay(float delay)
		{
			while (delay > 0f)
			{
				delay -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			initialized = true;

			yield break;
		}
		void Update()
		{
			if (!started || Time.timeScale == 0) return;

			if (!targetRenderer)
			{
				if (records.Count == 0 || records2.Count == 0)
					Destroy(gameObject);
			}
			else
			{
				targetRenderer.GetPropertyBlock(block);
				records.Enqueue(new(targetRenderer.transform.position, targetRenderer.sprite));
				records2.Enqueue(new(block.GetFloat("_SpriteRotation"), targetRenderer.transform.localScale));
			}
			if (initialized && records.Count != 0 && records2.Count != 0)
			{
				var k = records.Dequeue();
				renderer.sprite = k.Value;
				transform.position = k.Key;
				var k2 = records2.Dequeue();
				transform.localScale = k2.Value;
				renderer.SetSpriteRotation(k2.Key);
			}
		}

		Queue<KeyValuePair<Vector3, Sprite>> records;
		Queue<KeyValuePair<float, Vector3>> records2;

		bool initialized = false, started = false;
		SpriteRenderer targetRenderer;
		EnvironmentController ec;
		MaterialPropertyBlock block;

		[SerializeField]
		internal SpriteRenderer renderer;
	}
}
