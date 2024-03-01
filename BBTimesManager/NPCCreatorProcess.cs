using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.Registers;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void CreateNPCs(BaseUnityPlugin plug)
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			NPC npc;

			// Office Chair
			npc = CreatorExtensions.CreateNPC<OfficeChair, OfficeChairCustomData>("OfficeChair", 35f, 60f, [RoomCategory.Office, RoomCategory.Faculty], [], "PST_OFC_Name", "PST_OFC_Desc", true, ignorePlayerOnSpawn: true).AddMeta(plug, "OfficeChair", NPCFlags.Standard).value;
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 50 }); // This is how it's gonna work
		}

		

		
	}
}
