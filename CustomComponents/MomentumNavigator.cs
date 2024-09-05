using BBTimes.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BBTimes.CustomComponents
{
	public class MomentumNavigator : MonoBehaviour
	{
		public void Initialize(EnvironmentController ec, bool useDefaultMovement)
		{
			this.ec = ec;
			if (useDefaultMovement)
				OnMove += (x, _) => transform.position = x;
			initialized = true;
		}

		void Update()
		{
			if (!initialized || targets.Count == 0) return;

			if (!usesAcceleration)
				accel = maxSpeed;
			else
			{
				accel += ec.EnvironmentTimeScale * accelerationAddend * Time.deltaTime;
				if (accel > maxSpeed)
					accel = maxSpeed;
			}

			Vector3 pos = transform.position;
			pos.y = yOffset;

			float speed = accel * ec.EnvironmentTimeScale * Time.deltaTime;
			if (speed == 0f) return;

			var dist = targets[0] - pos;
			var dir = dist.normalized;
			float magnitude = dist.magnitude;

			float moveSpeed = momentumAddend;
			momentumAddend = 0f;

			if (magnitude >= speed)
				moveSpeed += speed;
			else
			{
				moveSpeed += magnitude;
				momentumAddend = speed - magnitude;
				targets.RemoveAt(0);
			}

			OnMove?.Invoke(pos + dir * moveSpeed, dir);
		}

		public delegate void OnMoveDel(Vector3 newPos, Vector3 newDir);
		public event OnMoveDel OnMove;

		readonly List<Vector3> targets = [];
		public List<Vector3> Targets => targets;
		EnvironmentController ec;

		bool initialized = false;
		float momentumAddend = 0f, accel = 0f;

		public float Acceleration => accel;

		[SerializeField]
		internal float maxSpeed = 1f, accelerationAddend = 1f, yOffset = 0f;

		[SerializeField]
		internal bool usesAcceleration = true;
	}
}