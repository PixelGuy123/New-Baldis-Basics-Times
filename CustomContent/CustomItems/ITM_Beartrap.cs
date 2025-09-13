using BBTimes.CustomComponents;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Beartrap : Item, IEntityTrigger, IItemPrefab
	{
		public void SetupPrefab()
		{

			audMan = gameObject.CreatePropagatedAudioManager(75f, 105f);
			audTrap = BBTimesManager.man.Get<SoundObject>("BeartrapCatch");
			var trapSprs = BBTimesManager.man.Get<Sprite[]>("Beartrap");
			closedTrap = trapSprs[0];

			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(trapSprs[1]).AddSpriteHolder(out var trapRenderer, 1f);
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.down * 4.75f;

			this.renderer = trapRenderer;
			entity = gameObject.CreateEntity(1f, 1f, renderer.transform);

			gaugeSprite = ItmObj.itemSpriteSmall;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }

		// -------------------------------------------------------------------
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			ec = pm.ec;
			entity.Initialize(pm.ec, pm.ec.CellFromPosition(pm.transform.position).FloorWorldPosition);
			pm.RuleBreak("littering", 5f, 0.8f);
			owner = pm.gameObject;
			cooldown = maxCooldown;

			return true;
		}

		public void EntityTriggerEnter(Collider other, bool validCollision)
		{
			if (!validCollision || other.gameObject == owner || active)
				return;

			caughtPlayer = other.CompareTag("Player");

			if (other.CompareTag("NPC") || caughtPlayer)
			{
				var e = other.GetComponent<Entity>();
				if (e != null && e.Grounded)
				{
					active = true;
					target = e;
					target.ExternalActivity.moveMods.Add(moveMod);
					audMan.PlaySingle(audTrap);
					renderer.sprite = closedTrap;

					if (caughtPlayer)
						gauge = Singleton<CoreGameManager>.Instance.GetHud(
							other.GetComponent<PlayerManager>().playerNumber
							).gaugeManager.ActivateNewGauge(gaugeSprite, maxCooldown);
				}
			}
		}

		public void EntityTriggerStay(Collider other, bool validCollision)
		{
		}

		public void EntityTriggerExit(Collider other, bool validCollision)
		{
			if (validCollision && other.gameObject == owner)
				owner = null;
		}

		private void Update()
		{
			if (!active) return;

			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (caughtPlayer)
				gauge.SetValue(maxCooldown, cooldown);
			if (cooldown < 0f)
			{
				target.ExternalActivity.moveMods.Remove(moveMod);
				Destroy(gameObject);
				if (caughtPlayer)
					gauge.Deactivate();
			}
		}

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SoundObject audTrap;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Sprite closedTrap, gaugeSprite;

		[SerializeField]
		internal float maxCooldown = 15f;

		EnvironmentController ec;
		HudGauge gauge;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		bool active = false, caughtPlayer = false;

		float cooldown = 15f;

		Entity target;

		GameObject owner;
	}
}
