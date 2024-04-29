using PixelInternalAPI.Components;
using PixelInternalAPI.Classes;
using System.Collections;
using UnityEngine;

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
			var comp = pm.GetComponent<PlayerAttributesComponent>();
			sMod = new(staminaMaxMod, staminaRiseMod, staminaDropMod);
			comp.StaminaMods.Add(sMod);
			if (attribute != null)
				comp.AddAttribute(attribute);

			StartCoroutine(Timer(comp));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp)
		{
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			comp.StaminaMods.Remove(sMod);
			comp.RemoveAttribute(attribute);

			Destroy(gameObject);

			yield break;
		}

		internal void SetMod(float staminamax, float staminarise, float staminadrop) 
		{
			staminaMaxMod = staminamax;
			staminaRiseMod = staminarise;
			staminaDropMod = staminadrop;
		}

		StaminaModifier sMod;

		[SerializeField]
		internal SoundObject audDrink;

		[SerializeField]
		internal float staminaMaxMod = 1f, staminaRiseMod = 1f, staminaDropMod = 1f, cooldown = 15f;

		[SerializeField]
		internal string attribute;
	}
}
