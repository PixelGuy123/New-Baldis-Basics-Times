using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_IceSkates : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			// Entity setup
			entity = gameObject.CreateEntity(1.5f, 1.5f);
			gameObject.layer = LayerStorage.standardEntities;

			audMan = gameObject.CreatePropagatedAudioManager(65f, 85f);
			audSkateloop = this.GetSoundNoSub("skatesLoop.wav", SoundType.Effect);
			audSkateHitWall = this.GetSoundNoSub("skatesHit.wav", SoundType.Effect);

			slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").SafeDuplicatePrefab(true);
			((SpriteRenderer)slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = this.GetSprite(13.85f, "iceFloor.png");
			slipMatPre.name = "IceFloor";

			gaugeSprite = ItmObj.itemSpriteSmall;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			if (active) return false;

			lifeTime = totalLifeTime;

			this.pm = pm;
			ec = pm.ec;
			active = true;
			entity.Initialize(pm.ec, pm.transform.position);
			pm.plm.Entity.OnTeleport += OnTeleportEv;

			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				currentDirection = Vector3.Reflect(currentDirection, hit.normal);
				audMan.PlaySingle(audSkateHitWall);
			};

			pm.Am.moveMods.Add(blockMovementMod);
			audMan.QueueAudio(audSkateloop);
			audMan.SetLoop(true);

			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, totalLifeTime);

			currentDirection = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;

			return true;
		}

		void Update()
		{
			if (!active)
			{
				entity.UpdateInternalMovement(Vector3.zero);
				return;
			}

			lifeTime -= pm.PlayerTimeScale * Time.deltaTime;
			gauge.SetValue(totalLifeTime, lifeTime);

			if (lifeTime <= 0f)
			{
				Cleanup();
				return;
			}

			var tar = pm.plm.Entity;

			if (tar.Frozen != entity.Frozen)
				entity.SetFrozen(tar.Frozen);

			if (tar.InteractionDisabled != entity.InteractionDisabled)
				entity.SetInteractionState(!tar.InteractionDisabled);

			Vector3 tarDir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			currentDirection = Vector3.RotateTowards(currentDirection, tarDir, rotationSpeed * Time.deltaTime * pm.PlayerTimeScale, 0f).normalized;
			entity.UpdateInternalMovement(tar.ExternalActivity.Addend + currentDirection * skateSpeed * pm.PlayerTimeScale);
			pm.Teleport(transform.position);

			CreateSlipper(ec.CellFromPosition(transform.position));
		}

		void Cleanup()
		{
			active = false;
			pm.Am.moveMods.Remove(blockMovementMod);
			pm.plm.Entity.OnTeleport -= OnTeleportEv;
			for (int i = 0; i < generatedSlippers.Count; i++)
				Destroy(generatedSlippers[i].gameObject);
			Destroy(gameObject);
			gauge.Deactivate();
		}

		void CreateSlipper(Cell cell)
		{
			if (!foundCells.Add(cell))
				return;

			var slipper = Instantiate(slipMatPre);
			slipper.SetAnOwner(pm.gameObject);
			slipper.transform.position = cell.FloorWorldPosition;
			generatedSlippers.Add(slipper);
		}

		void OnTeleportEv(Vector3 pos)
		{
			if (pos != transform.position)
				entity.Teleport(pos); // To not spam the teleport on itself

		}

		void OnDestroy()
		{
			if (active) Cleanup();
		}

		float lifeTime;

		[SerializeField]
		private float skateSpeed = 60f, rotationSpeed = 1.85f, totalLifeTime = 45f;
		[SerializeField]
		private SoundObject audSkateloop, audSkateHitWall;
		[SerializeField]
		Entity entity;
		[SerializeField]
		PropagatedAudioManager audMan;
		[SerializeField]
		internal SlippingMaterial slipMatPre;

		[SerializeField]
		internal Sprite gaugeSprite;

		HudGauge gauge;
		Vector3 currentDirection;
		bool active = false;
		EnvironmentController ec;
		readonly HashSet<Cell> foundCells = [];
		readonly List<SlippingMaterial> generatedSlippers = [];

		readonly MovementModifier blockMovementMod = new(Vector3.zero, 0f);
	}
}