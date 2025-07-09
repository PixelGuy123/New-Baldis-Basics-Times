using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_GSoda : ITM_BSODA, IItemPrefab
	{
		public void SetupPrefab()
		{
			var nbsoda = GetComponent<ITM_BSODA>();

			spriteRenderer = nbsoda.spriteRenderer;
			sound = nbsoda.sound;
			entity = nbsoda.entity;
			time = nbsoda.time;
			moveMod = nbsoda.moveMod;
			baseSpeed = 30f;
			speed = baseSpeed;

			spriteRenderer.sprite = this.GetSprite(spriteRenderer.sprite.pixelsPerUnit, "GrapeSpray.png");
			Destroy(transform.Find("RendereBase").Find("Particles").gameObject);

			Destroy(nbsoda);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		[SerializeField]
		float baseSpeed = 30f;

		void LateUpdate()
		{
			speed = baseSpeed / (activityMods.Count + 1);
		}
	}
}
