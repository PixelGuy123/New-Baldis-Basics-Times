using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Gum : Item, IEntityTrigger, IItemPrefab
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			audMan = gameObject.CreatePropagatedAudioManager(55, 75);
			aud_fly = GenericExtensions.FindResourceObjectByName<SoundObject>("Ben_Gum_Whoosh");
			aud_splash = GenericExtensions.FindResourceObjectByName<SoundObject>("Ben_Splat");
			aud_spit = this.GetSound("gum_spit.wav", "Vfx_GUM_spit", SoundType.Effect, new Color(1, 0.2039f, 0.8863f));

			rendererBase = Instantiate(Resources.FindObjectsOfTypeAll<Gum>()[0].transform.Find("RendererBase"));
			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;

			flyingSprite = rendererBase.Find("Sprite_Flying");
			groundedSprite = rendererBase.Find("Sprite_Grounded");

			entity = gameObject.CreateEntity(1f, 1f, out var collider, out _, rendererBase).SetEntityCollisionLayerMask(LayerStorage.gumCollisionMask);
			entity.SetGrounded(false);
			collider.height = 4;

		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true); // do not forget this lol

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(aud_spit);
			audMan.QueueAudio(aud_fly);
			audMan.SetLoop(true);
			owner = pm.gameObject;
			transform.position = pm.transform.position;
			transform.rotation = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.rotation;
			this.pm = pm;
			ec = pm.ec;
			entity.Initialize(pm.ec, transform.position);

			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				if (flying && hit.transform.gameObject.layer != 2)
				{
					flying = false;
					entity.SetFrozen(true);

					transform.rotation = Quaternion.LookRotation(hit.normal * -1f, Vector3.up);

					audMan.FlushQueue(true);
					audMan.PlaySingle(aud_splash);
					HitSomething(hit);

					StartCoroutine(Timer(null));
				}
			}; // lambda functions :O

			return true;
		}

#pragma warning disable IDE0060
		void HitSomething(RaycastHit hit) { } // Easy way to patch the gum when hitting a wall (BB+ Animations lol)
#pragma warning restore IDE0060

		public void EntityTriggerEnter(Collider other, bool validCollision)
		{
			if (!validCollision || other.gameObject == owner || !flying) return;

			if (other.isTrigger && other.CompareTag("NPC")) // Might affect players soon. But I don't plan on doing that now
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					e.ExternalActivity.moveMods.Add(moveMod);
					flying = false;
					flyingSprite.gameObject.SetActive(false);
					groundedSprite.gameObject.SetActive(true);
					audMan.FlushQueue(true);
					audMan.PlaySingle(aud_splash);

					transform.SetParent(other.transform);
					transform.localPosition = Vector3.zero;

					pm.RuleBreak("Bullying", 2f);

					StartCoroutine(Timer(e));

					entity.SetFrozen(true);
				}
			}
		}

		public void EntityTriggerStay(Collider other, bool validCollision) { }
		public void EntityTriggerExit(Collider other, bool validCollision) { }

		private IEnumerator Timer(Entity target)
		{
			float time = 15f;
			while (time > 0f)
			{
				time -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}
			target?.ExternalActivity.moveMods.Remove(moveMod);
			Destroy(gameObject);
			yield break;
		}

		void Update()
		{
			if (flying)
				entity.UpdateInternalMovement(transform.forward * speed * ec.EnvironmentTimeScale);
		}



		[SerializeField]
		internal Entity entity;

		GameObject owner;

		[SerializeField]
		internal Transform flyingSprite, groundedSprite, rendererBase;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		public SoundObject aud_fly, aud_splash, aud_spit; // If any mod wanna change this I guess

		bool flying = true;

		EnvironmentController ec;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.05f);

		const float speed = 30f;
	}
}
