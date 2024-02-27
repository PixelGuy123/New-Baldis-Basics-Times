using UnityEngine;

namespace BBTimes.NPCs
{
	public class OfficeChair : NPC // Npc here
	{
		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new OfficeChair_Moving(this));

			navigator.maxSpeed = normSpeed;
			navigator.SetSpeed(normSpeed);
		}

		const float normSpeed = 10f;
	}

	public class OfficeChair_StateBase(OfficeChair office) : NpcState(office) // A default npc state
	{
		protected OfficeChair chair = office;
	}

	public class OfficeChair_Moving(OfficeChair office) : OfficeChair_StateBase(office) // A basic moving npc state
	{
		public override void Enter()
		{
			base.Enter();
			if (!chair.Navigator.HasDestination)
				ChangeNavigationState(new NavigationState_WanderRandom(chair, 0));
			
		}
	}
}
