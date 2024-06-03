using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Glubotrony : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			behaviorStateMachine.ChangeState(new GlubotronyState(this));
		}
		public void Step()
		{
			step = !step;
			renderer.targetSprite = step ? sprStep1 : sprStep2;
			StartCoroutine(Walk());
		}

		public void Stand() =>
			renderer.targetSprite = sprIdle;

		IEnumerator Walk()
		{
			isWalking = true;
			
			float stepCool = 0.35f;
			while (stepCool > 0f)
			{
				stepCool -= TimeScale * Time.deltaTime;
				yield return null;
			}
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
			stepCool = 0.5f;
			while (stepCool > 0f)
			{
				stepCool -= TimeScale * Time.deltaTime;
				yield return null;
			}
			navigator.maxSpeed = 0f;
			navigator.SetSpeed(0f);
			isWalking = false;
			Stand();
			yield break;
		}

		[SerializeField]
		internal AnimatedSpriteRotator renderer;

		[SerializeField]
		internal Sprite sprIdle, sprStep1, sprStep2;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		bool step = false, isWalking = false;
		const float speed = 7f;

		public bool IsWalking => isWalking;
	}

	internal class GlubotronyState(Glubotrony gb) : NpcState(gb)
	{
		readonly Glubotrony gb = gb;

		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(gb, 0));
		}

		public override void MadeNavigationDecision()
		{
			base.MadeNavigationDecision();
			isTurning = true;
			if (!gb.Navigator.Entity.ExternalActivity.moveMods.Contains(moveMod))
				gb.Navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
		}

		public override void Update()
		{
			base.Update();

			if (!gb.IsWalking)
			{
				stepCooldown -= gb.TimeScale * Time.deltaTime;
				if (stepCooldown < 0f)
				{
					stepCooldown += 1f;
					gb.Step();
					gb.transform.RotateSmoothlyToNextPoint(gb.Navigator.NextPoint, 15f);
					if (isTurning && Vector3.Angle(gb.transform.forward, gb.Navigator.NextPoint - gb.transform.position) <= 22.5f)
					{
						isTurning = false;
						gb.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
					}
				}
			}
		}

		float stepCooldown = 1f;

		bool isTurning = false;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
