using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_AaaahTomato : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.ignoreRaycast;
			// Create trigger area
			BoxCollider collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size = new Vector3(3f, 5f, 3f);
			collider.center = Vector3.up * 5f;

			// Visual setup
			var sprs = this.GetSpriteSheet(1, 2, 65f, "tomato_world.png");
			screamSpr = sprs[1];
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0])
				.AddSpriteHolder(out spriteRenderer, 1.21f);
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.zero;
			renderer.name = "TomatoRenderer";
			spriteRenderer.name = "Sprite";

			audMan = gameObject.CreatePropagatedAudioManager(95f, 115f);

			audScream = this.GetSound("TomScream.wav", "Vfx_AAAHTomato_Scream", SoundType.Effect, Color.red);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			pm.RuleBreak("littering", litteringTimer, 0.8f);
			ec = pm.ec;
			owner = pm.gameObject;
			transform.position = pm.ec.CellFromPosition(pm.transform.position).FloorWorldPosition;
			return true;
		}

		void OnTriggerEnter(Collider other)
		{
			if (triggered || owner == other.gameObject || !other.isTrigger) return;

			var npc = other.GetComponent<NPC>();
			if (npc != null && (npc.IsAPrincipal() || !npc.Navigator.Entity.Grounded))
				return;

			triggered = true;
			StartCoroutine(ScreamBehavior(other));
		}

		void OnTriggerExit(Collider other)
		{
			if (owner == other.gameObject)
				owner = null;
		}

		IEnumerator ScreamBehavior(Collider offender)
		{
			// Visual/Sound change
			spriteRenderer.sprite = screamSpr;
			audMan.PlaySingle(audScream);

			// Apply guilt effects
			if (offender.CompareTag("Player"))
			{
				var pm = offender.GetComponent<PlayerManager>();
				pm.RuleBreak("Bullying", guiltTimer);
			}
			else if (offender.CompareTag("NPC"))
			{
				var npc = offender.GetComponent<NPC>();
				npc.SetGuilt(guiltTimer, "Bullying");
			}

			// Summon authority
			ec.CallOutPrincipals(transform.position, 11.5f);

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}

		[SerializeField]
		private Sprite screamSpr;
		[SerializeField]
		private SoundObject audScream;
		[SerializeField]
		private SpriteRenderer spriteRenderer;
		[SerializeField]
		private PropagatedAudioManager audMan;
		[SerializeField]
		private float guiltTimer = 30f, litteringTimer = 2f;

		EnvironmentController ec;
		GameObject owner;
		bool triggered = false;
	}
}