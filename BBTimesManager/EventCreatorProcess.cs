using BBTimes.CustomContent.Events;
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

			// Office Chair
			e = CreatorExtensions.CreateEvent<PrincipalOut>("PrincipalOut", "Event_PriOut", 40f, 80f, []).AddMeta(plug, RandomEventFlags.CharacterSpecific).value;
			floorDatas[0].Events.Add(new() { selection = e, weight = 5022 }); // This is how it's gonna work
		}

		

		
	}
}
