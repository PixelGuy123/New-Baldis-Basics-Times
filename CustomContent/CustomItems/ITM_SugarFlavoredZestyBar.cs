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

			float boostDuration = lifeTime * 0.4f;
			float decayDuration = lifeTime * 0.6f;
			float timer = 0f;

			// Boost phase
			while (timer < boostDuration)
			{
				timer += pm.PlayerTimeScale * Time.deltaTime;
				gauge.SetValue(lifeTime, lifeTime - timer);
				yield return null;
			}

			// Decay phase
			float decayTimer = 0f;
			while (decayTimer < decayDuration)
			{
				decayTimer += pm.PlayerTimeScale * Time.deltaTime;
				// Gradually reduce multiplier from 4f to 0.5f over decayDuration
				mod.multiplier = Mathf.Lerp(4f, 0.5f, decayTimer / decayDuration);
				pm.plm.stamina = Mathf.Min(pm.plm.stamina, pm.plm.staminaMax);
				gauge.SetValue(lifeTime, lifeTime - (decayTimer + timer));
				yield return null;
			}

			gauge.Deactivate();

			// Restore multiplier back to 1f smoothly
			while (mod.multiplier < 1f)
			{
				mod.multiplier += pm.PlayerTimeScale * Time.deltaTime * 0.7f;
				if (mod.multiplier >= 1f)
				{
					mod.multiplier = 1f;
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
		internal float lifeTime = 50f;

		[SerializeField]
		internal Sprite gaugeSprite;

		HudGauge gauge;
	}
}
