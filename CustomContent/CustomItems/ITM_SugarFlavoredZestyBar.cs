using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SugarFlavoredZestyBar : Item
	{
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
			StartCoroutine(SugarEffect());
			return true;
		}

		IEnumerator SugarEffect()
		{
			var m = pm.GetMovementStatModifier();
			var mod = new ValueModifier(4f);
			m.AddModifier("staminaMax", mod);
			yield return null;
			pm.plm.AddStamina(pm.plm.staminaMax - pm.plm.stamina, true);
			float cooldown = 25f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			while (true)
			{
				mod.multiplier -= pm.ec.EnvironmentTimeScale * Time.deltaTime * 1.5f;
				pm.plm.stamina = Mathf.Min(pm.plm.stamina, pm.plm.staminaMax);
				if (mod.multiplier <= 0.5f)
				{
					break;
				}
				yield return null;
			}

			cooldown = 10f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			while (true)
			{
				mod.multiplier += pm.ec.EnvironmentTimeScale * Time.deltaTime * 1.5f;
				if (mod.multiplier >= 1f)
				{
					break;
				}
				yield return null;
			}
			m.RemoveModifier(mod);

			yield break;
		}

		[SerializeField]
		internal SoundObject audEat;
	}
}
