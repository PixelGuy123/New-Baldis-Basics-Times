using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Bell : Item, IEntityTrigger, IItemPrefab
	{
		readonly ValueModifier valMod = new(0f);

		public void SetupPrefab()
		{
			var storedSprites = this.GetSpriteSheet(2, 1, 25f, "bellWorld.png");
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]).AddSpriteHolder(out var rendererBell, 1f);

			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.zero;

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(1.5f, 2.5f, renderer.transform);

			audMan = gameObject.CreatePropagatedAudioManager(165, 200);
			audBell = this.GetSound("bell_bellnoise.wav", "Vfx_BEL_Ring", SoundType.Effect, Color.white);

			this.renderer = rendererBell;
			deactiveSprite = storedSprites[1];
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			owner = pm.gameObject;
			pm.RuleBreak("littering", 5f, 0.8f);

			entity.Initialize(pm.ec, pm.ec.CellFromPosition(pm.transform.position).FloorWorldPosition);
			ec = pm.ec;

			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (owner == other.gameObject || !active)
				return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				var e = other.GetComponent<Entity>();
				if (e && e.Grounded)
				{
					active = false;
					renderer.sprite = deactiveSprite;
					var baldi = ec.GetBaldi();
					if (baldi) 
					{
						baldi.ClearSoundLocations(); // Forcefully makes him follow the bell
						baldi.GetNPCContainer()?.AddLookerMod(valMod);
					}
					ec.MakeNoise(transform.position, noiseVal);
					audMan.PlaySingle(audBell);

					StartCoroutine(WaitForDespawn());
				}
			}
		}

		public void EntityTriggerStay(Collider other)
		{

		}

		public void EntityTriggerExit(Collider other)
		{
			if (owner == other.gameObject)
				owner = null; // left owner's 
		}

		IEnumerator WaitForDespawn()
		{
			var baldi = ec.GetBaldi();
			var cell = ec.CellFromPosition(transform.position);
			while (audMan.AnyAudioIsPlaying || 
				(baldi && (ec.CellFromPosition(baldi.transform.position) != cell
				&& baldi.soundLocations[noiseVal] == transform.position))) 

				yield return null;

			baldi?.GetNPCContainer().RemoveLookerMod(valMod);
			Destroy(gameObject);
			yield break;
		}

		bool active = true;
		GameObject owner;
		EnvironmentController ec;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audBell;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Sprite deactiveSprite;

		const int noiseVal = 112;
	}
}
