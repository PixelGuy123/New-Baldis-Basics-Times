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
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDrink);
			this.pm = pm;
			pm.RuleBreak("Drinking", 1.5f);
			var comp = pm.GetMovementStatModifier();

			stamMax = new(staminaMaxMod);
			stamRise = new(staminaRiseMod);
			stamDrop = new(staminaDropMod);

			comp.AddModifier("staminaMax", stamMax);
			comp.AddModifier("staminaRise", stamRise);
			comp.AddModifier("staminaDrop", stamDrop);

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

			comp.RemoveModifier(stamMax);
			comp.RemoveModifier(stamRise);
			comp.RemoveModifier(stamDrop);

			Destroy(gameObject);

			yield break;
		}

		internal void SetMod(float staminamax, float staminarise, float staminadrop) 
		{
			staminaMaxMod = staminamax;
			staminaRiseMod = staminarise;
			staminaDropMod = staminadrop;
		}

		ValueModifier stamMax, stamRise, stamDrop;

		[SerializeField]
		public float cooldown = 10f;

		[SerializeField]
		float staminaMaxMod = 1f, staminaRiseMod = 1f, staminaDropMod = 1f;

		[SerializeField]
		internal SoundObject audDrink;

		[SerializeField]
		internal string attribute;
	}
}
