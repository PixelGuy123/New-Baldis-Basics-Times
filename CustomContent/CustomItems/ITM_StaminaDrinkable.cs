using System.Collections;
using UnityEngine;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Components;
using BBTimes.Extensions;
using BBTimes.CustomComponents;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_StaminaDrinkable : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDrink);
			this.pm = pm;
			pm.RuleBreak("Drinking", 1.5f);
			var comp = pm.GetMovementStatModifier();
			var mod = pm.GetAttribute();

			if (!string.IsNullOrEmpty(attribute))
				mod.AddAttribute(attribute);

			stamMax = new(staminaMaxMod);
			stamRise = new(staminaRiseMod);
			stamDrop = new(staminaDropMod);

			comp.AddModifier("staminaMax", stamMax);
			comp.AddModifier("staminaRise", stamRise);
			comp.AddModifier("staminaDrop", stamDrop);

			StartCoroutine(Timer(pm.plm, comp, mod));

			return true;
		}

		IEnumerator Timer(PlayerMovement plm, PlayerMovementStatModifier comp, PlayerAttributesComponent mod)
		{
			
			while (cooldown > 0f)
			{
				plm.AddStamina(staminaIncreasePerTime, true);
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}

			comp.RemoveModifier(stamMax);
			comp.RemoveModifier(stamRise);
			comp.RemoveModifier(stamDrop);
			mod.RemoveAttribute(attribute);

			Destroy(gameObject);

			yield break;
		}

		internal void SetMod(float staminamax, float staminarise, float staminadrop) 
		{
			staminaMaxMod = staminamax;
			staminaRiseMod = staminarise;
			staminaDropMod = staminadrop;
		}

		internal void SetStaminaIncrease(float increase) =>
			staminaIncreasePerTime = Mathf.Max(0, increase);

		ValueModifier stamMax, stamRise, stamDrop;

		[SerializeField]
		public float cooldown = 10f;

		[SerializeField]
		float staminaMaxMod = 1f, staminaRiseMod = 1f, staminaDropMod = 1f, staminaIncreasePerTime = 0f;

		[SerializeField]
		internal SoundObject audDrink;

		[SerializeField]
		internal string attribute;
	}
}
