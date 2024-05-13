using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.NPCs;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.ObjectCreation;

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
			//npc = CreatorExtensions.CreateNPC<OfficeChair, OfficeChairCustomData>("OfficeChair", 35f, 60f, [RoomCategory.Office, RoomCategory.Faculty], [], "PST_OFC_Name", "PST_OFC_Desc", true, ignorePlayerOnSpawn: true, spriteYOffset:-2f).AddMeta(plug, "OfficeChair", NPCFlags.Standard).value;
			npc = new NPCBuilder<OfficeChair>(plug.Info).SetMinMaxAudioDistance(35f, 60f)
				.AddSpawnableRoomCategories(RoomCategory.Office, RoomCategory.Faculty)
				.IgnorePlayerOnSpawn()
				.SetEnum("OfficeChair")
				.SetName("OfficeChair")
				.AddTrigger()
				.Build()
				.SetupNPCData<OfficeChairCustomData>("OfficeChair", "PST_OFC_Name", "PST_OFC_Desc", -2f);

			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 99999 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 60 });
			/*
			// Happy Holidays
			npc = CreatorExtensions.CreateNPC<HappyHolidays, HappyHolidaysCustomData>("HappyHolidays", 45f, 80f, [RoomCategory.Hall], [], "PST_HapH_Name", "PST_HapH_Desc", lookerDistance: 125, spriteYOffset: -2f).AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 25 });

			// Classic Gotta Sweep
			npc = CreatorExtensions.CreateCustomNPCFromExistent<GottaSweep, ClassicGottaSweepCustomData>(Character.Sweep, "ClassicGottaSweep").MarkAsReplacement(Character.Sweep);
			npc.AddMetaPrefab();
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 100 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 90 });

			// Crazy Clock
			npc = CreatorExtensions.CreateNPC<CrazyClock, CrazyClockCustomData>("CrazyClock", 55f, 90f, [RoomCategory.Hall], [], "PST_CC_Name", "PST_CC_Desc", ignorePlayerOnSpawn:true, ignoreBelts:true, hasTrigger: false, lookerDistance: 55f, grounded: false)
				.AddMeta(plug, NPCFlags.StandardNoCollide).value;
			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 20 });

			// Superintendent
			npc = CreatorExtensions.CreateNPC<Superintendent, SuperintendentCustomData>("Superintendent", 110f, 140f, [RoomCategory.Office, RoomCategory.Class, RoomCategory.Faculty], [], "PST_SI_Name", "PST_SI_Desc", usesHeatMap:true, lookerDistance: 90f, avoidRooms:false, spriteYOffset: -0.5f)
				.AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Let's Drum
			npc = CreatorExtensions.CreateNPC<LetsDrum, LetsDrumCustomData>("Letsdrum", 75f, 110f, [RoomCategory.Hall], [], "PST_DRUM_Name", "PST_DRUM_Desc", lookerDistance: 25f, spriteYOffset: -1.4f).AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 65 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Stunly
			npc = CreatorExtensions.CreateNPC<Stunly, StunlyCustomData>("Stunly", 75f, 100f, [RoomCategory.Hall, RoomCategory.Special], [], "PST_Stunly_Name", "PST_Stunly_Desc", lookerDistance: 60f, spriteYOffset: -1.5f).AddMeta(plug , NPCFlags.Standard).value;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });

			// Pix
			npc = CreatorExtensions.CreateNPC<Pix, PixCustomData>("Pix", 155f, 165f, [RoomCategory.Hall], [], "PST_Pix_Name", "PST_Pix_Desc", lookerDistance: 90f, spriteYOffset:-1f).AddMeta(plug, NPCFlags.Standard).value
				.SetNPCLookerFov(100f);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });
			*/
		}

		

		
	}
}
