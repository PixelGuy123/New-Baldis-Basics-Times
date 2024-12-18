using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.FrozenEvent
{
	public class SnowDrift : MonoBehaviour
	{
		void Start() =>
			moveMod.movementMultiplier = slowFactor;

		void OnTriggerEnter(Collider other)
		{
			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					affectedModifiers.Add(e.ExternalActivity);
					e.ExternalActivity.moveMods.Add(moveMod);
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					affectedModifiers.Remove(e.ExternalActivity);
					e.ExternalActivity.moveMods.Remove(moveMod);
				}
			}
		}

		void OnDestroy()
		{
			for (int i = 0; i < affectedModifiers.Count; i++)
				if (affectedModifiers[i])
					affectedModifiers[i].moveMods.Remove(moveMod);
		}

		readonly List<ActivityModifier> affectedModifiers = [];

		[SerializeField]
		[Range(0f, 1f)]
		internal float slowFactor = 0.6f;

		readonly MovementModifier moveMod = new(Vector3.zero, 1f);
	}
}
