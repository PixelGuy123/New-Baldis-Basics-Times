using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.Misc
{
    public class SketchEntity : NPC
    {
		[SerializeField]
		internal SoundObject audDoingTrouble;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal AnimationComponent animComp;

		[SerializeField]
		internal MovementModifier fightMod = new(Vector3.zero, 0f);

		[SerializeField]
		internal float speed = 35f, runSpeed = 75f, fightTimer = 25f, lifeTime = 60f;

		Entity targetedEntity;
		bool isOnAFight = false;

		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new SketchEntity_Wander(this));
		}

		public void TargetNPC(bool target)
		{
			float speed = target ? runSpeed : this.speed;
			navigator.maxSpeed = speed;
			navigator.SetSpeed(speed);
		}

		public void GetIntoAFight(Entity entity)
		{
			navigator.SetSpeed(0f);
			navigator.maxSpeed = 0f;
			isOnAFight = true;
			targetedEntity = entity;
			navigator.Entity.SetVisible(false);
			animComp.gameObject.SetActive(true);
			animComp.Initialize(ec);

			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audDoingTrouble);

			entity.SetInteractionState(false);
			entity.SetBlinded(true);
			entity.ExternalActivity.moveMods.Add(fightMod);
		}

		public void EndFight()
		{
			if (targetedEntity)
			{
				targetedEntity.ExternalActivity.moveMods.Remove(fightMod);
				targetedEntity.SetInteractionState(true);
				targetedEntity.SetBlinded(false);
			}
			Despawn();
		}

		public override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (isOnAFight)
			{
				if (!targetedEntity)
				{
					Despawn();
					return;
				}
				navigator.Entity.Teleport(targetedEntity.transform.position);
				return;
			}

			lifeTime -= TimeScale * Time.deltaTime;
			if (lifeTime <= 0f)
				Despawn();
		}
	}

	internal class SketchEntity_StateBase(SketchEntity ske) : NpcState(ske)
	{
		protected SketchEntity ske = ske;
	}

	internal class SketchEntity_Wander(SketchEntity ske) : SketchEntity_StateBase(ske)
	{
		public override void Enter()
		{
			base.Enter();
			ske.TargetNPC(false);
			ChangeNavigationState(new NavigationState_WanderRandom(ske, 0));
		}

		public override void Update()
		{
			base.Update();
			if (ske.Blinded)
				return;
			NPC foundNPC = null;
			float npcDistance = -1f;
			for (int i = 0; i < ske.ec.Npcs.Count; i++)
			{
				if (ske != ske.ec.Npcs[i] && ske.ec.Npcs[i].Navigator.isActiveAndEnabled && !ske.ec.Npcs[i].Navigator.Entity.InteractionDisabled)
				{
					if (ske.looker.RaycastNPC(ske.ec.Npcs[i]))
					{
						float distance = Vector3.Distance(ske.ec.Npcs[i].transform.position, ske.transform.position);
						if (npcDistance == -1f || distance < npcDistance)
						{
							foundNPC = ske.ec.Npcs[i];
							npcDistance = distance;
						}
					}
				}
			}

			if (npcDistance != -1f) // Find the closest NPC available to start targetting
			{
				ske.behaviorStateMachine.ChangeState(new SketchEntity_TargetNPC(ske, foundNPC));
			}
		}
	}

	internal class SketchEntity_TargetNPC(SketchEntity ske, NPC target) : SketchEntity_StateBase(ske)
	{
		protected NPC target = target;
		NavigationState_TargetPosition tarPos;
		public override void Enter()
		{
			base.Enter();
			ske.TargetNPC(true);
			tarPos = new(ske, 63, target.transform.position);
			ChangeNavigationState(tarPos);
		}

		public override void Update()
		{
			base.Update();
			if (target == null || target.Navigator.Entity.InteractionDisabled)
				ske.behaviorStateMachine.ChangeState(new SketchEntity_Wander(ske));
			else
				tarPos.UpdatePosition(target.transform.position);

		}

		public override void OnStateTriggerStay(Collider other)
		{
			base.OnStateTriggerStay(other);
			if (other.gameObject == target.gameObject)
			{
				ske.behaviorStateMachine.ChangeState(new SketchEntity_FightNPC(ske, target));
			}
		}
	}

	internal class SketchEntity_FightNPC(SketchEntity ske, NPC target) : SketchEntity_StateBase(ske)
	{
		protected NPC target = target;

		float timer = ske.fightTimer;
		public override void Enter()
		{
			base.Enter();
			ske.GetIntoAFight(target.Navigator.Entity);
		}

		public override void Update()
		{
			base.Update();
			timer -= ske.TimeScale * Time.deltaTime;
			if (timer <= 0f)
				ske.EndFight();
		}
	}
}
