using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_ComicallyLargeJello : Item, IItemPrefab, IEntityTrigger
	{
		[SerializeField]
		internal Canvas canvasPre;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SoundObject audThrow, audLand;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Transform renderer;

		[SerializeField]
		internal float throwSpeed = 35f, throwAcceleration = -15f, lifeTime = 10f, fallLimit = 6.3f, swallowSpeed = 500f, fallGravity = 38f, startHeightSpeed = 11f, distanceTolerance = 25f;

		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(15f, "bigjello.png")).AddSpriteHolder(out var actualRenderer, 0f);
			renderer.transform.SetParent(transform);
			renderer.transform.localPosition = Vector3.up * fallLimit;
			renderer.name = "JelloRendererBase";
			renderer.gameObject.AddComponent<BillboardUpdater>();
			actualRenderer.name = "Sprite";
			actualRenderer.transform.localPosition = Vector3.back * 0.5f;
			this.renderer = renderer.transform;

			entity = gameObject.CreateEntity(3f, 4.5f, renderer.transform);

			audMan = gameObject.CreatePropagatedAudioManager(75f, 105f);
			audThrow = this.GetSound("JelloThrow.wav", "Vfx_ComicallyLargeJello_Throw", SoundType.Effect, new(66f / 256f, 99f / 256f, 60f / 256f));
			audLand = this.GetSound("JelloLand.wav", "Vfx_ComicallyLargeJello_Land", SoundType.Effect, new(66f / 256f, 99f / 256f, 60f / 256f));

			canvasPre = ObjectCreationExtensions.CreateCanvas();
			canvasPre.name = "JelloSwallowCanvas";
			canvasPre.gameObject.ConvertToPrefab(true);

			ObjectCreationExtensions.CreateImage( // Image for the swallow canvas
				canvasPre,
				AssetLoader.SpriteFromTexture2D(
					TextureExtensions.CreateSolidTexture(
						480,
						360,
						new(15f / 256f, 84f / 256f, 17f / 256f, 0.45f)
						),
					1f),
				true);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			ec = pm.ec;
			pm.RuleBreak("littering", 2f, 0.8f);

			owner = pm.gameObject;
			entity.Initialize(pm.ec, pm.transform.position);
			entity.AddForce(new(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, throwSpeed, throwAcceleration));
			audMan.PlaySingle(audThrow);
			StartCoroutine(Fall());

			return true;
		}

		IEnumerator Fall()
		{
			entity.SetHeight(fallLimit);
			float fallSpeed = startHeightSpeed;
			while (true)
			{
				fallSpeed -= ec.EnvironmentTimeScale * Time.deltaTime * fallGravity;
				entity.SetHeight(entity.BaseHeight + fallSpeed * Time.deltaTime * ec.EnvironmentTimeScale);
				if (entity.BaseHeight <= fallLimit)
				{
					entity.SetHeight(fallLimit);
					audMan.PlaySingle(audLand);
					activated = true;
					StartCoroutine(LifetimeTimer());
					yield break;
				}

				yield return null;
			}
		}

		public void EntityTriggerEnter(Collider other, bool validCollision)
		{
			if (!validCollision || !activated || !other.isTrigger || other.gameObject == owner) return;

			var entity = other.GetComponent<Entity>();
			if (entity && !stuckEntities.Exists(x => x.Key == entity))
			{
				MovementModifier moveMod = new(Vector3.zero, 0.65f);
				entity.ExternalActivity.moveMods.Add(moveMod);

				if (!canvasesMade.Exists(x => x.Key == entity) && other.CompareTag("Player"))
				{
					var player = other.GetComponent<PlayerManager>();
					var canvas = Instantiate(canvasPre, transform);
					canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(player.playerNumber).canvasCam;
					canvasesMade.Add(new(entity, canvas));
				}

				stuckEntities.Add(new(entity, moveMod));

			}
		}

		public void EntityTriggerExit(Collider other, bool validCollision)
		{
			if (!validCollision || !other.isTrigger) return;

			if (other.gameObject == owner)
				owner = null;

			var entity = other.GetComponent<Entity>();
			if (entity)
			{
				int idx = stuckEntities.FindIndex(x => x.Key == entity);
				if (idx == -1)
					return;
				RemoveEntity(idx);
			}
		}

		public void EntityTriggerStay(Collider other, bool validCollision) { }

		IEnumerator LifetimeTimer()
		{
			float timer = lifeTime;
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Destroy(gameObject);
		}

		void OnDestroy()
		{
			for (int i = 0; i < stuckEntities.Count; i++)
				stuckEntities[i].Key.ExternalActivity.moveMods.Remove(stuckEntities[i].Value);
		}

		void Update()
		{
			for (int i = 0; i < stuckEntities.Count; i++)
			{
				var entry = stuckEntities[i];
				if (entry.Key && entry.Key.transform)
				{
					Vector3 distance = transform.position - entry.Key.transform.position;
					if (distance.magnitude <= distanceTolerance)
						entry.Value.movementAddend = distance * swallowSpeed * Time.deltaTime * ec.EnvironmentTimeScale;
					else
						RemoveEntity(i--);
				}
			}
		}

		void RemoveEntity(int index)
		{
			Entity entity = stuckEntities[index].Key;
			int canvasIndex = canvasesMade.FindIndex(x => x.Key == entity);
			if (canvasIndex != -1)
			{
				Destroy(canvasesMade[canvasIndex].Value.gameObject);
				canvasesMade.RemoveAt(canvasIndex);
			}
			stuckEntities[index].Key.ExternalActivity.moveMods.Remove(stuckEntities[index].Value);
			stuckEntities.RemoveAt(index);
		}

		readonly List<KeyValuePair<Entity, MovementModifier>> stuckEntities = [];
		readonly List<KeyValuePair<Entity, Canvas>> canvasesMade = [];

		bool activated;
		GameObject owner;
		EnvironmentController ec;
	}
}