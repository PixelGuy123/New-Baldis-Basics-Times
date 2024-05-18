﻿using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.Events;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void CreateEvents(BaseUnityPlugin plug)
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			RandomEvent e;

			// Principal out
			e = new RandomEventBuilder<PrincipalOut>(plug.Info)
				.SetEnum("Principalout")
				.SetMinMaxTime(40f, 80f)
				.AddRequiredCharacters([Character.Principal, ..GetReplacementNPCs(Character.Principal)])
				.SetDescription("Event_PriOut")
				.SetName("Principalout")
				.Build()
				.SetupEvent<CustomEventData>();

				//CreatorExtensions.CreateEvent<PrincipalOut, CustomEventData>("Principalout", "Event_PriOut", 40f, 80f, []).AddMeta(plug, RandomEventFlags.CharacterSpecific).value;
			floorDatas[0].Events.Add(new() { selection = e, weight = 65 });
			floorDatas[1].Events.Add(new() { selection = e, weight = 55 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 75 });

			// BlackOut
			e = new RandomEventBuilder<BlackOut>(plug.Info)
				.SetEnum("Blackout")
				.SetMinMaxTime(55f, 90f)
				.SetDescription("Event_BlackOut")
				.SetName("Blackout")
				.Build()
				.SetupEvent<BlackOutCustomData>();


			//CreatorExtensions.CreateEvent<BlackOut, BlackOutCustomData>("Blackout", "Event_BlackOut", 55f, 90f, []).AddMeta(plug, RandomEventFlags.None).value;
			floorDatas[2].Events.Add(new() { selection = e, weight = 55 });

			// Freezing Event
			e = new RandomEventBuilder<FrozenEvent>(plug.Info)
				.SetEnum("Frozenschool")
				.SetMinMaxTime(45f, 60f)
				.SetDescription("Event_FreezeEvent")
				.SetName("FrozenEvent")
				.Build()
				.SetupEvent<FrozenEventCustomData>();


			//CreatorExtensions.CreateEvent<FrozenEvent, FrozenEventCustomData>("FrozenEvent", "Event_FreezeEvent", 45f, 60f, []).AddMeta(plug, RandomEventFlags.None).value;
			floorDatas[3].Events.Add(new() { selection = e, weight = 999999 });
		}

		

		
	}
}
