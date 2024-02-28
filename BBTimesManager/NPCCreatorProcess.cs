using BBTimes.CustomComponents.CustomDatas;
using BBTimes.Extensions;
using BBTimes.Helpers;
using BBTimes.NPCs;
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
			npc = CreatorExtensions.CreateNPC<OfficeChair, OfficeChairCustomData>("OfficeChair", 20f, 40f, [RoomCategory.Office, RoomCategory.Faculty], [], "PST_OFC_Name", "PST_OFC_Desc", true).AddMeta(plug, "OfficeChair", NPCFlags.Standard).value;
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 1000 }); // This is how it's gonna work
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1000 });
		}

		

		
	}
}
