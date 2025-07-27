using BBTimes.CustomContent.NPCs;
using BBTimes.Helpers;
using BBTimes.Plugin;
using MTM101BaldAPI;
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
			const string
			STUDENT_TAG = "student",
			FACULTY_TAG = "faculty",
			NEITHER_TAG = "neither"
			;

			NPC npc;

			// Office Chair
			//npc = CreatorExtensions.CreateNPC<OfficeChair, OfficeChairCustomData>("OfficeChair", 35f, 60f, [RoomCategory.Office, RoomCategory.Faculty], [], "PST_OFC_Name", "PST_OFC_Desc", true, ignorePlayerOnSpawn: true, spriteYOffset:-2f).AddMeta(plug, "OfficeChair", NPCFlags.Standard).value;
			npc = new NPCBuilder<OfficeChair>(plug.Info)
				.SetMinMaxAudioDistance(20f, 80f)
				.AddSpawnableRoomCategories(RoomCategory.Office, RoomCategory.Faculty)
				.IgnorePlayerOnSpawn()
				.SetEnum("OfficeChair")
				.SetName("Office Chair")
				.SetMetaName("PST_OFC_Name")
				.AddTrigger()
				.SetMetaTags([NEITHER_TAG])
				.Build()
				.SetupNPCData("OfficeChair", "PST_OFC_Name", "PST_OFC_Desc", -0.75f);

			floorDatas[F1].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 60));

			// Happy Holidays
			npc = new NPCBuilder<HappyHolidays>(plug.Info)
				.SetMinMaxAudioDistance(25f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("HappyHolidays")
				.SetName("Happy Holidays")
				.SetMetaName("PST_HapH_Name")
				.AddTrigger()
				.AddLooker()
				.SetMetaTags([NEITHER_TAG, Storage.ChristmasSpecial_TimesTag])
				.SetMaxSightDistance(125)
				.Build()
				.SetupNPCData("HappyHolidays", "PST_HapH_Name", "PST_HapH_Desc", -2f);
			//CreatorExtensions.CreateNPC<HappyHolidays, HappyHolidaysCustomData>("HappyHolidays", 45f, 80f, [RoomCategory.Hall], [], "PST_HapH_Name", "PST_HapH_Desc", lookerDistance: 125, spriteYOffset: -2f).AddMeta(plug, NPCFlags.Standard).value;
			floorDatas[F1].NPCs.Add(new(npc, 10));

			// Classic Gotta Sweep
			npc = CreatorExtensions.CreateCustomNPCFromExistent<GottaSweep, ClassicGottaSweep>(Character.Sweep, "oldsweep", "ClassicGottaSweep").MarkAsReplacement(45, Character.Sweep);
			npc.AddMetaPrefab();
			floorDatas[F1].NPCs.Add(new(npc, 100));
			floorDatas[END].NPCs.Add(new(npc, 90));


			// Superintendent
			npc = new NPCBuilder<Superintendent>(plug.Info)
				.SetMinMaxAudioDistance(25f, 120f)
				.AddSpawnableRoomCategories(RoomCategory.Hall, RoomCategory.Class, RoomCategory.Faculty)
				.SetEnum("Superintendent")
				.SetName("Superintendent")
				.SetMetaName("PST_SI_Name")
				.AddLooker()
				.AddTrigger()
				.AddHeatmap()
				.SetMetaTags([FACULTY_TAG])
				.Build()
				.SetupNPCData("Superintendent", "PST_SI_Name", "PST_SI_Desc", -0.5f);
			//CreatorExtensions.CreateNPC<Superintendent, SuperintendentCustomData>("Superintendent", 110f, 140f, [RoomCategory.Office, RoomCategory.Class, RoomCategory.Faculty], [], "PST_SI_Name", "PST_SI_Desc", usesHeatMap:true, lookerDistance: 90f, avoidRooms:false, spriteYOffset: -0.5f).AddMeta(plug, NPCFlags.Standard).value;
			npc.Navigator.SetRoomAvoidance(false);
			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Stunly
			npc = new NPCBuilder<Stunly>(plug.Info)
				.SetMinMaxAudioDistance(20f, 90f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Stunly")
				.SetName("Stunly")
				.SetMetaName("PST_Stunly_Name")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(60)
				.SetMetaTags([STUDENT_TAG])
				.Build()
				.SetupNPCData("Stunly", "PST_Stunly_Name", "PST_Stunly_Desc", -1.5f);
			//CreatorExtensions.CreateNPC<Stunly, StunlyCustomData>("Stunly", 75f, 100f, [RoomCategory.Hall, RoomCategory.Special], [], "PST_Stunly_Name", "PST_Stunly_Desc", lookerDistance: 60f, spriteYOffset: -1.5f).AddMeta(plug , NPCFlags.Standard).value;
			floorDatas[F2].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Pix
			npc = new NPCBuilder<Pix>(plug.Info)
				.SetMinMaxAudioDistance(25f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Pix")
				.SetName("Pix")
				.SetMetaName("PST_Pix_Name")
				.AddTrigger()
				.AddLooker()
				.SetMetaTags([STUDENT_TAG])
				.SetMaxSightDistance(90)
				.SetFOV(100f)
				.Build()
				.SetupNPCData("Pix", "PST_Pix_Name", "PST_Pix_Desc", -1f);

			//CreatorExtensions.CreateNPC<Pix, PixCustomData>("Pix", 155f, 165f, [RoomCategory.Hall], [], "PST_Pix_Name", "PST_Pix_Desc", lookerDistance: 90f, spriteYOffset:-1f).AddMeta(plug, NPCFlags.Standard).value.SetNPCLookerFov(100f);

			floorDatas[F2].NPCs.Add(new(npc, 55));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Zero Prize

			npc = new NPCBuilder<ZeroPrize>(plug.Info)
				.SetMinMaxAudioDistance(25f, 120f)
				.SetEnum("ZeroPrize")
				.SetName("ZeroPrize")
				.SetMetaName("PST_0TH_Name")
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.SetMetaTags([FACULTY_TAG])
				.Build()
				.SetupNPCData("0thPrize", "PST_0TH_Name", "PST_0TH_Desc", -0.4f)
				.MarkAsReplacement(75, Character.Sweep); // 25

			floorDatas[F3].NPCs.Add(new(npc, 25));

			// Pencil Boy
			npc = new NPCBuilder<PencilBoy>(plug.Info)
				.SetMinMaxAudioDistance(30f, 90f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("PencilBoy")
				.SetName("Pencil Boy")
				.SetMetaName("PST_PB_Name")
				.AddLooker()
				.SetMetaTags([STUDENT_TAG])
				.SetMaxSightDistance(45f)
				.AddTrigger()
				.Build()
				.SetupNPCData("PencilBoy", "PST_PB_Name", "PST_PB_Desc", -1.77f);

			floorDatas[F2].NPCs.Add(new(npc, 55));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Rolling Bot
			npc = new NPCBuilder<RollingBot>(plug.Info)
				.SetMinMaxAudioDistance(30f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Rollingbot")
				.SetName("Rollingbot")
				.SetMetaName("PST_Rollbot_Name")
				.AddTrigger()
				.DisableAutoRotation()
				.SetMetaTags([NEITHER_TAG])
				.Build()
				.SetupNPCData("RollingBot", "PST_Rollbot_Name", "PST_Rollbot_Desc", -1.88f);

			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 65));

			// Watcher
			npc = new NPCBuilder<Watcher>(plug.Info)
				.SetMinMaxAudioDistance(30f, 120f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Watcher")
				.SetName("Watcher")
				.SetMetaName("PST_Wch_Name")
				.AddTrigger()
				.AddLooker()
				.SetMetaTags([NEITHER_TAG])
				.EnableAcceleration()
				.Build()
				.SetupNPCData("Watcher", "PST_Wch_Name", "PST_Wch_Desc", 0f);
			floorDatas[F3].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 15));

			// MGS
			npc = new NPCBuilder<MagicalStudent>(plug.Info)
				.SetMinMaxAudioDistance(40f, 200f)
				.AddSpawnableRoomCategories(RoomCategory.Office)
				.SetEnum("Magicalstudent")
				.SetName("Magicalstudent")
				.SetMetaName("PST_MGS_Name")
				.AddTrigger()
				.AddLooker()
				.AddHeatmap()
				.SetMetaTags([FACULTY_TAG])
				.Build()
				.SetupNPCData("MagicalStudent", "PST_MGS_Name", "PST_MGS_Desc", -1.91f)
				.MarkAsReplacement(35, Character.Principal); //35

			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Superintendent Jr.
			npc = new NPCBuilder<SuperIntendentJr>(plug.Info)
				.SetMinMaxAudioDistance(40f, 180f)
				.AddSpawnableRoomCategories(RoomCategory.Faculty, RoomCategory.Office)
				.SetEnum("Superintendentjr")
				.SetName("Superintendentjr")
				.SetMetaName("PST_Spj_Name")
				.AddTrigger()
				.AddLooker()
				.SetMetaTags([FACULTY_TAG])
				.SetMaxSightDistance(155)
				.Build()
				.SetupNPCData("SuperintendentJr", "PST_Spj_Name", "PST_Spj_Desc", -1f);

			npc.Navigator.SetRoomAvoidance(false);
			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[F2].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 65));

			// Leapy
			npc = new NPCBuilder<Leapy>(plug.Info)
				.SetMinMaxAudioDistance(25f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Leapy")
				.SetName("Leapy")
				.SetMetaTags([STUDENT_TAG])
				.SetMetaName("PST_Leapy_Name")
				.AddTrigger()
				.Build()
				.SetupNPCData("Leapy", "PST_Leapy_Name", "PST_Leapy_Desc", -1.1f);

			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Glue boy
			npc = new NPCBuilder<Glubotrony>(plug.Info)
				.SetMinMaxAudioDistance(25f, 90f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Glubotrony")
				.SetName("Glubotrony")
				.SetMetaName("PST_Gboy_Name")
				.AddLooker()
				.SetMetaTags([STUDENT_TAG])
				.SetMaxSightDistance(45)
				.SetFOV(110f)
				.AddTrigger()
				.DisableAutoRotation()
				.Build()
				.SetupNPCData("Glubotrony", "PST_Gboy_Name", "PST_Gboy_Desc", -0.7f);

			floorDatas[F2].NPCs.Add(new(npc, 55));
			floorDatas[END].NPCs.Add(new(npc, 28));

			// Dribble
			npc = new NPCBuilder<Dribble>(plug.Info)
				.SetMinMaxAudioDistance(30f, 110f)
				.SetEnum("Dribble")
				.SetName("Coach Dribble")
				.SetMetaName("PST_Dribble_Name")
				.AddSpawnableRoomCategories(RoomCategory.Special)
				.AddLooker()
				.SetMetaTags([FACULTY_TAG])
				.AddTrigger()
				.Build()
				.SetupNPCData("Dribble", "PST_Dribble_Name", "PST_Dribble_Desc", 0.3f)
				.MarkAsReplacement(45, Character.DrReflex);

			npc.Navigator.SetRoomAvoidance(false);

			floorDatas[F2].NPCs.Add(new(npc, 65));
			floorDatas[END].NPCs.Add(new(npc, 33));

			// Bubbly
			npc = new NPCBuilder<Bubbly>(plug.Info)
				.SetMinMaxAudioDistance(25f, 90f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Bubbly")
				.SetName("Bubbly")
				.SetMetaName("PST_Bubbly_Name")
				.SetMetaTags([STUDENT_TAG])
				.AddTrigger()
				.Build()
				.SetupNPCData("Bubbly", "PST_Bubbly_Name", "PST_Bubbly_Desc", -1.03f)
				.MarkAsReplacement(55, Character.Cumulo);

			floorDatas[F2].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Phawillow
			npc = new NPCBuilder<Phawillow>(plug.Info)
				.SetMinMaxAudioDistance(30f, 120f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Phawillow")
				.SetName("Phawillow")
				.SetMetaName("PST_Phawillow_Name")
				.AddLooker()
				.AddTrigger()
				.SetAirborne()
				.SetMetaTags([STUDENT_TAG])
				.Build()
				.SetupNPCData("Phawillow", "PST_Phawillow_Name", "PST_Phawillow_Desc", 0f);

			floorDatas[F2].NPCs.Add(new(npc, 45));
			floorDatas[END].NPCs.Add(new(npc, 25));

			// Faker
			npc = new NPCBuilder<Faker>(plug.Info)
				.SetMinMaxAudioDistance(30f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Faker")
				.SetMetaName("PST_Faker_Name")
				.SetName("Faker")
				.AddLooker()
				.AddTrigger()
				.SetMetaTags([NEITHER_TAG])
				.Build()
				.SetupNPCData("Faker", "PST_Faker_Name", "PST_Faker_Desc", -1.36f)
				.MarkAsReplacement(45, Character.LookAt); // 45

			floorDatas[F3].NPCs.Add(new(npc, 23));
			floorDatas[END].NPCs.Add(new(npc, 44));

			// Camera Stand
			npc = new NPCBuilder<CameraStand>(plug.Info)
				.SetMinMaxAudioDistance(30f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Camerastand")
				.SetMetaName("PST_CamSt_Name")
				.SetName("CameraStand")
				.AddLooker()
				.SetMetaTags([NEITHER_TAG])
				.AddTrigger()
				.Build()
				.SetupNPCData("CameraStand", "PST_CamSt_Name", "PST_CamSt_Desc", -0.75f);

			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 66));

			// Mugh
			npc = new NPCBuilder<Mugh>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMetaTags([STUDENT_TAG])
				.SetEnum("Mugh")
				.SetMetaName("PST_Mugh_Name")
				.SetName("Mugh")
				.AddTrigger()
				.Build()
				.SetupNPCData("Mugh", "PST_Mugh_Name", "PST_Mugh_Desc", -1.36f);

			floorDatas[F1].NPCs.Add(new(npc, 50));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Penny
			npc = new NPCBuilder<Penny>(plug.Info)
				.SetMinMaxAudioDistance(30f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Penny")
				.SetMetaName("PST_PEN_Name")
				.SetName("Penny")
				.SetMetaTags([FACULTY_TAG])
				.AddLooker()
				.AddTrigger()
				.Build()
				.SetupNPCData("Penny", "PST_PEN_Name", "PST_PEN_Desc", -1.35f)
				.MarkAsReplacement(30, Character.DrReflex);

			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[F2].NPCs.Add(new(npc, 21));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Ser Oran
			npc = new NPCBuilder<SerOran>(plug.Info)
				.SetMinMaxAudioDistance(30f, 110f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("SerOran")
				.SetMetaName("PST_Oran_Name")
				.SetName("SerOran")
				.SetMetaTags([STUDENT_TAG])
				.AddLooker()
				.SetMaxSightDistance(90)
				.AddTrigger()
				.Build()
				.SetupNPCData("SerOran", "PST_Oran_Name", "PST_Oran_Desc", -0.196f);

			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 10));

			// CoolMop
			npc = new NPCBuilder<CoolMop>(plug.Info)
				.SetMinMaxAudioDistance(35f, 120f)
				.SetEnum("CoolMop")
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMetaName("PST_MOP_Name")
				.SetName("CoolMop")
				.SetMetaTags([FACULTY_TAG])
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.Build()
				.SetupNPCData("CoolMop", "PST_MOP_Name", "PST_MOP_Desc", -0.67f)
				.MarkAsReplacement(35, Character.Sweep);

			floorDatas[F2].NPCs.Add(new(npc, 55));
			floorDatas[END].NPCs.Add(new(npc, 55));

			// Ink Artist
			npc = new NPCBuilder<InkArtist>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("InkArtist")
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMetaName("PST_InkArt_Name")
				.SetMetaTags([STUDENT_TAG])
				.SetName("InkArtist")
				.AddTrigger()
				.Build()
				.SetupNPCData("InkArtist", "PST_InkArt_Name", "PST_InkArt_Desc", -0.196f);

			floorDatas[F1].NPCs.Add(new(npc, 20));

			// Tick Tock
			npc = new NPCBuilder<TickTock>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("TickTock")
				.AddSpawnableRoomCategories(RoomCategory.Faculty, RoomCategory.Office)
				.SetMetaTags([NEITHER_TAG])
				.SetMetaName("PST_TickTock_Name")
				.SetName("TickTock")
				.AddTrigger()
				.Build()
				.SetupNPCData("TickTock", "PST_TickTock_Name", "PST_TickTock_Desc", -1.12f);

			floorDatas[F1].NPCs.Add(new(npc, 45));
			floorDatas[END].NPCs.Add(new(npc, 20));

			// Quiker
			npc = new NPCBuilder<Quiker>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("Quiker")
				.SetMetaName("PST_Quiker_Name")
				.SetMetaTags([NEITHER_TAG])
				.SetName("Quiker")
				.SetAirborne()
				.AddTrigger()
				.Build()
				.SetupNPCData("Quiker", "PST_Quiker_Name", "PST_Quiker_Desc", 0)
				.MarkAsReplacement(55, Character.LookAt);

			floorDatas[F4].NPCs.Add(new(npc, 20, LevelType.Maintenance));

			// Jerry The Air Conditioner
			npc = new NPCBuilder<JerryTheAC>(plug.Info)
				.SetMinMaxAudioDistance(35f, 120f)
				.SetEnum("JerryTheAC")
				.SetMetaName("PST_JerryAc_Name")
				.SetName("JerryTheAirConditioner")
				.SetMetaTags([FACULTY_TAG])
				.AddTrigger()
				.Build()
				.SetupNPCData("JerryTheAirConditioner", "PST_JerryAc_Name", "PST_JerryAc_Desc", 0)
				.MarkAsReplacement(35, Character.Cumulo);

			floorDatas[F1].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Zap Zap
			npc = new NPCBuilder<ZapZap>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Office, RoomCategory.Faculty)
				.SetEnum("ZapZap")
				.SetMetaTags([NEITHER_TAG])
				.SetMetaName("PST_ZapZap_Name")
				.SetName("ZapZap")
				.AddTrigger()
				.Build()
				.SetupNPCData("ZapZap", "PST_ZapZap_Name", "PST_ZapZap_Desc", -1.0018f)
				.MarkAsReplacement(40, EnumExtensions.GetFromExtendedName<Character>("Rollingbot"));

			npc.Navigator.SetRoomAvoidance(false);

			floorDatas[F2].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Cheese McSwiss
			npc = new NPCBuilder<CheeseMan>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("CheeseMan")
				.SetMetaTags([STUDENT_TAG])
				.SetMetaName("PST_CheeseMan_Name")
				.SetName("CheeseMan")
				.AddTrigger()
				.Build()
				.SetupNPCData("CheeseMan", "PST_CheeseMan_Name", "PST_CheeseMan_Desc", -1.86f);

			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 50));

			// Detention Bot
			npc = new NPCBuilder<DetentionBot>(plug.Info)
				.SetMinMaxAudioDistance(35f, 120f)
				.AddSpawnableRoomCategories(RoomCategory.Office)
				.AddHeatmap()
				.AddLooker()
				.SetEnum("DetentionBot")
				.SetMetaName("PST_DetentionBot_Name")
				.SetMetaTags([FACULTY_TAG])
				.SetName("DetentionBot")
				.AddTrigger()
				.Build()
				.SetupNPCData("DetentionBot", "PST_DetentionBot_Name", "PST_DetentionBot_Desc", -0.715f)
				.MarkAsReplacement(15, Character.Principal);

			npc.Navigator.SetRoomAvoidance(false);
			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[F2].NPCs.Add(new(npc, 38));
			floorDatas[END].NPCs.Add(new(npc, 41));

			// Science Teacher
			npc = new NPCBuilder<ScienceTeacher>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("ScienceTeacher")
				.SetMetaName("PST_SciTeacher_Name")
				.SetMetaTags([FACULTY_TAG])
				.SetName("ScienceTeacher")
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(81.4f)
				.Build()
				.SetupNPCData("ScienceTeacher", "PST_SciTeacher_Name", "PST_SciTeacher_Desc", -0.1f);

			npc.Navigator.SetRoomAvoidance(false);
			floorDatas[F2].NPCs.Add(new(npc, 24));
			floorDatas[END].NPCs.Add(new(npc, 37));

			// Adverto
			npc = new NPCBuilder<Adverto>(plug.Info)
				.SetEnum("Adverto")
				.SetMetaName("PST_Adverto_Name")
				.SetName("Adverto")
				.SetMetaTags([FACULTY_TAG])
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(110f)
				.Build()
				.SetupNPCData("Adverto", "PST_Adverto_Name", "PST_Adverto_Desc", -0.1f);

			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[F2].NPCs.Add(new(npc, 15));
			floorDatas[END].NPCs.Add(new(npc, 25));

			// Vacuum Cleaner
			npc = new NPCBuilder<VacuumCleaner>(plug.Info)
				.SetEnum("VacuumCleaner")
				.SetMetaName("PST_VacClean_Name")
				.SetName("VacuumCleaner")
				.SetMetaTags([FACULTY_TAG])
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(55f)
				.Build()
				.SetupNPCData("VacuumCleaner", "PST_VacClean_Name", "PST_VacClean_Desc", -1.1691f)
				.MarkAsReplacement(15, Character.Sweep);

			npc.looker.layerMask = LayerStorage.principalLookerMask;
			floorDatas[F2].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Mopliss
			npc = new NPCBuilder<Mopliss>(plug.Info)
				.SetMinMaxAudioDistance(35f, 120f)
				.SetEnum("Mopliss")
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetMetaName("PST_MOPLISS_Name")
				.SetName("Mopliss")
				.SetMetaTags([FACULTY_TAG])
				.IgnorePlayerOnSpawn()
				.AddTrigger()
				.Build()
				.SetupNPCData("Mopliss", "PST_MOPLISS_Name", "PST_MOPLISS_Desc", -0.75f)
				.MarkAsReplacement(20, Character.Sweep);

			floorDatas[F2].NPCs.Add(new(npc, 65));
			floorDatas[END].NPCs.Add(new(npc, 45));

			// Nose
			npc = new NPCBuilder<NoseMan>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("NoseMan")
				.SetMetaName("PST_NOSE_Name")
				.SetName("NoseMan")
				.SetMetaTags([STUDENT_TAG])
				.AddTrigger()
				.AddLooker()
				.SetMaxSightDistance(135f)
				.Build()
				.SetupNPCData("NoseMan", "PST_NOSE_Name", "PST_NOSE_Desc", -1.45f);

			floorDatas[F2].NPCs.Add(new(npc, 20));

			// Mimicry
			npc = new NPCBuilder<Mimicry>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("Mimicry")
				.SetMetaName("PST_Mimi_Name")
				.SetName("Mimicry")
				.SetMetaTags([STUDENT_TAG])
				.AddTrigger()
				.SetForcedSubtitleColor(new(0.546875f, 0.1015625f, 0.99609375f))
				.AddLooker()
				.SetMaxSightDistance(20f)
				.Build()
				.SetupNPCData("Mimicry", "PST_Mimi_Name", "PST_Mimi_Desc", -3.3f);

			npc.looker.layerMask = LayerStorage.principalLookerMask;

			floorDatas[F2].NPCs.Add(new(npc, 35));
			floorDatas[END].NPCs.Add(new(npc, 20));

			// Pran the Dancer
			npc = new NPCBuilder<PranTheDancer>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.AddSpawnableRoomCategories(RoomCategory.Hall)
				.SetEnum("Pran")
				.SetMetaTags([NEITHER_TAG])
				.SetMetaName("PST_Pran_Name")
				.SetName("Pran")
				.AddTrigger()
				.Build()
				.SetupNPCData("Pran", "PST_Pran_Name", "PST_Pran_Desc", 0f);
			floorDatas[F2].NPCs.Add(new(npc, 45));
			floorDatas[END].NPCs.Add(new(npc, 15));

			// Winterry
			npc = new NPCBuilder<Winterry>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("Winterry")
				.SetMetaName("PST_Winterry_Name")
				.SetName("Winterry")
				.SetMetaTags([STUDENT_TAG, Storage.ChristmasSpecial_TimesTag])
				.AddTrigger()
				.SetForcedSubtitleColor(new(0.3984375f, 0.59765625f, 0.99609375f))
				.AddLooker()
				.SetMaxSightDistance(55f)
				.Build()
				.SetupNPCData("Winterry", "PST_Winterry_Name", "PST_Winterry_Desc", -1.4f)
				.MarkAsReplacement(65, Character.Beans);

			npc.looker.layerMask = LayerStorage.principalLookerMask;

			floorDatas[F2].NPCs.Add(new(npc, 40));
			floorDatas[END].NPCs.Add(new(npc, 20));

			// Snowfolke
			npc = new NPCBuilder<Snowfolke>(plug.Info)
				.SetMinMaxAudioDistance(30f, 100f)
				.SetEnum("Snowfolke")
				.SetMetaName("PST_Snowfolke_Name")
				.SetAirborne()
				.SetName("Snowfolke")
				.SetMetaTags([STUDENT_TAG, Storage.ChristmasSpecial_TimesTag])
				.AddTrigger()
				.SetForcedSubtitleColor(new(0.69921875f, 0.796875f, 0.99609375f))
				.Build()
				.SetupNPCData("Snowfolke", "PST_Snowfolke_Name", "PST_Snowfolke_Desc", 0f);

			floorDatas[F2].NPCs.Add(new(npc, 55));
			floorDatas[END].NPCs.Add(new(npc, 32));

			// Everett Treewood
			npc = new NPCBuilder<EverettTreewood>(plug.Info)
				.SetMinMaxAudioDistance(45f, 135f)
				.SetEnum("EverettTreewood")
				.SetMetaName("PST_EverettTree_Name")
				.SetName("EverettTreewood")
				.SetMetaTags([NEITHER_TAG, Storage.ChristmasSpecial_TimesTag])
				.AddTrigger()
				.SetForcedSubtitleColor(new(0f, 0.5f, 0.16796875f))
				.AddLooker()
				.SetMaxSightDistance(100f)
				.Build()
				.SetupNPCData("EverettTreewood", "PST_EverettTree_Name", "PST_EverettTree_Desc", -0.5f);

			npc.looker.layerMask = LayerStorage.principalLookerMask;

			floorDatas[F2].NPCs.Add(new(npc, 10));
			floorDatas[END].NPCs.Add(new(npc, 35));

			// Mr. Kreye
			npc = new NPCBuilder<MrKreye>(plug.Info)
				.SetMinMaxAudioDistance(65f, 200f)
				.SetEnum("MrKreye")
				.SetMetaName("PST_Kreye_Name")
				.SetName("MrKreye")
				.SetMetaTags([FACULTY_TAG])
				.AddTrigger()
				.SetForcedSubtitleColor(new(0.44140625f, 0.078125f, 0.0234375f))
				.AddLooker()
				.Build()
				.SetupNPCData("MrKreye", "PST_Kreye_Name", "PST_Kreye_Desc", -0.8f)
				.MarkAsReplacement(25, Character.Principal);

			npc.looker.layerMask = LayerStorage.principalLookerMask;

			floorDatas[F2].NPCs.Add(new(npc, 25));
			floorDatas[END].NPCs.Add(new(npc, 40));
		}
	}
}
