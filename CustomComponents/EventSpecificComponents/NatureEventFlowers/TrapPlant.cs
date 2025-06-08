using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class TrapPlant : Plant
	{
		protected override void TriggerEnterNPC(NPC npc)
		{
			if (catched) return;

			base.TriggerEnterNPC(npc);
			Catch(npc.Navigator.Am);
		}
		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			if (catched) return;

			base.TriggerEnterPlayer(pm);
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, timeTrapped);

			Catch(pm.Am);
		}
		protected override void TriggerExitNPC(NPC npc)
		{
			base.TriggerExitNPC(npc);
			if (target == npc.Navigator.Am)
				DespawnEarlier();
		}
		protected override void TriggerExitPlayer(PlayerManager pm)
		{
			base.TriggerExitPlayer(pm);
			if (target == pm.Am)
				DespawnEarlier();

		}

		void DespawnEarlier()
		{
			if (catchCool != null)
				StopCoroutine(catchCool);
			target.moveMods.Remove(moveMod);
			Despawn(true);
		}

		void Catch(ActivityModifier actMod)
		{
			audMan.PlaySingle(audCatch);
			catched = true;
			target = actMod;
			renderer.sprite = sprCatch;
			catchCool = StartCoroutine(CatchCooldown(actMod));
		}

		IEnumerator CatchCooldown(ActivityModifier actMod)
		{
			float time = timeTrapped;
			actMod.moveMods.Add(moveMod);
			while (time > 0f)
			{
				time -= ec.EnvironmentTimeScale * Time.deltaTime;
				gauge?.SetValue(timeTrapped, time);
				yield return null;
			}
			actMod?.moveMods.Remove(moveMod);
			gauge?.Deactivate();
			Despawn(true);
		}

		ActivityModifier target;
		bool catched = false;
		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
		Coroutine catchCool;

		[SerializeField]
		internal SoundObject audCatch;

		[SerializeField]
		internal Sprite sprCatch, gaugeSprite;

		[SerializeField]
		internal float timeTrapped = 5f;

		HudGauge gauge;
	}
}
