﻿using BBTimes.CustomContent.Events;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
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
			e = CreatorExtensions.CreateEvent<PrincipalOut>("PrincipalOut", "Event_PriOut", 40f, 80f, []).AddMeta(plug, RandomEventFlags.CharacterSpecific).value;
			floorDatas[0].Events.Add(new() { selection = e, weight = 65 }); // This is how it's gonna work
			floorDatas[1].Events.Add(new() { selection = e, weight = 55 }); // This is how it's gonna work
			floorDatas[3].Events.Add(new() { selection = e, weight = 75 }); // This is how it's gonna work
		}

		

		
	}
}
