using UnityEngine;

namespace BBTimes.CustomContent.NPCs
{
	public class Mugh : NPC
	{
		public override void Initialize()
		{
			base.Initialize();
			behaviorStateMachine.ChangeState(new Mugh_Wandering(this));
		}
	}

	internal class Mugh_StateBase(Mugh mu) : NpcState(mu)
	{
		protected Mugh mu = mu;
	}

	internal class Mugh_Wandering(Mugh mu) : Mugh_StateBase(mu)
	{
		public override void Enter()
		{
			base.Enter();
			ChangeNavigationState(new NavigationState_WanderRandom(mu, 0));
		}
	}
}
