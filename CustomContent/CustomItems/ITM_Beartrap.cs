﻿using BBTimes.CustomComponents;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.Extensions;

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
			renderer.transform.localPosition = Vector3.zero;

			this.renderer = trapRenderer;
			entity = gameObject.CreateEntity(1f, 1f, renderer.transform);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }

		// -------------------------------------------------------------------
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			entity.Initialize(pm.ec, pm.ec.CellFromPosition(pm.transform.position).FloorWorldPosition);
			pm.RuleBreak("littering", 5f, 0.8f);
			owner = pm.gameObject;

			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (other.gameObject == owner || active)
				return;

			if (other.CompareTag("NPC") || other.CompareTag("Player"))
			{
				var e = other.GetComponent<Entity>();
				if (e != null && e.Grounded)
				{
					active = true;
					target = e;
					target.ExternalActivity.moveMods.Add(moveMod);
					audMan.PlaySingle(audTrap);
					renderer.sprite = closedTrap;
				}
			}
		}

		public void EntityTriggerStay(Collider other)
		{
		}

		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == owner)
				owner = null;
		}

		private void Update()
		{
			if (!active) return;

			cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				target.ExternalActivity.moveMods.Remove(moveMod);
				Destroy(gameObject);
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
		internal Sprite closedTrap;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		bool active = false;

		float cooldown = 15f;

		Entity target;

		GameObject owner;
	}
}
