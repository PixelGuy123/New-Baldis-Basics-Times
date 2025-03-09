using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_GSoda : ITM_BSODA, IItemPrefab, IEntityTrigger
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

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm) =>
			base.Use(pm);
		

		[SerializeField]
		float baseSpeed = 30f;

		public new void EntityTriggerEnter(Collider other)
		{
			Entity component = other.GetComponent<Entity>();
			if ((!other.CompareTag("Player") || !launching) && component != null)
			{
				component.ExternalActivity.moveMods.Add(moveMod);
				activityMods.Add(component.ExternalActivity);
				speed = baseSpeed / (activityMods.Count + 1);
			}
		}

		public new void EntityTriggerExit(Collider other)
		{
			Entity component = other.GetComponent<Entity>();
			if (other.CompareTag("Player"))
				launching = false;
			
			if (component != null)
			{
				component.ExternalActivity.moveMods.Remove(moveMod);
				activityMods.Remove(component.ExternalActivity);
				speed = baseSpeed / (activityMods.Count + 1);
			}
		}

		public new void EntityTriggerStay(Collider other) { }
	}
}
