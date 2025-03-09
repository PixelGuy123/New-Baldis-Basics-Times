using BBTimes.CustomComponents;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SugarFlavoredZestyBar : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			audEat = GenericExtensions.FindResourceObject<ITM_ZestyBar>().audEat;
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


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
				mod.multiplier -= pm.ec.EnvironmentTimeScale * Time.deltaTime * 0.7f;
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
				mod.multiplier += pm.ec.EnvironmentTimeScale * Time.deltaTime * 0.7f;
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
