using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_FireworkRocket : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			rocketCanvas = ObjectCreationExtensions.CreateCanvas();
			rocketCanvas.transform.SetParent(transform);
			rocketCanvas.transform.localPosition = Vector3.zero;
			var imageSprs = this.GetSpriteSheet(3, 4, 1f, "rocketCanvas.png").ExcludeNumOfSpritesFromSheet(1);
			rocketImage = ObjectCreationExtensions.CreateImage(rocketCanvas, imageSprs[0]);

			explosionAnimation = gameObject.AddComponent<AnimationComponent>();
			explosionAnimation.imageRenderers = [rocketImage];
			explosionAnimation.speed = 16f;
			explosionAnimation.animation = imageSprs;


			smokeParticles = new GameObject("NormalSmokeParticles").AddComponent<ParticleSystem>();
			smokeParticles.transform.SetParent(transform);
			smokeParticles.transform.localPosition = Vector3.zero;
			Texture2D smokeTexture = TextureExtensions.CreateSolidTexture(64, 64, Color.white);
			smokeTexture.name = "RocketSmokeTexture";
			smokeParticles.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = smokeTexture };

			var main = smokeParticles.main;
			main.gravityModifierMultiplier = 0.05f;
			main.startLifetimeMultiplier = 1.8f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(0.5f, 1f);
			main.startColor = new(new(0.1f, 0.1f, 0.1f), Color.white);

			var emission = smokeParticles.emission;
			emission.rateOverTimeMultiplier = 100f;
			emission.enabled = true;

			var vel = smokeParticles.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.Local;
			vel.x = new(-5f, 5f);
			vel.y = new(-5f, 5);
			vel.z = new(15, 65f);

			var col = smokeParticles.collision;
			col.enabled = true;
			col.type = ParticleSystemCollisionType.World;
			col.enableDynamicColliders = false;

			explosionSmokeParticles = Instantiate(smokeParticles, transform);
			explosionSmokeParticles.transform.localPosition = Vector3.zero;
			explosionSmokeParticles.GetComponent<ParticleSystemRenderer>().material = smokeParticles.GetComponent<ParticleSystemRenderer>().material;

			emission = explosionSmokeParticles.emission;
			emission.enabled = false;

			vel = explosionSmokeParticles.velocityOverLifetime;
			vel.x = new(-20f, 20f);
			vel.y = vel.x;
			vel.z = vel.x;

			explosionSmokeParticles.gameObject.SetActive(false);
			explosionSmokeParticles.name = "ExplosionSmokeParticles";
			main = explosionSmokeParticles.main;
			main.startLifetimeMultiplier = 3.5f;

			audMan = gameObject.CreateAudioManager(75f, 95f).MakeAudioManagerNonPositional();
			audRocketLoopSound = this.GetSoundNoSub("rocketLaunch.wav", SoundType.Effect);
			audExplosionSound = BBTimesManager.man.Get<SoundObject>("audExplosion");

			gaugeSprite = ItmObj.itemSpriteSmall;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			moveMod = new(Vector3.zero, movementFactor) { forceTrigger = true };
			pm.plm.Entity.ExternalActivity.moveMods.Add(moveMod);

			rocketCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

			audMan.QueueAudio(audRocketLoopSound);
			audMan.SetLoop(true);

			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, maxLifeTime);

			StartCoroutine(RocketLifetime());
			return true;
		}

		void UpdateVelocity()
		{
			Vector3 vel = moveMod.movementAddend;
			float offset = pm.ec.EnvironmentTimeScale * Time.deltaTime * deacceleration;

			vel.x += Mathf.Sign(vel.x) * offset;
			vel.z += Mathf.Sign(vel.z) * offset;

			vel += Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward *
				   acceleration * Time.deltaTime * pm.ec.EnvironmentTimeScale;
			vel.Limit(maxSpeed, 0f, maxSpeed);

			if (vel.magnitude > 0f)
			{
				if (Physics.Raycast(pm.transform.position, vel.normalized, out RaycastHit hit, rayCastHitDistance, collisionLayer, QueryTriggerInteraction.Collide) && hit.transform.CompareTag("Wall"))
				{
					// Zero out only the velocity component moving into the wall
					vel -= Vector3.Project(vel, hit.normal);
				}
			}

			moveMod.movementAddend = vel;
		}


		IEnumerator RocketLifetime()
		{
			active = true;
			float lifetime = maxLifeTime;

			while (lifetime > 0f && active)
			{
				float prevMagnitude = moveMod.movementAddend.magnitude; // to avoid velocity changing mid-time
				UpdateVelocity();

				smokeParticles.transform.forward = -Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
				smokeParticles.transform.position = pm.transform.position + smokeParticles.transform.forward * 1.5f;
				lifetime -= pm.ec.EnvironmentTimeScale * Time.deltaTime;

				gauge.SetValue(maxLifeTime, lifetime);

				if (prevMagnitude > hitSpeed &&
					Physics.Raycast(pm.transform.position, moveMod.movementAddend.normalized, out var hit, rayCastHitDistance, collisionLayer, QueryTriggerInteraction.Collide) &&
					hit.transform.CompareTag("Wall"))
					Explode();

				yield return null;
			}

			Explode();
		}

		void Explode()
		{
			gauge.Deactivate();

			active = false;
			audMan.FlushQueue(true);
			audMan.PlaySingle(audExplosionSound);

			smokeParticles.gameObject.SetActive(false);
			if (pm != null)
				transform.position = pm.transform.position;
			explosionSmokeParticles.gameObject.SetActive(true);
			explosionSmokeParticles.Emit(particleExplosionAmount);

			explosionAnimation.Initialize(pm.ec);
			explosionAnimation.StopLastFrameMode();

			pm?.plm.Entity.ExternalActivity.moveMods.Remove(moveMod);

			StartCoroutine(DelayedDestroy());
		}

		IEnumerator DelayedDestroy()
		{

			yield return new WaitForSecondsEnvironmentTimescale(pm.ec, dieDelay);
			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}

		void OnDestroy()
		{
			if (pm != null)
			{
				StopAllCoroutines();
				pm.plm.Entity.ExternalActivity.moveMods.Remove(moveMod);
			}
		}

		[SerializeField]
		private float maxSpeed = 125f, hitSpeed = 75f, acceleration = 75f, deacceleration = 17f, maxLifeTime = 60f, dieDelay = 2f, rayCastHitDistance = 3.5f;
		[SerializeField]
		[Range(0f, 1f)]
		private float movementFactor = 0.15f;

		[SerializeField]
		private int particleExplosionAmount = 100;

		[SerializeField]
		private Canvas rocketCanvas;

		[SerializeField]
		private ParticleSystem smokeParticles, explosionSmokeParticles;

		[SerializeField]
		private UnityEngine.UI.Image rocketImage;

		[SerializeField]
		private SoundObject audRocketLoopSound, audExplosionSound;

		[SerializeField]
		private AudioManager audMan;

		[SerializeField]
		private AnimationComponent explosionAnimation;

		[SerializeField]
		private LayerMask collisionLayer = LayerStorage.gumCollisionMask;

		[SerializeField]
		internal Sprite gaugeSprite;
		HudGauge gauge;

		bool active = false;
		MovementModifier moveMod;
	}
}