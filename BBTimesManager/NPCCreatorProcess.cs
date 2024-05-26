using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.NPCs;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI;
using MTM101BaldAPI.ObjectCreation;
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
			//npc = CreatorExtensions.CreateNPC<OfficeChair, OfficeChairCustomData>("OfficeChair", 35f, 60f, [RoomCategory.Office, RoomCategory.Faculty], [], "PST_OFC_Name", "PST_OFC_Desc", true, ignorePlayerOnSpawn: true, spriteYOffset:-2f).AddMeta(plug, "OfficeChair", NPCFlags.Standard).value;
			npc = new NPCBuilder<OfficeChair>(plug.Info)
				.SetMinMaxAudioDistance(35f, 60f)
				.AddSpawnableRoomCategories(RoomCategory.Office, RoomCategory.Faculty)
				.IgnorePlayerOnSpawn()
				.SetEnum("OfficeChair")
				.SetName("OfficeChair")
				.AddTrigger()
				.Build()
				.SetupNPCData<OfficeChairCustomData>("OfficeChair", "PST_OFC_Name", "PST_OFC_Desc", -2f);

			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 60 });
			
			// Happy Holidays
			npc = new NPCBuilder<HappyHolidays>(plug.Info)
				.SetMinMaxAudioDistance(45f, 80f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("HappyHolidays")
				.SetName("HappyHolidays")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(125)
				.Build()
				.SetupNPCData<HappyHolidaysCustomData>("HappyHolidays", "PST_HapH_Name", "PST_HapH_Desc", -2f);
			//CreatorExtensions.CreateNPC<HappyHolidays, HappyHolidaysCustomData>("HappyHolidays", 45f, 80f, [RoomCategory.Hall], [], "PST_HapH_Name", "PST_HapH_Desc", lookerDistance: 125, spriteYOffset: -2f).AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 25 });

			// Classic Gotta Sweep
			npc = CreatorExtensions.CreateCustomNPCFromExistent<GottaSweep, ClassicGottaSweepCustomData>(Character.Sweep, "ClassicGottaSweep").MarkAsReplacement(45, Character.Sweep);
			npc.AddMetaPrefab();
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 100 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 90 });

			// Crazy Clock
			npc = new NPCBuilder<CrazyClock>(plug.Info)
				.SetMinMaxAudioDistance(55f, 90f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.IgnorePlayerOnSpawn()
				.SetEnum("CrazyClock")
				.SetName("CrazyClock")
				.AddLooker()
				.SetMaxSightDistance(55)
				.IgnoreBelts()
				//.SetStationary() temporarily broken by the api, so HUAfdH8an8An8anfahsundau8fja8
				.Build()
				.SetupNPCData<CrazyClockCustomData>("CrazyClock", "PST_CC_Name", "PST_CC_Desc", -2f);
			//CreatorExtensions.CreateNPC<CrazyClock, CrazyClockCustomData>("CrazyClock", 55f, 90f, [RoomCategory.Hall], [], "PST_CC_Name", "PST_CC_Desc", ignorePlayerOnSpawn:true, ignoreBelts:true, hasTrigger: false, lookerDistance: 55f, grounded: false).AddMeta(plug, NPCFlags.StandardNoCollide).value;
			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 20 });


			// Superintendent
			npc = new NPCBuilder<Superintendent>(plug.Info)
				.SetMinMaxAudioDistance(110f, 140f)
				.AddSpawnableRoomCategories(RoomCategory.Hall, RoomCategory.Class, RoomCategory.Faculty)
				.SetEnum("Superintendent")
				.SetName("Superintendent")
				.AddLooker()
				.AddTrigger()
				.AddHeatmap()
				.Build()
				.SetupNPCData<SuperintendentCustomData>("Superintendent", "PST_SI_Name", "PST_SI_Desc", -0.5f);
			//CreatorExtensions.CreateNPC<Superintendent, SuperintendentCustomData>("Superintendent", 110f, 140f, [RoomCategory.Office, RoomCategory.Class, RoomCategory.Faculty], [], "PST_SI_Name", "PST_SI_Desc", usesHeatMap:true, lookerDistance: 90f, avoidRooms:false, spriteYOffset: -0.5f).AddMeta(plug, NPCFlags.Standard).value;
			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Let's Drum
			npc = new NPCBuilder<LetsDrum>(plug.Info)
				.SetMinMaxAudioDistance(75f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Letsdrum")
				.SetName("Letsdrum")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(25)
				.Build()
				.SetupNPCData<LetsDrumCustomData>("Letsdrum", "PST_DRUM_Name", "PST_DRUM_Desc", -1.4f);
			//CreatorExtensions.CreateNPC<LetsDrum, LetsDrumCustomData>("Letsdrum", 75f, 110f, [RoomCategory.Hall], [], "PST_DRUM_Name", "PST_DRUM_Desc", lookerDistance: 25f, spriteYOffset: -1.4f).AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 65 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Stunly
			npc = new NPCBuilder<Stunly>(plug.Info)
				.SetMinMaxAudioDistance(75f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Stunly")
				.SetName("Stunly")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(60)
				.Build()
				.SetupNPCData<StunlyCustomData>("Stunly", "PST_Stunly_Name", "PST_Stunly_Desc", -1.5f);
			//CreatorExtensions.CreateNPC<Stunly, StunlyCustomData>("Stunly", 75f, 100f, [RoomCategory.Hall, RoomCategory.Special], [], "PST_Stunly_Name", "PST_Stunly_Desc", lookerDistance: 60f, spriteYOffset: -1.5f).AddMeta(plug , NPCFlags.Standard).value;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });

			// Pix
			npc = new NPCBuilder<Pix>(plug.Info)
				.SetMinMaxAudioDistance(155f, 165f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Pix")
				.SetName("Pix")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(90)
				.SetFOV(100f)
				.Build()
				.SetupNPCData<PixCustomData>("Pix", "PST_Pix_Name", "PST_Pix_Desc", -1f);

			//CreatorExtensions.CreateNPC<Pix, PixCustomData>("Pix", 155f, 165f, [RoomCategory.Hall], [], "PST_Pix_Name", "PST_Pix_Desc", lookerDistance: 90f, spriteYOffset:-1f).AddMeta(plug, NPCFlags.Standard).value.SetNPCLookerFov(100f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });

			// Zero Prize

			npc = new NPCBuilder<ZeroPrize>(plug.Info)
				.SetMinMaxAudioDistance(135f, 175f)
				.AddSpawnableRoomCategories(RoomCategory.Special)
				.SetEnum("ZeroPrize")
				.SetName("0thPrize")
				.AddPotentialRoomAssets(Character.Sweep.GetFirstInstance().potentialRoomAssets) // DUUUUH, THAT'S WHY I DIDN'T FOUND THE ROOM
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.Build()
				.SetupNPCData<ZeroPrizeCustomData>("0thPrize", "PST_0TH_Name", "PST_0TH_Desc", -1.14f)
				.MarkAsReplacement(15, Character.Sweep); // 25

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 1 });

			// Pencil Boy
			npc = new NPCBuilder<PencilBoy>(plug.Info)
				.SetMinMaxAudioDistance(75f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("PencilBoy")
				.SetName("PencilBoy")
				.AddLooker()
				.SetMaxSightDistance(45f)
				.AddTrigger()
				.Build()
				.SetupNPCData<PencilBoyCustomData>("PencilBoy", "PST_PB_Name", "PST_PB_Desc", -1.77f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Rolling Bot
			npc = new NPCBuilder<RollingBot>(plug.Info)
				.SetMinMaxAudioDistance(55f, 135f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Rollingbot")
				.SetName("RollingBot")
				.AddTrigger()
				.DisableAutoRotation()
				.Build()
				.SetupNPCData<RollingBotCustomData>("RollingBot", "PST_Rollbot_Name", "PST_Rollbot_Desc", -1.88f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 65 });

			// Watcher
			npc = new NPCBuilder<Watcher>(plug.Info)
				.SetMinMaxAudioDistance(75f, 185f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Watcher")
				.SetName("Watcher")
				.AddTrigger()
				.AddLooker()
				.EnableAcceleration()
				.Build()
				.SetupNPCData<WatcherCustomData>("Watcher", "PST_Wch_Name", "PST_Wch_Desc", 0f);

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 15 });

			// MGS
			npc = new NPCBuilder<MagicalStudent>(plug.Info)
				.SetMinMaxAudioDistance(175f, 275f)
				.AddSpawnableRoomCategories(RoomCategory.Office)
				.SetEnum("Magicalstudent")
				.SetName("MagicalStudent")
				.AddTrigger()
				.AddLooker()
				.AddHeatmap()
				.IgnorePlayerOnSpawn()
				.Build()
				.SetupNPCData<MagicalStudentCustomData>("MagicalStudent", "PST_MGS_Name", "PST_MGS_Desc", -1.91f)
				.MarkAsReplacement(35, Character.Principal);
			
			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Superintendent Jr.
			npc = new NPCBuilder<SuperIntendentJr>(plug.Info)
				.SetMinMaxAudioDistance(245f, 365f)
				.AddSpawnableRoomCategories(RoomCategory.Faculty, RoomCategory.Office)
				.SetEnum("Superintendentjr")
				.SetName("SuperintendentJr")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(155)
				.Build()
				.SetupNPCData<SuperIntendentJrCustomData>("SuperintendentJr", "PST_MGS_Name", "PST_MGS_Desc", -1.91f);

			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 999999 });
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 65 });
		}

		

		
	}
}
