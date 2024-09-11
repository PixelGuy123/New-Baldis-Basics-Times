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
		static void CreateNPCs()
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
				.SetName("Office Chair")
				.SetMetaName("PST_OFC_Name")
				.AddTrigger()
				.Build()
				.SetupNPCData("OfficeChair", "PST_OFC_Name", "PST_OFC_Desc", -2f);

			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 60 });
			
			// Happy Holidays
			npc = new NPCBuilder<HappyHolidays>(plug.Info)
				.SetMinMaxAudioDistance(45f, 80f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("HappyHolidays")
				.SetName("Happy Holidays")
				.SetMetaName("PST_HapH_Name")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(125)
				.Build()
				.SetupNPCData("HappyHolidays", "PST_HapH_Name", "PST_HapH_Desc", -2f);
			//CreatorExtensions.CreateNPC<HappyHolidays, HappyHolidaysCustomData>("HappyHolidays", 45f, 80f, [RoomCategory.Hall], [], "PST_HapH_Name", "PST_HapH_Desc", lookerDistance: 125, spriteYOffset: -2f).AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 25 });

			// Classic Gotta Sweep
			npc = CreatorExtensions.CreateCustomNPCFromExistent<GottaSweep, ClassicGottaSweep>(Character.Sweep, "oldsweep", "ClassicGottaSweep").MarkAsReplacement(45, Character.Sweep);
			npc.AddMetaPrefab();
			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 100 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 90 });

			// Crazy Clock
			npc = new NPCBuilder<CrazyClock>(plug.Info)
				.SetMinMaxAudioDistance(55f, 90f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.IgnorePlayerOnSpawn()
				.SetEnum("CrazyClock")
				.SetName("Crazy Clock")
				.SetMetaName("PST_CC_Name")
				.AddLooker()
				.SetMaxSightDistance(55)
				.IgnoreBelts()
				.SetStationary()
				.Build()
				.SetupNPCData("CrazyClock", "PST_CC_Name", "PST_CC_Desc", 0f);
			//CreatorExtensions.CreateNPC<CrazyClock, CrazyClockCustomData>("CrazyClock", 55f, 90f, [RoomCategory.Hall], [], "PST_CC_Name", "PST_CC_Desc", ignorePlayerOnSpawn:true, ignoreBelts:true, hasTrigger: false, lookerDistance: 55f, grounded: false).AddMeta(plug, NPCFlags.StandardNoCollide).value;
			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 20 });


			// Superintendent
			npc = new NPCBuilder<Superintendent>(plug.Info)
				.SetMinMaxAudioDistance(110f, 140f)
				.AddSpawnableRoomCategories(RoomCategory.Hall, RoomCategory.Class, RoomCategory.Faculty)
				.SetEnum("Superintendent")
				.SetName("Superintendent")
				.SetMetaName("PST_SI_Name")
				.AddLooker()
				.AddTrigger()
				.AddHeatmap()
				.Build()
				.SetupNPCData("Superintendent", "PST_SI_Name", "PST_SI_Desc", -0.5f);
			//CreatorExtensions.CreateNPC<Superintendent, SuperintendentCustomData>("Superintendent", 110f, 140f, [RoomCategory.Office, RoomCategory.Class, RoomCategory.Faculty], [], "PST_SI_Name", "PST_SI_Desc", usesHeatMap:true, lookerDistance: 90f, avoidRooms:false, spriteYOffset: -0.5f).AddMeta(plug, NPCFlags.Standard).value;
			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Stunly
			npc = new NPCBuilder<Stunly>(plug.Info)
				.SetMinMaxAudioDistance(75f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Stunly")
				.SetName("Stunly")
				.SetMetaName("PST_Stunly_Name")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(60)
				.Build()
				.SetupNPCData("Stunly", "PST_Stunly_Name", "PST_Stunly_Desc", -1.5f);
			//CreatorExtensions.CreateNPC<Stunly, StunlyCustomData>("Stunly", 75f, 100f, [RoomCategory.Hall, RoomCategory.Special], [], "PST_Stunly_Name", "PST_Stunly_Desc", lookerDistance: 60f, spriteYOffset: -1.5f).AddMeta(plug , NPCFlags.Standard).value;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });

			// Pix
			npc = new NPCBuilder<Pix>(plug.Info)
				.SetMinMaxAudioDistance(155f, 165f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Pix")
				.SetName("Pix")
				.SetMetaName("PST_Pix_Name")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(90)
				.SetFOV(100f)
				.Build()
				.SetupNPCData("Pix", "PST_Pix_Name", "PST_Pix_Desc", -1f);

			//CreatorExtensions.CreateNPC<Pix, PixCustomData>("Pix", 155f, 165f, [RoomCategory.Hall], [], "PST_Pix_Name", "PST_Pix_Desc", lookerDistance: 90f, spriteYOffset:-1f).AddMeta(plug, NPCFlags.Standard).value.SetNPCLookerFov(100f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });

			// Zero Prize

			npc = new NPCBuilder<ZeroPrize>(plug.Info)
				.SetMinMaxAudioDistance(135f, 175f)
				.SetEnum("ZeroPrize")
				.SetName("ZeroPrize")
				.SetMetaName("PST_0TH_Name")
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.Build()
				.SetupNPCData("0thPrize", "PST_0TH_Name", "PST_0TH_Desc", -0.4f)
				.MarkAsReplacement(75, Character.Sweep); // 25

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 1 });

			// Pencil Boy
			npc = new NPCBuilder<PencilBoy>(plug.Info)
				.SetMinMaxAudioDistance(75f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("PencilBoy")
				.SetName("Pencil Boy")
				.SetMetaName("PST_PB_Name")
				.AddLooker()
				.SetMaxSightDistance(45f)
				.AddTrigger()
				.Build()
				.SetupNPCData("PencilBoy", "PST_PB_Name", "PST_PB_Desc", -1.77f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Rolling Bot
			npc = new NPCBuilder<RollingBot>(plug.Info)
				.SetMinMaxAudioDistance(55f, 135f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Rollingbot")
				.SetName("Rollingbot")
				.SetMetaName("PST_Rollbot_Name")
				.AddTrigger()
				.DisableAutoRotation()
				.Build()
				.SetupNPCData("RollingBot", "PST_Rollbot_Name", "PST_Rollbot_Desc", -1.88f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 65 });

			// Watcher
			npc = new NPCBuilder<Watcher>(plug.Info)
				.SetMinMaxAudioDistance(75f, 185f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Watcher")
				.SetName("Watcher")
				.SetMetaName("PST_Wch_Name")
				.AddTrigger()
				.AddLooker()
				.EnableAcceleration()
				.Build()
				.SetupNPCData("Watcher", "PST_Wch_Name", "PST_Wch_Desc", 0f);
			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 15 });

			// MGS
			npc = new NPCBuilder<MagicalStudent>(plug.Info)
				.SetMinMaxAudioDistance(400f, 500f)
				.AddSpawnableRoomCategories(RoomCategory.Office)
				.SetEnum("Magicalstudent")
				.SetName("Magicalstudent")
				.SetMetaName("PST_MGS_Name")
				.AddTrigger()
				.AddLooker()
				.AddHeatmap()
				.Build()
				.SetupNPCData("MagicalStudent", "PST_MGS_Name", "PST_MGS_Desc", -1.91f)
				.MarkAsReplacement(35, Character.Principal); //35
			
			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Superintendent Jr.
			npc = new NPCBuilder<SuperIntendentJr>(plug.Info)
				.SetMinMaxAudioDistance(245f, 365f)
				.AddSpawnableRoomCategories(RoomCategory.Faculty, RoomCategory.Office)
				.SetEnum("Superintendentjr")
				.SetName("Superintendentjr")
				.SetMetaName("PST_Spj_Name")
				.AddTrigger()
				.IgnoreBelts()
				.AddLooker()
				.SetMaxSightDistance(155)
				.Build()
				.SetupNPCData("SuperintendentJr", "PST_Spj_Name", "PST_Spj_Desc", -1.73f);

			npc.Navigator.SetRoomAvoidance(false);
			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 25 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 65 });

			// Leapy
			npc = new NPCBuilder<Leapy>(plug.Info)
				.SetMinMaxAudioDistance(100f, 135f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Leapy")
				.SetName("Leapy")
				.SetMetaName("PST_Leapy_Name")
				.AddTrigger()
				.Build()
				.SetupNPCData("Leapy", "PST_Leapy_Name", "PST_Leapy_Desc", -1.1f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 45 });

			// Glue boy
			npc = new NPCBuilder<Glubotrony>(plug.Info)
				.SetMinMaxAudioDistance(75f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Glubotrony")
				.SetName("Glubotrony")
				.SetMetaName("PST_Gboy_Name")
				.AddLooker()
				.SetMaxSightDistance(45)
				.SetFOV(110f)
				.AddTrigger()
				.DisableAutoRotation()
				.Build()
				.SetupNPCData("Glubotrony", "PST_Gboy_Name", "PST_Gboy_Desc", -0.7f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 55 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 28 });

			// Dribble
			npc = new NPCBuilder<Dribble>(plug.Info)
				.SetMinMaxAudioDistance(85f, 135f)
				.SetEnum("Dribble")
				.SetName("Coach Dribble")
				.SetMetaName("PST_Dribble_Name")
				.AddSpawnableRoomCategories(RoomCategory.Special)
				.AddLooker()
				.IgnoreBelts()
				.AddTrigger()
				.Build()
				.SetupNPCData("Dribble", "PST_Dribble_Name", "PST_Dribble_Desc", -0.7f)
				.MarkAsReplacement(45, Character.DrReflex);

			npc.Navigator.SetRoomAvoidance(false);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Bubbly
			npc = new NPCBuilder<Bubbly>(plug.Info)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMinMaxAudioDistance(90f, 110f)
				.SetEnum("Bubbly")
				.SetName("Bubbly")
				.SetMetaName("PST_Bubbly_Name")
				.AddTrigger()
				.Build()
				.SetupNPCData("Bubbly", "PST_Bubbly_Name", "PST_Bubbly_Desc", -1.03f)
				.MarkAsReplacement(55, Character.Cumulo);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Phawillow
			npc = new NPCBuilder<Phawillow>(plug.Info)
				.SetMinMaxAudioDistance(100f, 200f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Phawillow")
				.SetName("Phawillow")
				.SetMetaName("PST_Phawillow_Name")
				.AddLooker()
				.AddTrigger()
				.SetAirborne()
				.IgnoreBelts()
				.Build()
				.SetupNPCData("Phawillow", "PST_Phawillow_Name", "PST_Phawillow_Desc", 0f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 45 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 25 });

			// Faker
			npc = new NPCBuilder<Faker>(plug.Info)
				.SetMinMaxAudioDistance(90f, 140f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Faker")
				.SetMetaName("PST_Faker_Name")
				.SetName("Faker")
				.AddLooker()
				.AddTrigger()
				.Build()
				.SetupNPCData("Faker", "PST_Faker_Name", "PST_Faker_Desc", -1.36f)
				.MarkAsReplacement(45, Character.LookAt); // 45

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Camera Stand
			npc = new NPCBuilder<CameraStand>(plug.Info)
				.SetMinMaxAudioDistance(90f, 140f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Camerastand")
				.SetMetaName("PST_CamSt_Name")
				.SetName("CameraStand")
				.AddLooker()
				.AddTrigger()
				.Build()
				.SetupNPCData("CameraStand", "PST_CamSt_Name", "PST_CamSt_Desc", -0.75f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 66 });

			// Mugh
			npc = new NPCBuilder<Mugh>(plug.Info)
				.SetMinMaxAudioDistance(165f, 175f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Mugh")
				.SetMetaName("PST_Mugh_Name")
				.SetName("Mugh")
				.AddTrigger()
				.Build()
				.SetupNPCData("Mugh", "PST_Mugh_Name", "PST_Mugh_Desc", -1.36f);

			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 50 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 35 });

			// Penny
			npc = new NPCBuilder<Penny>(plug.Info)
				.SetMinMaxAudioDistance(140f, 170f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Penny")
				.SetMetaName("PST_PEN_Name")
				.SetName("Penny")
				.AddLooker()
				.AddTrigger()
				.Build()
				.SetupNPCData("Penny", "PST_PEN_Name", "PST_PEN_Desc", -0.525f)
				.MarkAsReplacement(30, Character.DrReflex);

			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Pran the Dancer
			npc = new NPCBuilder<PranTheDancer>(plug.Info)
				.SetMinMaxAudioDistance(165f, 170f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Pran")
				.SetMetaName("PST_Pran_Name")
				.SetName("Pran")
				.AddTrigger()
				.Build()
				.SetupNPCData("Pran", "PST_Pran_Name", "PST_Pran_Desc", 0f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 45 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 15 });

			// Ser Oran
			npc = new NPCBuilder<SerOran>(plug.Info)
				.SetMinMaxAudioDistance(165f, 170f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("SerOran")
				.SetMetaName("PST_Oran_Name")
				.SetName("SerOran")
				.AddLooker()
				.SetMaxSightDistance(90)
				.AddTrigger()
				.Build()
				.SetupNPCData("SerOran", "PST_Oran_Name", "PST_Oran_Desc", -0.196f);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 35 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 10 });

			// Mopper
			npc = new NPCBuilder<Mopper>(plug.Info)
				.SetMinMaxAudioDistance(195f, 235f)
				.SetEnum("Mopper")
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMetaName("PST_MOP_Name")
				.SetName("Mopper")
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.Build()
				.SetupNPCData("Mopper", "PST_MOP_Name", "PST_MOP_Desc", -0.196f)
				.MarkAsReplacement(35, Character.Sweep);

			floorDatas[1].NPCs.Add(new() { selection = npc, weight = 1 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 1 });

			// Ink Artist
			npc = new NPCBuilder<InkArtist>(plug.Info)
				.SetMinMaxAudioDistance(155f, 175f)
				.SetEnum("InkArtist")
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMetaName("PST_InkArt_Name")
				.SetName("InkArtist")
				.AddTrigger()
				.Build()
				.SetupNPCData("InkArtist", "PST_InkArt_Name", "PST_InkArt_Desc", -0.196f);

			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 20 });

			// Tick Tock
			npc = new NPCBuilder<TickTock>(plug.Info)
				.SetMinMaxAudioDistance(155f, 175f)
				.SetEnum("TickTock")
				.AddSpawnableRoomCategories(RoomCategory.Faculty, RoomCategory.Office)
				.SetMetaName("PST_TickTock_Name")
				.SetName("TickTock")
				.AddTrigger()
				.Build()
				.SetupNPCData("TickTock", "PST_TickTock_Name", "PST_TickTock_Desc", -1.12f);

			floorDatas[0].NPCs.Add(new() { selection = npc, weight = 45 });
			floorDatas[3].NPCs.Add(new() { selection = npc, weight = 20 });

			// Quiker
			npc = new NPCBuilder<Quiker>(plug.Info)
				.SetMinMaxAudioDistance(155f, 165f)
				.SetEnum("Quiker")
				.SetMetaName("PST_Quiker_Name")
				.SetName("Quiker")
				.AddTrigger()
				.Build()
				.SetupNPCData("Quiker", "PST_Quiker_Name", "PST_Quiker_Desc", 0)
				.MarkAsReplacement(55, Character.LookAt);

			floorDatas[2].NPCs.Add(new() { selection = npc, weight = 1 });
		}

		

		
	}
}
