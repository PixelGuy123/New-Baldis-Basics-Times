using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class PersistentBearTrap : EnvironmentObject
	{
		void OnTriggerEnter(Collider other)
		{
			if (active) return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var e = other.GetComponent<Entity>();
                if (e)
                {
					active = true;
					StartCoroutine(Trap(e));
                }
            }
		}

		IEnumerator Trap(Entity e)
		{
			audMan.PlaySingle(audCatch);
			renderer.sprite = sprClosed;
			e.ExternalActivity.moveMods.Add(moveMod);
			float cooldown = Random.Range(10f, 15f);
			while (cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			e.ExternalActivity.moveMods.Remove(moveMod);
			cooldown = Random.Range(30f, 60f);
			while (cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			renderer.sprite = sprOpen;
			active = false;

			yield break;
		}

		[SerializeField]
		internal Sprite sprOpen, sprClosed;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal SoundObject audCatch;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		bool active = false;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
