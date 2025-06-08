using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class SpeedChangingFlower : Plant
	{
		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			base.TriggerEnterPlayer(pm);
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, speedTime);
			SpeedSomeone(pm.Am);
		}
		protected override void TriggerEnterNPC(NPC npc)
		{
			base.TriggerEnterNPC(npc);
			SpeedSomeone(npc.Navigator.Am);
		}

		void SpeedSomeone(ActivityModifier am)
		{
			audMan.PlaySingle(audAffect);
			Despawn(true, false);
			StartCoroutine(SpeedUp(am));
		}

		IEnumerator SpeedUp(ActivityModifier actMod)
		{
			moveMod.movementMultiplier = moveMultiplier;
			actMod.moveMods.Add(moveMod);
			float timer = speedTime;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				gauge?.SetValue(speedTime, timer);
				yield return null;
			}
			gauge?.Deactivate();
			actMod?.moveMods.Remove(moveMod);
			Destroy(gameObject);
		}

		readonly MovementModifier moveMod = new(Vector3.zero, 1);

		[SerializeField]
		internal float speedTime = 10f;

		[SerializeField]
		internal float moveMultiplier = 1.2f;

		[SerializeField]
		internal SoundObject audAffect;

		[SerializeField]
		internal Sprite gaugeSprite;

		HudGauge gauge;
	}
}
