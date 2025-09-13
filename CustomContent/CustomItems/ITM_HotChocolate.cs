using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_HotChocolate : ITM_StaminaDrinkable, IItemPrefab
	{
		public void SetupPrefab()
		{
			var collider = gameObject.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = 3.75f;

			gameObject.layer = LayerStorage.ignoreRaycast;

			canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.name = "HotCanvas";
			canvas.transform.SetParent(transform);
			canvas.gameObject.SetActive(false);

			image = ObjectCreationExtensions.CreateImage(canvas, this.GetSprite(1f, "hotCanvas.png"));
			image.name = "HotImage";

			gaugeSprite = ItmObj.itemSpriteSmall;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			pm.GetCustomCam().SlideFOVAnimation(camFov, fovFactor, fovSmoothnessChange);

			StartCoroutine(FadeInImage());

			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

			foreach (var npc in pm.ec.Npcs)
				pm.plm.Entity.IgnoreEntity(npc.Entity, true);

			return base.Use(pm);
		}
		public override void Despawn() =>
			StartCoroutine(WaitForFade());

		IEnumerator FadeInImage()
		{
			image.color = new(1f, 1f, 1f, 0f);
			float t = 0;
			while (t < 1f)
			{
				t += pm.PlayerTimeScale * Time.deltaTime * 5f;
				image.color = new(1f, 1f, 1f, t);
				yield return null;
			}
			image.color = Color.white;
		}
		IEnumerator WaitForFade()
		{
			image.color = Color.white;
			float t = 1;
			while (t > 0f)
			{
				t -= pm.PlayerTimeScale * Time.deltaTime * 5f;
				image.color = new(1f, 1f, 1f, t);
				yield return null;
			}
			image.color = Color.clear;

			pm.GetCustomCam().ResetSlideFOVAnimation(camFov, fovSmoothnessChange);

			foreach (var npc in pm.ec.Npcs)
				pm.plm.Entity.IgnoreEntity(npc.Entity, false);

			base.Despawn();
		}

		void Update() =>
			transform.position = pm.transform.position;


		void OnTriggerStay(Collider other)
		{
			if (other.gameObject != pm.gameObject && other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<Entity>(out var e))
				e.AddForce(new((other.transform.position - pm.transform.position).normalized, hotnessForce, hotnessAcceleration));
		}

		readonly ValueModifier camFov = new();

		[SerializeField]
		internal float fovFactor = 7.5f, fovSmoothnessChange = 2.5f, canvasFadeSecs = 1.5f, hotnessForce = 10f, hotnessAcceleration = -6.5f;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal Image image;
	}
}
