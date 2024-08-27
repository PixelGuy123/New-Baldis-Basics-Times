using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Bell : Item, IEntityTrigger, IItemPrefab
	{

		public void SetupPrefab()
		{
			var storedSprites = this.GetSpriteSheet(2, 1, 25f, "bellWorld.png");
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]).AddSpriteHolder(-4f);
			var rendererBase = renderer.transform.parent;
			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(1.5f, 2.5f, rendererBase);

			audMan = gameObject.CreatePropagatedAudioManager(165, 200);
			audBell = this.GetSound("bell_bellnoise.wav", "Vfx_BEL_Ring", SoundType.Voice, Color.white);

			this.renderer = renderer;
			deactiveSprite = storedSprites[1];
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			owner = pm.gameObject;
			pm.RuleBreak("littering", 5f, 0.8f);

			gameObject.SetActive(true);

			entity.Initialize(pm.ec, pm.transform.position);
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
			while (audMan.AnyAudioIsPlaying) yield return null;

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
