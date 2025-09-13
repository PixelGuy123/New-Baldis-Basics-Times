using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using MTM101BaldAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Slingshot : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			// Canvas setup
			chargeCanvas = ObjectCreationExtensions.CreateCanvas();
			chargeCanvas.transform.SetParent(transform);
			chargeCanvas.transform.localPosition = Vector3.zero;

			shootStages = this.GetSpriteSheet(3, 2, 1f, "slingCanvas.png");

			slingImage = ObjectCreationExtensions.CreateImage(chargeCanvas, shootStages[0]);
			slingImage.rectTransform.anchoredPosition = Vector2.down * startCanvasPos;

			// Audio setup
			audMan = gameObject.CreateAudioManager(65f, 85f).MakeAudioManagerNonPositional();
			shootSound = this.GetSound("slingShoot.wav", "Vfx_Slingshot_Shoot", SoundType.Effect, Color.white);
			chargeSound = this.GetSoundNoSub("slingPull.wav", SoundType.Effect);


			var ballObj = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(40f, "slingBall.png"))
				.AddSpriteHolder(out var renderer, 0f, LayerStorage.standardEntities);
			ballObj.gameObject.ConvertToPrefab(true);
			ballObj.name = "SlingBall";
			renderer.name = "Sprite";

			projectilePre = ballObj.gameObject.AddComponent<SlingshotProjectile>();
			projectilePre.entity = projectilePre.gameObject.CreateEntity(1f, 1f, renderer.transform);
			projectilePre.audMan = projectilePre.gameObject.CreatePropagatedAudioManager(55f, 80f);
			projectilePre.audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			chargeCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			StartCoroutine(RevealSlingShot());
			StartCoroutine(ChargeRoutine());

			if (nextSlingShot)
			{
				pm.itm.SetItem(nextSlingShot, pm.itm.selectedItem);
				return false;
			}

			return true;
		}

		IEnumerator RevealSlingShot()
		{
			Vector2 startPos = Vector2.down * startCanvasPos;

			// Slide up animation
			float duration = slightShotShowUpSlideDuration;
			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				slingImage.rectTransform.anchoredPosition = Vector2.Lerp(startPos, Vector2.zero, t / duration);
				yield return null;
			}
			slingImage.rectTransform.anchoredPosition = Vector2.zero;

			while (active)
				yield return null;

			// Slide down animation
			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				slingImage.rectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, startPos, t / duration);
				yield return null;
			}

			slingImage.enabled = false;

			while (audMan.QueuedAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}

		IEnumerator ChargeRoutine()
		{
			active = true;
			currentCharge = 0f;


			// Charge phase
			audMan.QueueAudio(chargeSound);

			float chargeTime = 0f, stageChargeTime = 0f, stageMaxChargeTime = maxChargeDuration / shootStages.Length;
			int stageIndex = 1;

			while (Singleton<InputManager>.Instance.GetDigitalInput("UseItem", false) && chargeTime < maxChargeDuration)
			{
				chargeTime += pm.ec.EnvironmentTimeScale * Time.deltaTime;
				if (stageIndex < shootStages.Length) // Change stages through out the time
				{
					stageChargeTime += pm.ec.EnvironmentTimeScale * Time.deltaTime;
					while (stageChargeTime >= stageMaxChargeTime)
					{
						slingImage.sprite = shootStages[stageIndex++];
						stageChargeTime -= stageMaxChargeTime;
					}
				}
				currentCharge = Mathf.Clamp01(chargeTime / maxChargeDuration);
				// Update canvas sprites overtime
				yield return null;
			}

			// Release
			ShootProjectile(currentCharge);
			active = false;
		}

		void ShootProjectile(float strength)
		{
			audMan.FlushQueue(true);
			audMan.PlaySingle(shootSound);
			var ball = Instantiate(projectilePre);
			ball.Initialize(pm.ec,
				pm.transform.position + Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward * 2f, // position
				minStrength + (maxStrength - minStrength) * strength, // speed
				strength,
				Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, // direction
				pm.gameObject,
				pm
			);
		}

		[SerializeField]
		private SlingshotProjectile projectilePre;
		[SerializeField]
		private float minStrength = 15f, maxStrength = 80f, maxChargeDuration = 2.5f, slightShotShowUpSlideDuration = 0.3f, startCanvasPos = 450;
		[SerializeField]
		private SoundObject chargeSound, shootSound;
		[SerializeField]
		internal ItemObject nextSlingShot;
		[SerializeField]
		AudioManager audMan;
		[SerializeField]
		Canvas chargeCanvas;
		[SerializeField]
		Image slingImage;
		[SerializeField]
		Sprite[] shootStages;

		float currentCharge = 0f;
		bool active = false;
	}

	public class SlingshotProjectile : MonoBehaviour, IEntityTrigger
	{
		public void Initialize(EnvironmentController ec, Vector3 pos, float speed, float slowDownFactor, Vector3 direction, GameObject owner, PlayerManager pmOwner)
		{
			this.ec = ec;
			this.direction = direction;
			this.speed = speed;
			this.pmOwner = pmOwner;
			entity.Initialize(ec, pos);
			entity.OnEntityMoveInitialCollision += (hit) => this.direction = Vector3.Reflect(this.direction, hit.normal);
			this.owner = owner;
			initialized = true;
			moveMod = new(Vector3.zero, minSlowDownFactor + (maxSlowDownFactor - minSlowDownFactor) * slowDownFactor);

			StartCoroutine(GravitytLoop());
		}

		public void EntityTriggerEnter(Collider other, bool validCollision)
		{
			if (!validCollision || hasHit || !other.isTrigger || other.gameObject == owner || !other.TryGetComponent<Entity>(out var e))
				return;

			pmOwner?.RuleBreak("Bullying", bullyingLinger);
			StartCoroutine(Hit(e));
		}

		public void EntityTriggerStay(Collider other, bool validCollision) { }
		public void EntityTriggerExit(Collider other, bool validCollision)
		{
			if (validCollision && other.gameObject == owner)
				owner = null;
		}

		void Update()
		{
			if (!initialized)
				return;

			if (hasHit)
			{
				entity.UpdateInternalMovement(Vector3.zero);
				return;
			}

			entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
		}

		IEnumerator GravitytLoop()
		{
			float verticalSpeed = startVerticalSpeed, currentHeight = entity.BaseHeight;

			while (!hasHit)
			{
				// Apply gravity with environment time scaling
				verticalSpeed += gravity * ec.EnvironmentTimeScale * Time.deltaTime;
				currentHeight += verticalSpeed * ec.EnvironmentTimeScale * Time.deltaTime;

				// Clamp height between ground and max height
				currentHeight = Mathf.Clamp(currentHeight, 0f, 9f);
				entity.SetHeight(currentHeight);

				// Destroy when touching ground
				if (entity.BaseHeight <= 0f)
				{
					Destroy(gameObject);
					yield break;
				}

				yield return null;
			}
		}


		IEnumerator Hit(Entity e)
		{
			audMan.PlaySingle(audHit);
			entity.SetVisible(false);
			entity.SetFrozen(true);
			hasHit = true;
			e.AddForce(new(direction, speed, -speed));
			e.ExternalActivity.moveMods.Add(moveMod);
			float timer = hitLifeTime;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			e?.ExternalActivity.moveMods.Remove(moveMod);
			Destroy(gameObject);
		}

		EnvironmentController ec;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		[Range(0f, 1f)]
		internal float minSlowDownFactor = 0.9f, maxSlowDownFactor = 0.25f;

		[SerializeField]
		internal float hitLifeTime = 15f, gravity = -12.5f, startVerticalSpeed = 9.5f, bullyingLinger = 2.5f;

		MovementModifier moveMod;
		GameObject owner;
		PlayerManager pmOwner;
		bool initialized = false, hasHit = false;
		Vector3 direction;
		float speed;
	}
}