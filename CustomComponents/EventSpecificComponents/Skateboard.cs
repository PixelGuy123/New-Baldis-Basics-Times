using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public class Skateboard : MonoBehaviour
	{
		public void Initialize(Entity target, EnvironmentController ec)
		{
			tar = target;
			tarHeight = tar.InternalHeight;
			this.ec = ec;
			entity.Initialize(ec, target.transform.position); // For some reason, entities are in 0,0???? So a pos is required

			target.OnTeleport += OnTeleportEv;

			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				if (Vector3.Angle(-curVelocity.normalized, hit.normal) <= 40f)
					curVelocity = Vector3.zero;
			};

			StartCoroutine(Delay());
		}

		IEnumerator Delay()
		{
			yield return null;
			initialized = true; // Gives time to the skateboard's entity's velocity turn into 0
			yield break;
		}

		void OnTeleportEv(Vector3 pos) => entity.Teleport(pos);

		void Update()
		{
			if (!initialized) return;
			if (!tar)
			{
				Destroy(gameObject);
				return;
			}

			entity.UpdateInternalMovement(curVelocity * ec.EnvironmentTimeScale);
			tar.SetHeight(tarHeight + 1f);

			// Get all these velocities to 0
			if (curVelocity.x > 0f)
				curVelocity.x = Mathf.Max(0f, curVelocity.x - (ec.EnvironmentTimeScale * Time.deltaTime));
			else if (curVelocity.x < 0f)
				curVelocity.x = Mathf.Min(0f, curVelocity.x + (ec.EnvironmentTimeScale * Time.deltaTime));

			if (curVelocity.y > 0f)
				curVelocity.y = Mathf.Max(0f, curVelocity.y - (ec.EnvironmentTimeScale * Time.deltaTime));
			else if (curVelocity.y < 0f)
				curVelocity.y = Mathf.Min(0f, curVelocity.y + (ec.EnvironmentTimeScale * Time.deltaTime));

			if (curVelocity.z > 0f)
				curVelocity.z = Mathf.Max(0f, curVelocity.z - (ec.EnvironmentTimeScale * Time.deltaTime));
			else if (curVelocity.z < 0f)
				curVelocity.z = Mathf.Min(0f, curVelocity.z + (ec.EnvironmentTimeScale * Time.deltaTime));

			tar.Teleport(transform.position);

			if (pushDelay <= 0f)
			{
				if (lostVelocity) // IDK WHAT TO DO IF THIS CODE DOESN'T WORK FOR SKATEBOARDS (tested still required, I've got to sleep)
				{
					lostVelocity = false;
					tar.SetFrozen(false);
				}
				var vel = tar.Velocity - entity.Velocity;
				if (!vel.x.CompareFloats(0f) || !vel.z.CompareFloats(0f)) // If it is above 0 (or below)
				{
					curVelocity += vel * 35f;
					curVelocity.Limit(maxVelocityLimit, maxVelocityLimit, maxVelocityLimit);

					transform.rotation = Quaternion.Euler(curVelocity.normalized);
					pushDelay = maxSkateboardPushDelay;
					lostVelocity = true;
					tar.SetFrozen(true);
				}
				return;
			}

			pushDelay -= ec.EnvironmentTimeScale * Time.deltaTime;
		}

		void OnDestroy()
		{
			if (!tar) return;
			tar.SetHeight(tarHeight);
			tar.OnTeleport -= OnTeleportEv;
		}

		Entity tar;
		EnvironmentController ec;
		bool initialized = false, lostVelocity = false;
		Vector3 curVelocity = Vector3.zero;
		float tarHeight = 5f, pushDelay = 0f;

		[SerializeField]
		internal float maxVelocityLimit = 75f, maxSkateboardPushDelay = 0.2f;

		[SerializeField]
		internal Entity entity;
	}
}
