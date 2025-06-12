using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_GoodGrades : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			// Set up trigger area
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size = new Vector3(3f, 5f, 3f);
			collider.center = Vector3.up * 5f;
			gameObject.layer = LayerStorage.ignoreRaycast; // Make sure to not affect raycasting at all

			// Create sprite display
			var sprs = this.GetSpriteSheet(2, 1, 80f, "world_grade.png");
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0])
				.AddSpriteHolder(out spriteRenderer, 1.2f);
			deadSprite = sprs[1];
			renderer.name = "GradeRenderer";
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.zero;
			spriteRenderer.name = "Sprite";

			audMan = gameObject.CreatePropagatedAudioManager(45f, 65f);
			deathSound = this.GetSound("gradeDie.wav", "Vfx_GoodGrades_Die", SoundType.Effect, Color.white);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			owner = pm.gameObject;
			pm.RuleBreak("littering", litteringTimer, 0.8f);
			transform.position = pm.ec.CellFromPosition(pm.transform.position).FloorWorldPosition;
			return true;
		}

		void OnTriggerEnter(Collider other)
		{
			if (triggered || !other.isTrigger || other.gameObject == owner) return;

			var npc = other.GetComponent<NPC>();

			if (npc && npc is Baldi baldo)
			{
				baldo.Praise(praiseTimer);
				Destroy(gameObject);
			}
			else
			{
				var e = other.GetComponent<Entity>();
				if (e && e.Grounded && (other.CompareTag("Player") || other.CompareTag("NPC")))
				{
					triggered = true;
					StartCoroutine(DeathSequence());
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.gameObject == owner)
				owner = null;
		}

		IEnumerator DeathSequence()
		{
			spriteRenderer.sprite = deadSprite;
			audMan.PlaySingle(deathSound);

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}

		[SerializeField]
		private Sprite deadSprite;
		[SerializeField]
		private SoundObject deathSound;
		[SerializeField]
		private SpriteRenderer spriteRenderer;
		[SerializeField]
		private PropagatedAudioManager audMan;
		[SerializeField]
		readonly float praiseTimer = 10f, litteringTimer = 2f;

		GameObject owner;

		bool triggered = false;
	}
}