using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using UnityEngine;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class FallingParticlesFunction : RoomFunction
	{
		public Transform[] planeBounderies { get; private set; }

		[SerializeField]
		internal Texture2D particleTexture;

		[SerializeField]
		internal float gravityFactor = 0.25f, initialFallingSpeed = 3f, emissionFactor = 60f, rotationFactor = 0.1f, lifeTime = 6.5f, yOffset = 65f;

		[SerializeField]
		internal Vector2 minMaxSpeedX = new(-1.5f, 1.5f), minMaxSpeedZ = new(-1.5f, 1.5f);
		public override void OnGenerationFinished()
		{
			base.OnGenerationFinished();
			var particle = GameExtensions.GetNewParticleSystem();
			particle.gameObject.name = room.name + "_particles";
			particle.transform.SetParent(transform);
			particle.transform.localPosition = Vector3.up * 1.25f;
			particle.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = particleTexture };

			var main = particle.main;

			main.startLifetime = lifeTime;
			main.startSpeed = 5f;
			main.gravityModifier = gravityFactor;

			var shape = particle.shape;
			shape.enabled = true;
			shape.shapeType = ParticleSystemShapeType.Box;
			shape.position = room.ec.RealRoomMid(room) + Vector3.up * yOffset;
			shape.scale = room.ec.RealRoomSize(room);
			shape.randomDirectionAmount = 0.5f;

			if (rotationFactor != 0f)
			{
				var rotation = particle.rotationOverLifetime;
				rotation.enabled = true;
				rotation.x = rotationFactor;
			}

			var velocity = particle.velocityOverLifetime;
			velocity.enabled = true;
			velocity.x = new(minMaxSpeedX.x, minMaxSpeedX.y);
			velocity.y = new(-Mathf.Abs(initialFallingSpeed), -Mathf.Abs(initialFallingSpeed));
			velocity.z = new(minMaxSpeedZ.x, minMaxSpeedZ.y);

			var emission = particle.emission;
			emission.enabled = true;
			emission.rateOverTimeMultiplier = emissionFactor;

			var collision = particle.collision;
			collision.enabled = true;
			collision.enableDynamicColliders = false;
			collision.bounceMultiplier = 0.25f;

			planeBounderies = new Transform[4];

			planeBounderies[0] = SetPlaneBoundarie(room.ec.RealRoomMin(room), Direction.South);
			planeBounderies[1] = SetPlaneBoundarie(room.ec.RealRoomMin(room), Direction.East);
			planeBounderies[2] = SetPlaneBoundarie(room.ec.RealRoomMax(room), Direction.North);
			planeBounderies[3] = SetPlaneBoundarie(room.ec.RealRoomMax(room), Direction.West);

			Transform SetPlaneBoundarie(Vector3 pos, Direction dir) // Min1 and Min2 are two corners
			{
				var plane = new GameObject($"PlaneOf_{dir}_rotation");
				plane.transform.SetParent(particle.transform);
				plane.transform.position = pos;
				plane.transform.rotation = Quaternion.Euler(0f, dir.ToRotation().eulerAngles.y, 90f);
				collision.AddPlane(plane.transform);
				return plane.transform;
			}

			// Check for snow piles
			foreach (var snowPile in room.objectObject.GetComponentsInChildren<SnowPile>())
				snowPile.AssignParticlePlanes(planeBounderies);
		}
	}
}
