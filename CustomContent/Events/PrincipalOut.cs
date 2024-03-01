using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
	public class PrincipalOut : RandomEvent
	{
		public override void Begin()
		{
			base.Begin();
			office = ec.offices[crng.Next(0, ec.offices.Count)];
			foreach (NPC npc in ec.Npcs)
			{
				if (npc.Navigator.enabled && !npc.Aggroed)
				{
					NavigationState_PrincipalOut navigationState_PartyEvent = new(npc, 999, office.RandomEventSafeCellNoGarbage().FloorWorldPosition);
					navigationStates.Add(navigationState_PartyEvent);
					npc.navigationStateMachine.ChangeState(navigationState_PartyEvent);
				}
			}
		}

		public override void End()
		{
			base.End();
			foreach (var state in navigationStates)
				state.End();
			
		}

		RoomController office;
		readonly List<NavigationState_PrincipalOut> navigationStates = [];
	}

	public class NavigationState_PrincipalOut(NPC npc, int priority, Vector3 randomPos) : NavigationState(npc, priority)
	{
		public override void Enter()
		{
			base.Enter();
			destination = randomPos;
			npc.Navigator.FindPath(destination);
		}

		public void End()
		{
			priority = 0;
			npc.behaviorStateMachine.RestoreNavigationState();
		}
	}
}
