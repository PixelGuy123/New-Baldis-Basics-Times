using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections;
using UnityEngine;
using PixelInternalAPI.Extensions;

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
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
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

			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

			StartCoroutine(Timer(pm.GetAttribute()));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp)
		{
			comp.AddAttribute("protectedhead");
			float cooldown = lifeTime;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}
			comp.RemoveAttribute("protectedhead");
			Destroy(gameObject);
			yield break;
		}

		void OnDestroy() => used = false;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal float lifeTime = 120f;

		static bool used = false;
	}
}
