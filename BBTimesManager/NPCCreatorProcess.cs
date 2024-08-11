using BBTimes.CustomComponents.CustomDatas;
using BBTimes.CustomContent.NPCs;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;

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
				.SetName("PST_OFC_Name")
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
				.SetName("PST_HapH_Name")
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
				.SetName("PST_CC_Name")
				.AddLooker()
				.SetMaxSightDistance(55)
				.IgnoreBelts()
				.SetStationary()
				.Build()
				.SetupNPCData<CrazyClockCustomData>("CrazyClock", "PST_CC_Name", "PST_CC_Desc", 0f);
			//CreatorExtensions.CreateNPC<CrazyClock, CrazyClockCustomData>("CrazyClock", 55f, 90f, [RoomCategory.Hall], [], "PST_CC_Name", "PST_CC_Desc", ignorePlayerOnSpawn:true, ignoreBelts:true, hasTrigger: false, lookerDistance: 55f, grounded: false).AddMeta(plug, NPCFlags.StandardNoCollide).value;
			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 20 });


			// Superintendent
			npc = new NPCBuilder<Superintendent>(plug.Info)
				.SetMinMaxAudioDistance(110f, 140f)
				.AddSpawnableRoomCategories(RoomCategory.Hall, RoomCategory.Class, RoomCategory.Faculty)
				.SetEnum("Superintendent")
				.SetName("PST_SI_Name")
				.AddLooker()
				.AddTrigger()
				.AddHeatmap()
				.Build()
				.SetupNPCData<SuperintendentCustomData>("Superintendent", "PST_SI_Name", "PST_SI_Desc", -0.5f);
			//CreatorExtensions.CreateNPC<Superintendent, SuperintendentCustomData>("Superintendent", 110f, 140f, [RoomCategory.Office, RoomCategory.Class, RoomCategory.Faculty], [], "PST_SI_Name", "PST_SI_Desc", usesHeatMap:true, lookerDistance: 90f, avoidRooms:false, spriteYOffset: -0.5f).AddMeta(plug, NPCFlags.Standard).value;
			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Stunly
			npc = new NPCBuilder<Stunly>(plug.Info)
				.SetMinMaxAudioDistance(75f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Stunly")
				.SetName("PST_Stunly_Name")
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
				.SetName("PST_Pix_Name")
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
				.SetEnum("ZeroPrize")
				.SetName("PST_0TH_Name")
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.Build()
				.SetupNPCData<ZeroPrizeCustomData>("0thPrize", "PST_0TH_Name", "PST_0TH_Desc", -0.4f)
				.MarkAsReplacement(75, Character.Sweep); // 25

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 1 });

			// Pencil Boy
			npc = new NPCBuilder<PencilBoy>(plug.Info)
				.SetMinMaxAudioDistance(75f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("PencilBoy")
				.SetName("PST_PB_Name")
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
				.SetName("PST_Rollbot_Name")
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
				.SetName("PST_Wch_Name")
				.AddTrigger()
				.AddLooker()
				.EnableAcceleration()
				.Build()
				.SetupNPCData<WatcherCustomData>("Watcher", "PST_Wch_Name", "PST_Wch_Desc", 0f);

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 15 });

			// MGS
			npc = new NPCBuilder<MagicalStudent>(plug.Info)
				.SetMinMaxAudioDistance(400f, 500f)
				.AddSpawnableRoomCategories(RoomCategory.Office)
				.SetEnum("Magicalstudent")
				.SetName("PST_MGS_Name")
				.AddTrigger()
				.AddLooker()
				.AddHeatmap()
				.Build()
				.SetupNPCData<MagicalStudentCustomData>("MagicalStudent", "PST_MGS_Name", "PST_MGS_Desc", -1.91f)
				.MarkAsReplacement(35, Character.Principal); //35
			
			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Superintendent Jr.
			npc = new NPCBuilder<SuperIntendentJr>(plug.Info)
				.SetMinMaxAudioDistance(245f, 365f)
				.AddSpawnableRoomCategories(RoomCategory.Faculty, RoomCategory.Office)
				.SetEnum("Superintendentjr")
				.SetName("PST_Spj_Name")
				.AddTrigger()
				.IgnoreBelts()
				.AddLooker()
				.SetMaxSightDistance(155)
				.Build()
				.SetupNPCData<SuperIntendentJrCustomData>("SuperintendentJr", "PST_Spj_Name", "PST_Spj_Desc", -1.73f);

			npc.Navigator.SetRoomAvoidance(false);
			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 999999 });
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 65 });

			// Leapy
			npc = new NPCBuilder<Leapy>(plug.Info)
				.SetMinMaxAudioDistance(100f, 135f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Leapy")
				.SetName("PST_Leapy_Name")
				.AddTrigger()
				.Build()
				.SetupNPCData<LeapyCustomData>("Leapy", "PST_Leapy_Name", "PST_Leapy_Desc", -1.1f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Glue boy
			npc = new NPCBuilder<Glubotrony>(plug.Info)
				.SetMinMaxAudioDistance(75f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Glubotrony")
				.SetName("PST_Gboy_Name")
				.AddLooker()
				.SetMaxSightDistance(45)
				.SetFOV(110f)
				.AddTrigger()
				.DisableAutoRotation()
				.Build()
				.SetupNPCData<GlubotronyCustomData>("Glubotrony", "PST_Gboy_Name", "PST_Gboy_Desc", -0.7f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 28 });

			// Dribble
			npc = new NPCBuilder<Dribble>(plug.Info)
				.SetMinMaxAudioDistance(85f, 135f)
				.SetEnum("Dribble")
				.SetName("PST_Dribble_Name")
				.AddLooker()
				.IgnoreBelts()
				.AddTrigger()
				.Build()
				.SetupNPCData<DribbleCustomData>("Dribble", "PST_Dribble_Name", "PST_Dribble_Desc", -0.7f)
				.MarkAsReplacement(45, Character.DrReflex);

			npc.spawnableRooms.Clear();
			npc.Navigator.SetRoomAvoidance(false);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Bubbly
			npc = new NPCBuilder<Bubbly>(plug.Info)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMinMaxAudioDistance(90f, 110f)
				.SetEnum("Bubbly")
				.SetName("PST_Bubbly_Name")
				.AddTrigger()
				.Build()
				.SetupNPCData<BubblyCustomData>("Bubbly", "PST_Bubbly_Name", "PST_Bubbly_Desc", -1.03f)
				.MarkAsReplacement(55, Character.Cumulo);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Phawillow
			npc = new NPCBuilder<Phawillow>(plug.Info)
				.SetMinMaxAudioDistance(100f, 200f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Phawillow")
				.SetName("PST_Phawillow_Name")
				.AddLooker()
				.AddTrigger()
				.SetAirborne()
				.IgnoreBelts()
				.Build()
				.SetupNPCData<PhawillowCustomData>("Phawillow", "PST_Phawillow_Name", "PST_Phawillow_Desc", 0f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 45 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 25 });

			// Faker
			npc = new NPCBuilder<Faker>(plug.Info)
				.SetMinMaxAudioDistance(90f, 140f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Faker")
				.SetName("PST_Faker_Name")
				.AddLooker()
				.AddTrigger()
				.Build()
				.SetupNPCData<FakerCustomData>("Faker", "PST_Faker_Name", "PST_Faker_Desc", -1.36f)
				.MarkAsReplacement(99999, Character.LookAt); // 45

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 1 });
		}

		

		
	}
}
