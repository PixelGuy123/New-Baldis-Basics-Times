using System.Collections.Generic;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.Mopliss;

public class SlipperController : MonoBehaviour // Copy paste from BloxyCola from LotsOfItems; will be changed later on to support interfaces (if needed)
{
	public static SlipperController CreateSlipperController(CustomContent.NPCs.Mopliss owner)
	{
		var stainController = new GameObject("StainController").AddComponent<SlipperController>();
		stainController.Initialize(owner);
		return stainController;
	}
	public static void CreateSlipperPackPrefab(CustomContent.NPCs.Mopliss owner, Sprite slipperSprite, SoundObject hitWallSound, SoundObject startSlipSound, SoundObject slippingLoopSound)
	{
		var puddleObject = ObjectCreationExtensions.CreateSpriteBillboard(
				slipperSprite,
				false
			).AddSpriteHolder(out var puddleSprite, 0.05f, LayerStorage.ignoreRaycast);
		puddleSprite.name = "Sprite";
		puddleSprite.gameObject.layer = 0;
		puddleSprite.transform.Rotate(90, 0, 0); // Face downward
		owner.slipperPre = puddleObject.gameObject.AddComponent<Slipper>();
		owner.slipperPre.gameObject.ConvertToPrefab(true);
		owner.slipperPre.name = "Slipper";

		var collider = owner.slipperPre.gameObject.AddComponent<BoxCollider>();
		collider.isTrigger = true;
		collider.size = new Vector3(4.96f, 1f, 4.96f);
		collider.center = Vector3.up * 2f;

		owner.slipperEffectorPre = new GameObject("SlipperEffector").AddComponent<SlipperEffector>();
		owner.slipperEffectorPre.gameObject.ConvertToPrefab(true);
		owner.slipperEffectorPre.gameObject.layer = LayerStorage.standardEntities;
		owner.slipperEffectorPre.entity = owner.slipperEffectorPre.gameObject.CreateEntity(4.5f, 2f);
		owner.slipperEffectorPre.entity.collisionLayerMask = ((ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item).entity.collisionLayerMask;

		owner.slipperEffectorPre.audMan = owner.slipperEffectorPre.gameObject.CreatePropagatedAudioManager(65f, 75f)
			.AddStartingAudiosToAudioManager(true, [slippingLoopSound]);
		owner.slipperEffectorPre.audHitWall = hitWallSound;
		owner.slipperEffectorPre.audStartSlip = startSlipSound;
	}
	public static void CreateSlipperPackPrefab(CustomContent.NPCs.Mopliss owner, Sprite slipperSprite) =>
		CreateSlipperPackPrefab(owner, slipperSprite,
			GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Sput"),
			GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Slip"),
			GenericExtensions.FindResourceObjectByName<SoundObject>("Nana_Loop"));

	internal CustomContent.NPCs.Mopliss owner;
	readonly internal HashSet<Cell> stainedCells = [];
	readonly internal HashSet<Entity> affectedEntities = [];

	public void Initialize(CustomContent.NPCs.Mopliss soda) =>
		owner = soda;


	public void CreateStain(Cell cell)
	{
		if (stainedCells.Contains(cell)) return;

		var stain = Instantiate(owner.slipperPre, transform);
		stain.transform.position = cell.FloorWorldPosition;
		stain.Initialize(this);
		stainedCells.Add(cell);
	}

	public void CreateEffector(Entity target)
	{
		if (affectedEntities.Contains(target)) return;

		// Create invisible effector using NanaPeel logic
		var effector = Instantiate(owner.slipperEffectorPre);
		effector.transform.position = target.transform.position;
		effector.Initialize(this, target);
		affectedEntities.Add(target);
	}
}
public class Slipper : MonoBehaviour
{
	private SlipperController controller;

	float spawnDelay = 0.1f;

	public void Initialize(SlipperController controller)
	{
		this.controller = controller;
	}

	void Update()
	{
		if (spawnDelay > 0f)
			spawnDelay -= Time.deltaTime;
	}


	void OnTriggerEnter(Collider other)
	{
		if (spawnDelay > 0f || other.gameObject == controller.owner.gameObject)
			return;

		Entity entity = other.GetComponent<Entity>();
		if (entity != null && entity.Grounded)
			controller.CreateEffector(entity);
	}
}
public class SlipperEffector : MonoBehaviour, IEntityTrigger
{
	private SlipperController controller;
	private Entity targetEntity;
	private readonly MovementModifier slipMod = new(Vector3.zero, 0);
	private Vector3 slipDirection;

	[SerializeField]
	internal Entity entity;

	[SerializeField]
	internal float speed = 15f, speedLimit = 50f;

	[SerializeField]
	internal AudioManager audMan;

	[SerializeField]
	internal SoundObject audHitWall, audStartSlip;

	public void Initialize(SlipperController controller, Entity target)
	{
		this.controller = controller;
		targetEntity = target;
		targetEntity.ExternalActivity.ignoreFrictionForce = true;
		targetEntity.ExternalActivity.moveMods.Add(slipMod);
		speed += targetEntity.Velocity.magnitude * 22.5f;
		if (speed > speedLimit)
			speed = speedLimit;
		slipDirection = targetEntity.Velocity.normalized;
		entity.IgnoreEntity(target, true); // Ignores collision to avoid breaking itself

		entity.Initialize(controller.owner.ec, target.transform.position);
		entity.OnEntityMoveInitialCollision += (hit) =>
		{
			slipDirection = Vector3.Reflect(slipDirection, hit.normal);
			audMan.PlaySingle(audHitWall);
		};

		audMan.PlaySingle(audStartSlip);
	}

	void Update()
	{
		if (controller == null || !IsInCoveredCell())
		{
			DestroyEffector();
			return;
		}

		entity.UpdateInternalMovement(slipDirection * speed * controller.owner.ec.EnvironmentTimeScale);
		slipMod.movementAddend = entity.ExternalActivity.Addend + slipDirection * speed * controller.owner.ec.EnvironmentTimeScale;

	}

	bool IsInCoveredCell()
	{
		Cell currentCell = controller.owner.ec.CellFromPosition(transform.position);
		return controller.stainedCells.Contains(currentCell);
	}

	void DestroyEffector()
	{
		targetEntity?.ExternalActivity.moveMods.Remove(slipMod);
		controller?.affectedEntities.RemoveWhere(x => !x || x == targetEntity);
		Destroy(gameObject);
	}

	public void EntityTriggerEnter(Collider other) { }
	public void EntityTriggerStay(Collider other) { }
	public void EntityTriggerExit(Collider other)
	{
		if (other.transform == targetEntity.transform)
		{
			DestroyEffector();
		}
	}
}