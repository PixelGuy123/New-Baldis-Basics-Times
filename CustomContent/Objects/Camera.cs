using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;

namespace BBTimes.CustomContent.Objects
{
	public class SecurityCamera : EnvironmentObject
	{
		public void Setup(List<Direction> dirs, int maximumDistance)
		{
			nextDirections = dirs;
			maxDistance = maximumDistance;
			basePos = ec.CellFromPosition(transform.position).position;
			UpdateVision();
		}

		void Update()
		{
			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				cooldown = Random.Range(minTurnCool, maxTurnCool);
				dirIndex = ++dirIndex % nextDirections.Count;
				UpdateVision();
			}
		}

		void UpdateVision()
		{
			while (indicators.Count > 0)
			{
				Destroy(indicators[0].gameObject);
				indicators.RemoveAt(0);
			}

			transform.rotation = Quaternion.Euler(0f, nextDirections[dirIndex].ToDegrees(), 0f);
			IntVector2 pos = basePos;
			int size = 0;
			for (int i = 0; i < maxDistance; i++)
			{
				pos += nextDirections[dirIndex].ToIntVector2();
				var cell = ec.CellFromPosition(pos);

				var indicator = Instantiate(visionIndicatorPre, transform, true);
				indicator.transform.position = cell.FloorWorldPosition + Vector3.up * 0.1f;
				indicator.transform.rotation = Quaternion.Euler(90f, nextDirections[dirIndex].ToDegrees(), 0f);
				indicator.color = Color.blue;
				indicators.Add(indicator);

				size++;

				if (cell.HasWallInDirection(nextDirections[dirIndex]))
					break;
			}

			collider.size = new Vector3(3f, 10f, size * 9f);
			collider.center = new Vector3(0f, 1f, size * 6.7f);
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				indicators.Do(x => x.color = Color.red);
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				
			}
		}

		void SetIndicatorsToColor(Color color) =>
			indicators.Do(x => x.color = Color.blue);

		List<Direction> nextDirections;

		int dirIndex = 0, maxDistance;

		IntVector2 basePos;

		float cooldown = Random.Range(minTurnCool, maxTurnCool);

		const float maxTurnCool = 30f, minTurnCool = 15f;

		[SerializeField]
		internal SpriteRenderer visionIndicatorPre;

		[SerializeField]
		internal BoxCollider collider;

		readonly List<SpriteRenderer> indicators = [];
	}
}
