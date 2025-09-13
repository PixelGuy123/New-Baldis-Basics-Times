using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Pogostick : Item, IItemPrefab, IEntityTrigger
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			audBoing = this.GetSound("boing.wav", "POGST_Boing", SoundType.Effect, Color.white);
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
			audThrow = BBTimesManager.man.Get<SoundObject>("audGenericThrow");

			pogoSprites = this.GetSpriteSheet(2, 2, 35f, "PogoWorld.png");
			rendererBase = ObjectCreationExtensions.CreateSpriteBillboard(pogoSprites[0]).AddSpriteHolder(out worldRenderer, 0f, null).transform;
			worldRenderer.name = "PogoStickRenderer";

			rendererBase.name = "PogoStick";
			rendererBase.transform.SetParent(transform);
			rendererBase.transform.localPosition = Vector3.zero;


			entity = gameObject.CreateEntity(2f, 2f, rendererBase: rendererBase);
			entity.SetGrounded(false);

			((CapsuleCollider)entity.collider).height = 10f;

			pogoCanvas = ObjectCreationExtensions.CreateCanvas();
			pogoCanvas.name = "PogoCanvas";
			pogoCanvas.transform.SetParent(transform);
			pogoCanvas.gameObject.SetActive(false);

			pogoImage = ObjectCreationExtensions.CreateImage(pogoCanvas, this.GetSprite(1f, "Pogostick_UI.png"));

		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (usingPogo || pm.plm.Entity.Frozen)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			pm.onPlayerTeleport += OnTeleport;
			usingPogo = true;

			pogoCanvas.gameObject.SetActive(true);
			pogoCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

			// Already hides the renderer
			entity.SetVisible(false);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBoing);
			entity.Initialize(pm.ec, pm.transform.position);
			transform.rotation = pm.cameraBase.rotation;

			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).UpdateTargets(rendererBase, targetIdx);

			StartCoroutine(AnimatedJump());

			if (pogoStickReplacement != null)
			{
				pm.itm.SetItem(pogoStickReplacement, pm.itm.selectedItem);
				return false;
			}


			return true;
		}

		IEnumerator AnimatedJump()
		{
			Force force = new(pm.transform.forward, 45f, -2.5f);
			entity.AddForce(force);
			forcesApplied.Add(force);
			pm.plm.Entity.Override(overrider);
			overrider.SetFrozen(true);
			overrider.SetInteractionState(false);
			pogoImage.transform.localPosition = Vector3.up * pogoImageDespawnPosition;

			while (true)
			{
				yVelocity -= pm.PlayerTimeScale * Time.deltaTime * 5.5f;
				height += yVelocity * Time.deltaTime * 0.35f;

				if (height > maxHeight)
				{
					height = maxHeight;
					yVelocity = 0f;
				}

				pm.Teleport(transform.position);

				if (forceStop || height < Entity.physicalHeight)
				{
					height = Entity.physicalHeight;
					break;
				}

				entity.SetHeight(height);

				// Slides the pogo image to give the impression of "bouncing"
				float targetHeight = Mathf.Lerp(0f, minVerticalHeightForPogoImage, height / maxHeight),
					currentHeight = pogoImage.transform.localPosition.y;
				pogoImage.transform.localPosition += Vector3.up * Time.deltaTime * pm.ec.EnvironmentTimeScale * ((targetHeight - currentHeight) * 12f);

				yield return null;
			}

			overrider.SetInteractionState(true);
			overrider.SetFrozen(false);
			overrider.Release();

			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).UpdateTargets(null, targetIdx);

			pm.onPlayerTeleport -= OnTeleport;

			// *** Thrown away animation ***
			float t = 0f;
			if (pogoStickReplacement) // If there's another replacement, it means the pogostick is still usable and won't be throwed away
			{
				float currentImageHeight = pogoImage.transform.localPosition.y;
				const float slideTime = 0.6f;
				while (t < slideTime)
				{
					t += pm.ec.EnvironmentTimeScale * Time.deltaTime;
					pogoImage.transform.localPosition = Vector3.up * Mathf.Lerp(currentImageHeight, pogoImageDespawnPosition, t / slideTime);
					yield return null;
				}
				Destroy(gameObject);

				yield break;
			}

			pogoCanvas.gameObject.SetActive(false);

			entity.SetVisible(true); // Shows renderer again
			DestroyImmediate(entity); // Removes entirely entity from existence

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audThrow);

			rendererBase.transform.SetParent(null, true); // De-attach from the main transform and let it free in the wild
			var rigidBody = rendererBase.gameObject.AddComponent<Rigidbody>(); // Literally
			rigidBody.useGravity = true;
			rigidBody.AddForce(pm.transform.forward * Random.Range(11f, 16f) + Vector3.up * Random.Range(4f, 6f), ForceMode.Impulse);

			while (rendererBase.position.y > -5f)
			{
				t += pm.ec.EnvironmentTimeScale * Time.deltaTime * 0.65f;
				worldRenderer.sprite = pogoSprites[Mathf.FloorToInt(Mathf.Lerp(0, pogoSprites.Length - 1, Mathf.Clamp01(t)))];
				worldRenderer.SetSpriteRotation(Mathf.Lerp(0f, 180f, Mathf.Clamp01(t)));

				yield return null;
			}

			Destroy(gameObject);

			yield break;
		}

		void Update()
		{
			transform.rotation = pm.cameraBase.rotation;
		}

		void OnTeleport(PlayerManager player, Vector3 pos, Vector3 positionDelta)
		{
			if (positionDelta.magnitude > teleportDistanceThreshold)
			{
				forceStop = true;
				return;
			}
			entity.Teleport(pos);
		}

		public void EntityTriggerEnter(Collider other, bool validCollision)
		{
			if (!validCollision || pm.gameObject == other.gameObject) return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e && !e.Squished)
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audHit);
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBoing);
					Vector3 dir = pm.transform.forward;
					Force f = new(dir, Mathf.Max(minForcePerHit, forcePerHit / (forcesApplied.Count * 0.5f)), -3f); // When more forces were applied, more weaker will be the force
					entity.AddForce(f);
					forcesApplied.ForEach(x => x.direction = new(dir.x, dir.z)); // Updates direction

					forcesApplied.Add(f);

					e.Squish(15f);
					if (yVelocity < 0f)
						yVelocity = 0f;
					yVelocity += 10f;

				}
			}
		}

		public void EntityTriggerStay(Collider other, bool validCollision) { }

		public void EntityTriggerExit(Collider other, bool validCollision) { }

		void OnDestroy()
		{
			usingPogo = false;
			if (pm)
				pm.onPlayerTeleport -= OnTeleport;
		}

		static bool usingPogo = false;
		bool forceStop = false;

		float height = Entity.physicalHeight, yVelocity = 10f;

		const int targetIdx = 15;

		const float maxHeight = 9.5f;

		readonly EntityOverrider overrider = new();

		readonly List<Force> forcesApplied = [];

		[SerializeField]
		internal ItemObject pogoStickReplacement;

		[SerializeField]
		internal SoundObject audBoing, audHit, audThrow;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Transform rendererBase;

		[SerializeField]
		internal Canvas pogoCanvas;

		[SerializeField]
		internal Image pogoImage;

		[SerializeField]
		internal SpriteRenderer worldRenderer;

		[SerializeField]
		internal Sprite[] pogoSprites;

		[SerializeField]
		internal float minVerticalHeightForPogoImage = -90f, pogoImageDespawnPosition = -150f, forcePerHit = 25f, minForcePerHit = 5f, teleportDistanceThreshold = 5f;
	}
}
