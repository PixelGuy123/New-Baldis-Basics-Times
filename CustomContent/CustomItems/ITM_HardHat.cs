using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Plugin;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_HardHat : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			var canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "hardHatOverlay";
			ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(1f, "hardHatHud.png"));
			this.canvas = canvas;

			gaugeSprite = ItmObj.itemSpriteSmall;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			if (used)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			used = true;

			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, lifeTime);
			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

			StartCoroutine(Timer(pm.GetAttribute()));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp)
		{
			comp.AddAttribute(Storage.HARDHAT_ATTR_TAG);
			float cooldown = lifeTime;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				gauge.SetValue(lifeTime, cooldown);
				yield return null;
			}
			gauge.Deactivate();
			comp.RemoveAttribute(Storage.HARDHAT_ATTR_TAG);
			Destroy(gameObject);
			yield break;
		}

		void OnDestroy() => used = false;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal float lifeTime = 120f;
		[SerializeField]
		internal Sprite gaugeSprite;

		static bool used = false;

		HudGauge gauge;
	}
}
