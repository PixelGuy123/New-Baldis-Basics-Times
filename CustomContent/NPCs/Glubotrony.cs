using BBTimes.CustomComponents.NpcSpecificComponents;
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
			stepAudMan.FlushQueue(true);
			stepAudMan.QueueAudio(audPrepareStep);

			StartCoroutine(Walk());
		}
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
			renderer.targetSprite = sprIdle;
			stepAudMan.FlushQueue(true);
			stepAudMan.PlaySingle(audStep);
			yield break;
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (cooldown <= 0f)
			{
				cooldown += Random.Range(15f, 30f);
				audMan.QueueRandomAudio(audWander);
			}
			cooldown -= TimeScale * Time.deltaTime;
		}

		public void SpillGlue()
		{
			audMan.FlushQueue(true);
			audMan.QueueRandomAudio(audPutGlue);
			Instantiate(gluePre).Initialize(this, transform.position, 0.08f);
			Directions.ReverseList(navigator.currentDirs);
			behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(this, 0));
			SetGuilt(3f, "littering");
		}

		[SerializeField]
		internal AnimatedSpriteRotator renderer;

		[SerializeField]
		internal Sprite sprIdle, sprStep1, sprStep2;

		[SerializeField]
		internal PropagatedAudioManager audMan, stepAudMan;

		[SerializeField]
		internal SoundObject audPrepareStep, audStep;

		[SerializeField]
		internal SoundObject[] audWander;

		[SerializeField]
		internal SoundObject[] audPutGlue;

		[SerializeField]
		internal Glue gluePre;

		bool step = false, isWalking = false;
		float cooldown = 15f;
		const float speed = 12f;

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

		public override void Update()
		{
			base.Update();
			float angle = Vector3.Angle(gb.transform.forward, gb.Navigator.NextPoint - gb.transform.position); // Basically should stop or not to turn
			if (angle <= 5f)
			{
				if (isTurning)
				{
					isTurning = false;
					gb.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);
				}
			}
			else
			{
				if (!isTurning)
				{
					isTurning = true;
					if (!gb.Navigator.Entity.ExternalActivity.moveMods.Contains(moveMod))
						gb.Navigator.Entity.ExternalActivity.moveMods.Add(moveMod);
				}
			}

			if (!gb.IsWalking)
			{
				stepCooldown -= gb.TimeScale * Time.deltaTime;
				if (stepCooldown < 0f)
				{
					stepCooldown += 1f;
					gb.Step();
					gb.transform.RotateSmoothlyToNextPoint(gb.Navigator.NextPoint, 10f);
				}
			}

			spillGlueCooldown -= gb.TimeScale * Time.deltaTime;
		}

		public override void PlayerInSight(PlayerManager player)
		{
			base.PlayerInSight(player);
			if (spillGlueCooldown <= 0f && !player.Tagged)
			{
				gb.SpillGlue();
				spillGlueCooldown = 40f;
			}
		}

		float stepCooldown = 1f;

		float spillGlueCooldown = 0f;

		bool isTurning = false;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
