using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Hammer : Item, IItemPrefab, IEntityTrigger
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			item = ItmObj.itemType;

			var sprs = this.GetSpriteSheet(7, 1, 25f, "floatingHammer.png");
			animComp = gameObject.AddComponent<AnimationComponent>();
			animComp.speed = 11.5f;
			animComp.animation = sprs;

			var hammer = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0]).AddSpriteHolder(out var hammerRenderer, 0f);// Placeholder sprite
			hammer.name = "Hammer";
			hammerRenderer.name = "HammerVisual";
			hammer.transform.SetParent(transform);
			hammer.transform.localPosition = Vector3.zero;

			animComp.renderers = [hammerRenderer];

			renderer = hammer.transform;

			entity = gameObject.CreateEntity(2f, 2f, hammer.transform);
			entity.SetGrounded(false);
			((CapsuleCollider)entity.collider).height = 10f;

			audMan = gameObject.CreatePropagatedAudioManager(86f, 125f);
			audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var raycastHit, pm.pc.reach, LayerStorage.windowLayer, QueryTriggerInteraction.Collide) && raycastHit.transform.CompareTag("Window"))
			{
				var w = raycastHit.transform.GetComponent<Window>();
				bool broken = false;
				if (w)
				{
					w.Break(true);
					broken = !raycastHit.transform.GetComponent<CustomWindowComponent>()?.unbreakable ?? true;
					if (broken)
						pm.RuleBreak("breakingproperty", 3f, 0.15f);
				}
				Destroy(gameObject);
				return broken;
			}

			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach, acceptorLayer)) // Npc layer I guess? Not sure, it was from the scissors
			{
				IItemAcceptor component = hit.transform.GetComponent<IItemAcceptor>();
				if (component != null && component.ItemFits(item))
				{
					Destroy(gameObject);
					component.InsertItem(pm, pm.ec);
					return true;
				}
			}

			ThrowHammer(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.ec, 38f, 6f, pm.plm.Entity.InternalHeight, pm.gameObject);

			return true;
		}

		public void ThrowHammer(Vector3 position, Vector3 direction, EnvironmentController ec, float throwSpeed, float throwUpperForce, float height = 5f, GameObject owner = null)
		{
			entity.Initialize(ec, position);
			this.direction = direction;
			entity.OnEntityMoveInitialCollision += (hit) => this.direction = Vector3.Reflect(this.direction, hit.normal);
			this.ec = ec;
			speed = throwSpeed;
			verticalSpeed = throwUpperForce * slownessConstant;
			this.height = height - heightForcedOffset;
			this.owner = owner;
			audMan.PlaySingle(audThrow);
			animComp.Initialize(ec);

			initialized = true;
		}

		void Update()
		{
			if (!initialized)
				return;

			entity.UpdateInternalMovement(direction * speed);

			verticalSpeed += gravityConstant * Time.deltaTime * ec.EnvironmentTimeScale * slownessConstant;
			height += ec.EnvironmentTimeScale * verticalSpeed * slownessConstant;

			float limit = heightLimit - heightForcedOffset;

			if (height >= limit)
			{
				height = limit;
				verticalSpeed = 0f;
			}
			renderer.transform.localPosition = Vector3.up * height;

			if (height <= -heightForcedOffset)
				Destroy(gameObject);
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (!initialized || other.gameObject == owner) return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					e.Squish(15f);
					audMan.PlaySingle(audHit);
					verticalSpeed = verticalSpeedGainOverHit;
				}
			}
		}

		public void EntityTriggerStay(Collider other) { }

		public void EntityTriggerExit(Collider other)
		{
			if (initialized && other.gameObject == owner)
				owner = null;
		}



		[SerializeField]
		internal Items item;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal SoundObject audThrow, audHit;

		[SerializeField]
		internal LayerMask acceptorLayer = 131072;

		[SerializeField]
		internal float gravityConstant = -4f, verticalSpeedGainOverHit = 5f, heightLimit = 9f;

		GameObject owner;
		bool initialized = false;
		float height = 5f, speed = 1f, verticalSpeed = 1f;
		Vector3 direction;
		EnvironmentController ec;

		const float slownessConstant = 0.15f, heightForcedOffset = 5f;
	}
}
