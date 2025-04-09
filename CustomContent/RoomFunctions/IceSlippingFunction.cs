using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions;

public class IceSlippingFunction : RoomFunction
{
	// Entity Ref > (how many touches; the moveMod)
	readonly private Dictionary<Entity, int> collidedEntities = [];
	readonly private Dictionary<Entity, MovementModifier> slippingEntities = [];
	readonly private List<Entity> entitiesToRemove = []; // To remove null entities, yknow


	[SerializeField]
	internal float iceFriction = 0.25f, entityMomentum = 75f;

	bool initialized = false;
	public override void Initialize(RoomController room)
	{
		base.Initialize(room);
		initialized = true;
	}

	public override void OnEntityEnter(Entity entity)
	{
		base.OnEntityEnter(entity);
		AddEntityToSlip(entity);
	}

	public override void OnEntityExit(Entity entity)
	{
		base.OnEntityExit(entity);
		RemoveEntity(entity);
	}

	public void AddEntityToSlip(Entity e)
	{
		if (collidedEntities.ContainsKey(e))
			collidedEntities[e]++;
		else
		{
			collidedEntities.Add(e, 1);
			MovementModifier moveMod = new(
				!float.IsNaN(e.Velocity.magnitude) ? // Avoid the player NaN issue
				e.Velocity :
				Vector3.zero,
				1f);

			e.ExternalActivity.moveMods.Add(moveMod);
			slippingEntities.Add(e, moveMod);
		}
	}

	public void RemoveEntity(Entity e)
	{
		if (collidedEntities.ContainsKey(e) && --collidedEntities[e] <= 0)
		{
			collidedEntities.Remove(e);
			e.ExternalActivity.moveMods.Remove(slippingEntities[e]);
			slippingEntities.Remove(e);
		}
	}

	void Update()
	{
		if (!initialized)
			return;

		// Clean up null entities
		foreach (var entity in collidedEntities.Keys)
		{
			if (entity == null)
			{
				entitiesToRemove.Add(entity);
			}
		}
		for (int i = 0; i < entitiesToRemove.Count; i++)
		{
			collidedEntities.Remove(entitiesToRemove[i]);
			slippingEntities.Remove(entitiesToRemove[i]);
			entitiesToRemove.RemoveAt(i--);
		}

		foreach (var kvp in collidedEntities)
		{
			Entity entity = kvp.Key;

			if (slippingEntities.TryGetValue(entity, out MovementModifier mod))
			{
				if (entity.Frozen || !entity.Grounded)
				{
					mod.movementAddend = Vector3.zero; // Reset external addend
					continue;
				}

				Vector3 externalAddend = entity.ExternalActivity.Addend * Time.deltaTime;
				Vector3 internalVelocity = entity.Velocity - externalAddend; // Seriously, why NPCs are the only ones that doesn't use entity.InternalMovement or anything that's not VELOCITY

				float magnitude = internalVelocity.magnitude;
				if (float.IsNaN(magnitude))
				{
					internalVelocity = Vector3.zero;
					magnitude = 0f;
				}

				// Apply ice friction uaing deltaTime and ec timescale
				float friction = iceFriction * Time.deltaTime * room.ec.EnvironmentTimeScale;
				mod.movementAddend *= Mathf.Clamp01(1f - friction);

				if (magnitude > 0f)
				{
					// Calculate slip force using corrected velocity
					Vector3 slipForce = internalVelocity * (entityMomentum * Time.deltaTime * room.ec.EnvironmentTimeScale);
					mod.movementAddend += slipForce;
				}
			}
			else
			{
				entitiesToRemove.Add(entity);
			}
		}
	}

}