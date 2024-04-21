using PixelInternalAPI.Components;
using PixelInternalAPI.Classes;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_BSED : Item
	{
		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDrink);
			this.pm = pm;
			pm.RuleBreak("Drinking", 1.5f);
			var comp = pm.GetComponent<PlayerAttributesComponent>();
			comp.StaminaMods.Add(sMod);
			StartCoroutine(Timer(comp));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp)
		{
			float cooldown = 15f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			comp.StaminaMods.Remove(sMod);

			Destroy(gameObject);

			yield break;
		}

		internal static SoundObject audDrink;

		readonly StaminaModifier sMod = new(1f, 2f, 0.5f);
	}
}
