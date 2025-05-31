using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_InvisibilityController : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audUse = this.GetSound("longHighBeep.wav", "InvCon_Active", SoundType.Effect, Color.white);
			audDeuse = this.GetSound("longDownBeep.wav", "InvCon_Deactive", SoundType.Effect, Color.white);
			gaugeSprite = ItmObj.itemSpriteSmall;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (use)
			{
				Destroy(gameObject);
				return false;
			}
			use = true;
			this.pm = pm;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
			pm.plm.Entity.SetHidden(true);

			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, totalCooldown);

			StartCoroutine(Timer());

			return true;
		}

		void OnDestroy() => use = false;

		IEnumerator Timer()
		{
			float cooldown = totalCooldown;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				gauge.SetValue(totalCooldown, cooldown);
				yield return null;
			}
			gauge.Deactivate();
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDeuse);
			pm.plm.Entity.SetHidden(false);
			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		internal SoundObject audUse, audDeuse;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float totalCooldown = 10f;

		HudGauge gauge;

		static bool use = false;
	}
}
