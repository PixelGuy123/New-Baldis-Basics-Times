using BBTimes.CustomComponents.CustomDatas;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI.Components;

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
				var data = npc.GetComponent<CustomNPCData>();
				if (npc.Navigator.enabled && (npc.Character == Character.Principal || (data != null && data.npcsBeingReplaced.Contains(Character.Principal)))) // Reminder to change for 
				{
					NavigationState_PrincipalOut navigationState_PartyEvent = new(npc, 88, office);
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

	public class NavigationState_PrincipalOut(NPC npc, int priority, RoomController office) : NavigationState(npc, priority)
	{
		readonly RoomController office = office;

		readonly MovementModifier moveMod = new(Vector3.zero, 5f);

		readonly ValueModifier blindMod = new(0f, 0f);

		bool reachedOffice = false;
		public override void Enter()
		{
			base.Enter();
			npc.Navigator.FindPath(office.RandomEventSafeCellNoGarbage().FloorWorldPosition);
			npc.Navigator.Am.moveMods.Add(moveMod);
			npc.GetNPCContainer().AddLookerMod(blindMod);
		}

		public override void DestinationEmpty()
		{
			base.DestinationEmpty();
			npc.Navigator.FindPath(office.RandomEventSafeCellNoGarbage().FloorWorldPosition);
			if (!reachedOffice && npc.ec.CellFromPosition(npc.transform.position).TileMatches(office))
			{
				npc.Navigator.Am.moveMods.Remove(moveMod);
				reachedOffice = true;
			}
		}

		public void End()
		{
			priority = 0;
			npc.behaviorStateMachine.RestoreNavigationState();
			npc.GetNPCContainer().RemoveLookerMod(blindMod);
		}
	}
}
