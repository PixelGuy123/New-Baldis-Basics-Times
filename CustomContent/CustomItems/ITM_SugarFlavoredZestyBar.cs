using System.Collections;
using BBTimes.CustomComponents;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SugarFlavoredZestyBar : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audEat = GenericExtensions.FindResourceObject<ITM_ZestyBar>().audEat;
			gaugeSprite = ItmObj.itemSpriteSmall;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audEat);
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, lifeTime);
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

			cooldown = lifeTime;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				gauge.SetValue(lifeTime, cooldown);
				yield return null;
			}

			gauge.Deactivate();

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

		[SerializeField]
		internal float lifeTime = 10f;

		[SerializeField]
		internal Sprite gaugeSprite;

		HudGauge gauge;
	}
}
