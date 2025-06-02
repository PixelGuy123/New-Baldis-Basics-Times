using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ScienceTeacher
{
	public class SpeedingOrSlowingPotion : Potion
	{
		protected override void Despawned()
		{
			base.Despawned();
			while (_entityList.Count != 0)
			{
				_entityList[0].ExternalActivity.moveMods.Remove(slowMod);
				_entityList[0].ExternalActivity.moveMods.Remove(speedMod);
				_entityList.RemoveAt(0);
			}

			gauge?.Deactivate();
		}

		protected override void Initialize()
		{
			base.Initialize();
			speedMod = new(Vector3.zero, speedBuff);
			slowMod = new(Vector3.zero, speedNerf);
		}

		protected override void OnEntityEnter(Entity entity)
		{
			base.OnEntityEnter(entity);
			if (!_entityList.Contains(entity))
			{
				_entityList.Add(entity);
				bool buff = Random.value <= speedChance;

				audMan.PlaySingle(buff ? audSpeedBuff : audSpeedNerf);
				splashRenderer.sprite = buff ? sprFast : sprSlow;
				StartCoroutine(Timer(entity, buff ? speedMod : slowMod, entity.GetComponent<PlayerManager>()));
			}
		}

		IEnumerator Timer(Entity e, MovementModifier moveMod, PlayerManager pm)
		{
			bool hasPm = pm != null;
			e.ExternalActivity.moveMods.Add(moveMod);
			float timer = Random.Range(minEffectCooldown, maxEffectCooldown), ogTimer = timer;
			if (hasPm)
				gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, ogTimer);

			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				if (hasPm) gauge.SetValue(ogTimer, timer);
				yield return null;
			}

			if (hasPm) gauge.Deactivate();

			_entityList.Remove(e);
			e.ExternalActivity.moveMods.Remove(moveMod);
		}

		MovementModifier slowMod, speedMod;
		readonly List<Entity> _entityList = [];

		[SerializeField]
		[Range(0f, 1f)]
		internal float speedChance = 0.6f;

		[SerializeField]
		internal float speedBuff = 1.45f, speedNerf = 0.65f, minEffectCooldown = 5f, maxEffectCooldown = 10f;

		[SerializeField]
		internal SoundObject audSpeedBuff, audSpeedNerf;

		[SerializeField]
		internal Sprite sprSlow, sprFast;
	}
}
