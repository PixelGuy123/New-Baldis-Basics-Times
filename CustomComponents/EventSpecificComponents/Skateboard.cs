using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public class Skateboard : MonoBehaviour
	{
		public void OverrideNavigator(Navigator nav)
		{
			navRef = nav;
			overridingNavigator = nav;
		}
		public void Initialize(Entity target, EnvironmentController ec)
		{
			tar = target;
			tarHeight = tar.InternalHeight;
			this.ec = ec;
			entity.Initialize(ec, target.transform.position); // For some reason, entities are in 0,0???? So a pos is required
			entity.SetHeight(2f);
			tar.SetHeight(tarHeight + 1f);

			target.OnTeleport += OnTeleportEv;

			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				direction = Vector3.Reflect(direction, hit.normal);
				speed *= speedHitDecreaser;
			};

			StartCoroutine(Delay());
		}

		IEnumerator Delay()
		{
			yield return null;
			initialized = true; // Gives time to the skateboard's entity's velocity turn into 0
			yield break;
		}

		void OnTeleportEv(Vector3 pos)
		{
			if (pos != transform.position)
				entity.Teleport(pos); // To not spam the teleport on itself
		}

		void Update()
		{
			if (!initialized) return;
			if (!tar || (overridingNavigator && !navRef))
			{
				Destroy(gameObject);
				return;
			}

			entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);

			// Get all these velocities to 0
			if (speed > 0f)
				speed -= ec.EnvironmentTimeScale * Time.deltaTime;

			tar.Teleport(transform.position);

			if (pushDelay <= 0f)
			{
				Vector3 vel = overridingNavigator ? navRef.velocity : tar.InternalMovement;

				if (!vel.x.CompareFloats(0f) || !vel.z.CompareFloats(0f)) // If it is above 0 (or below)
				{
					//if (overridingNavigator)
					//	Debug.Log($"{tar.name} nav velocity is {navRef.velocity}");
					direction = vel.normalized;
					speed += vel.magnitude * (overridingNavigator ? 25f : 1f);
					speed = Mathf.Clamp(speed, 0f, maxVelocityLimit);

					transform.LookAt(transform.position + direction); // Workaround because converting Vector3 directions to Quaternions is not easy apparently (?)
					pushDelay = maxSkateboardPushDelay;

					audMan.PlaySingle(audRoll);
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
		Navigator navRef;
		EnvironmentController ec;
		bool initialized = false, overridingNavigator = false;
		Vector3 direction = Vector3.zero;
		float tarHeight = 5f, pushDelay = 0f, speed = 0f;


		[SerializeField]
		[Range(0.0f, 1.0f)]
		internal float speedHitDecreaser = 0.75f;

		[SerializeField]
		internal float maxVelocityLimit = 75f, maxSkateboardPushDelay = 0.45f;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audRoll;
	}
}
