using UnityEngine;
using BBTimes.Extensions;
using BBTimes.CustomComponents;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using BBTimes.Manager;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_ToiletPaper : Item, IEntityTrigger, IItemPrefab
	{

		public void SetupPrefab()
		{
			var sprs = this.GetSpriteSheet(4, 2, 30f, "toiletpaper.png");
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(sprs[0]);
			var rendererBase = renderer.transform;

			animComp = renderer.gameObject.AddComponent<AnimationComponent>();
			animComp.animation = sprs;
			animComp.speed = 14.5f;
			animComp.renderers = [renderer];

			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(2f, 2f, rendererBase);
			entity.SetGrounded(false);

			audMan = gameObject.CreatePropagatedAudioManager(95, 115);
			audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
			this.renderer = rendererBase;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			owner = pm.gameObject;
			this.pm = pm;
			dir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;

			animComp.Initialize(pm.ec);
			entity.Initialize(pm.ec, pm.transform.position);
			ec = pm.ec;
			audMan.PlaySingle(audThrow);
			height = entity.BaseHeight;

			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (other.gameObject == owner) return;
			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					dir = (other.transform.position - transform.position).normalized;
					e.AddForce(new(dir, 35f, -20f));

					speed *= 0.3f;
					velocityY = 0.7f * heightIncreaseFactor;
					heightIncreaseFactor *= 0.97f;
					heightDecreaseFactor *= 1.03f;
					audMan.PlaySingle(audHit);
					if (other.gameObject != pm.gameObject)
						pm.RuleBreak("Bullying", 2f, 0.8f);
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

		void Update()
		{
			entity.UpdateInternalMovement(dir * ec.EnvironmentTimeScale * speed);
			velocityY -= heightDecreaseFactor * ec.EnvironmentTimeScale * Time.deltaTime;
			height += velocityY * 0.1f * Time.timeScale;
			
			if (height > 4f)
			{
				height = 4f;
				velocityY = 0f;
			}

			renderer.transform.localPosition = Vector3.up * height;
			if (height <= -4.9f)
				Destroy(gameObject);
		}

		GameObject owner;
		EnvironmentController ec;
		Vector3 dir;
		float speed = 35f;
		float height = 0f, heightDecreaseFactor = 1.015f, heightIncreaseFactor = 0.92f, velocityY = 0.03f;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal SoundObject audThrow, audHit;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal Entity entity;
	}
}