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
					StartCoroutine(Trap(e, other.GetComponent<PlayerManager>()));
				}
			}
		}

		IEnumerator Trap(Entity e, PlayerManager pm)
		{
			audMan.PlaySingle(audCatch);
			renderer.sprite = sprClosed;
			e.ExternalActivity.moveMods.Add(moveMod);

			float cooldown = Random.Range(minTrapTime, maxTrapTime), ogCooldown = cooldown;
			if (pm)
				gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, cooldown);

			while (cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				gauge?.SetValue(cooldown, ogCooldown);
				yield return null;
			}
			e.ExternalActivity.moveMods.Remove(moveMod);
			gauge?.Deactivate();
			cooldown = Random.Range(minRechargeTime, maxRechargeTime);
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

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float minTrapTime = 5f, maxTrapTime = 10f, minRechargeTime = 30f, maxRechargeTime = 60f;

		HudGauge gauge;

		bool active = false;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
