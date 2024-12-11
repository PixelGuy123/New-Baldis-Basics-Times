using System.Collections.Generic;
using UnityEngine;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI.Components;
using BBTimes.CustomComponents;
using BBTimes.Extensions;


namespace BBTimes.CustomContent.Events
{
    public class PrincipalOut : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("baldi_walking.wav", "Event_PriOut0", SoundType.Voice, Color.green);
			eventIntro.additionalKeys = [
				new() {time = 1.034f, key = "Event_PriOut1"},
				new() {time = 3.93f, key = "Event_PriOut2"}
				];
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------
		public override void Begin()
		{
			base.Begin();
			office = ec.offices[crng.Next(0, ec.offices.Count)];
			foreach (NPC npc in ec.Npcs)
			{
				var data = npc.GetComponent<INPCPrefab>();
				if (npc.Navigator.enabled && (npc.Character == Character.Principal || (data != null && data.ReplacesCharacter(Character.Principal)))) // Reminder to change for 
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
			npc.Navigator.Am.moveMods.Remove(moveMod);
		}
	}
}
