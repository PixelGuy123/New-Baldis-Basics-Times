using System.Collections;
using UnityEngine;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Components;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_StaminaDrinkable : Item
	{
		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDrink);
			this.pm = pm;
			pm.RuleBreak("Drinking", 1.5f);
			var comp = pm.GetMovementStatModifier();

			comp.AddModifier("staminaMax", staminaMaxMod);
			comp.AddModifier("staminaRise", staminaRiseMod);
			comp.AddModifier("staminaDrop", staminaDropMod);

			StartCoroutine(Timer(comp));

			return true;
		}

		IEnumerator Timer(PlayerMovementStatModifier comp)
		{
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}

			comp.RemoveModifier(staminaMaxMod);
			comp.RemoveModifier(staminaRiseMod);
			comp.RemoveModifier(staminaDropMod);

			Destroy(gameObject);

			yield break;
		}

		internal void SetMod(float staminamax, float staminarise, float staminadrop) 
		{
			staminaMaxMod.multiplier = staminamax;
			staminaRiseMod.multiplier = staminarise;
			staminaDropMod.multiplier = staminadrop;
		}

		[SerializeField]
		public float cooldown = 10f;

		[SerializeField]
		readonly ValueModifier staminaMaxMod = new(1f), staminaRiseMod = new(1f), staminaDropMod = new(1f);

		[SerializeField]
		internal SoundObject audDrink;

		[SerializeField]
		internal string attribute;
	}
}
