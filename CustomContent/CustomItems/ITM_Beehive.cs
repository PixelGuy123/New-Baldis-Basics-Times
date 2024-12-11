using BBTimes.Extensions;
using BBTimes.CustomComponents;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
    public class ITM_Beehive : Item, IEntityTrigger, IItemPrefab
	{
		public void SetupPrefab()
		{
			var spr = this.GetSprite(25f, "bees.png");
			var rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(spr);
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;
			rendererBase.gameObject.SetActive(true);

			gameObject.layer = LayerStorage.standardEntities;
			entity = gameObject.CreateEntity(2f, 2f, rendererBase.transform);
			entity.SetGrounded(false);

			audMan = gameObject.CreatePropagatedAudioManager(75, 105);
			stungAudMan = gameObject.CreatePropagatedAudioManager(95, 125);
			audBees = this.GetSound("bees.wav", "Beehive_bee", SoundType.Effect, Color.white);
			audStung = this.GetSound("poke.wav", "Beehive_Sting", SoundType.Effect, Color.white);

			renderer = rendererBase;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }







		// Prefab Setup Above^^
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			target = pm.gameObject;
			Setup(pm.ec, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.transform.position);

			return true;
		}

		public void Setup(EnvironmentController ec, Vector3 direction, Vector3 pos)
		{
			entity.Initialize(ec, pos);
			this.ec = ec;
			dir = direction;
			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audBees);
			entity.OnEntityMoveInitialCollision += hit => dir = Vector3.Reflect(dir, hit.normal);
		}

		void Update()
		{
			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (lifeTime < 0f)
			{
				Destroy(gameObject);
				return;
			}
			
			if (hasHit)
			{
				if (Time.timeScale != 0f)
					renderer.transform.localPosition = Random.insideUnitSphere * 3f;
				if (!e)
				{
					Destroy(gameObject);
					return;
				}
				entity.UpdateInternalMovement(Vector3.zero);
				entity.Teleport(((MonoBehaviour)e).transform.position);
				stingSecondsDelay -= ec.EnvironmentTimeScale * Time.deltaTime;
				if (stingSecondsDelay <= 0f)
				{
					stingSecondsDelay += Random.Range(minStingSecondDelayAddition, maxStingSecondDelayAddition);
					stungAudMan.PlaySingle(audStung);
					float force = Random.Range(minStungForce, maxStungForce);
					e.AddForce(new((dir + Random.insideUnitSphere).normalized, force, -force * 0.8f));
				}

				return;
			}
			entity.UpdateInternalMovement(dir * speed * ec.EnvironmentTimeScale);
			if (Time.timeScale != 0f)
				renderer.transform.localPosition = Random.insideUnitSphere;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (hasHit || other.gameObject == target) return;
			bool isnpc = other.CompareTag("NPC");
			if (other.isTrigger && (isnpc || other.CompareTag("Player")))
			{
				Entity e = other.GetComponent<Entity>();
				if (e)
				{
					if (isnpc && pm) pm.RuleBreak("Bullying", 3f);
					this.e = e;
					hasHit = true;
					lifeTime = stingingLifeTime;
				}
			}

		}
		public void EntityTriggerStay(Collider other)
		{
		}
		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == target)
				target = null;
		}

		GameObject target = null;
		float lifeTime = 200f, stingSecondsDelay = 0f;
		bool hasHit = false;
		EnvironmentController ec;
		Entity e;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal AudioManager audMan, stungAudMan;

		[SerializeField]
		internal SoundObject audBees, audStung;

		[SerializeField]
		internal float minStingSecondDelayAddition = 0.01f, maxStingSecondDelayAddition = 0.8f, minStungForce = 4f, maxStungForce = 15f, speed = 35f, stingingLifeTime = 30f;

		Vector3 dir;
	}
}
