using BBTimes.CustomComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.Misc;
using BBTimes.CustomContent.NPCs;
using BBTimes.CustomContent.Objects;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using BepInEx;
using EditorCustomRooms;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using PlusLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateCustomRooms(BaseUnityPlugin plug)
		{
			var lightPre = new WeightedTransform() { selection = GenericExtensions.FindResourceObjects<RoomAsset>().First(x => x.category == RoomCategory.Class).lightPre };
			var carpet = new WeightedTexture2D() { selection = GenericExtensions.FindResourceObjectByName<Texture2D>("Carpet") };
			var ceiling = new WeightedTexture2D() { selection = GenericExtensions.FindResourceObjectByName<Texture2D>("CeilingNoLight") };
			var saloonWall = new WeightedTexture2D() { selection = GenericExtensions.FindResourceObjectByName<Texture2D>("SaloonWall") };
			var normWall = new WeightedTexture2D() { selection = GenericExtensions.FindResourceObjectByName<Texture2D>("Wall") };
			var blackTexture = TextureExtensions.CreateSolidTexture(1, 1, Color.black); // It'll be stretched anyways lol
			var grass = man.Get<Texture2D>("Tex_Grass");

			//***************************************************
			//***************************************************
			//*************Bathroom Creation*******************
			//***************************************************
			//***************************************************

			var bathStall = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "bathToiletWalls.png")), false);
			bathStall.gameObject.AddNavObstacle(new(1f, 10f, 1f));
			bathStall.name = "bathStall";

			bathStall.transform.localScale = new(9.9f, 10f, 1f);
			bathStall.AddObjectToEditor();

			bathStall = bathStall.DuplicatePrefab();
			bathStall.GetComponent<MeshRenderer>().material.mainTexture = AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "BathDoor.png"));
			bathStall.name = "bathDoor";
			bathStall.AddObjectToEditor();

			var bathSink = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "sink.png")), 50f))
				.AddSpriteHolder(2f, LayerStorage.ignoreRaycast);
			bathSink.name = "sink";
			bathSink.transform.parent.name = "sink";
			bathSink.transform.parent.gameObject.AddNavObstacle(new(2.5f, 10f, 2.5f));
			bathSink.transform.parent.gameObject.AddBoxCollider(Vector3.zero, new(2.5f, 10f, 2.5f), false);
			bathSink.transform.parent.gameObject.AddObjectToEditor();

			var bathLightPre = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "long_hanginglamp.png")), 50f))
				.AddSpriteHolder(8.98f).transform.parent;
			bathLightPre.name = "hangingLongLight";
			bathLightPre.gameObject.ConvertToPrefab(true);

			var sets = RegisterRoom("Bathroom", Color.white,
				ObjectCreators.CreateDoorDataObject("BathDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "bathDoorOpened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "bathDoorClosed.png"))));

			Superintendent.AddAllowedRoom(sets.category);


			var room = GetAllAssets(GetRoomAsset("Bathroom"), 45, 25, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "MapBG_Bathroom.png")));
			var fun = room[0].selection.AddRoomFunctionToContainer<PosterAsideFromObject>();
			fun.targetPrefabName = "sink";
			fun.posterPre = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "mirror.png"))]);

			room.ForEach(x => x.selection.posterChance = 0f);
			sets.container = room[0].selection.roomFunctionContainer;

			var group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 0,
				maxRooms = 2,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(0)],
				name = "Bathroom",
				light = [new() { selection = bathLightPre }],
			};

			floorDatas[0].RoomAssets.Add(group);

			group = new RoomGroup()
			{
				stickToHallChance = 0.7f,
				minRooms = 1,
				maxRooms = 2,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(1)],
				name = "Bathroom",
				light = [new() { selection = bathLightPre }]
			};

			floorDatas[1].RoomAssets.Add(group);

			group = new RoomGroup()
			{
				stickToHallChance = 0.7f,
				minRooms = 1,
				maxRooms = 2,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(3)],
				name = "Bathroom",
				light = [new() { selection = bathLightPre }]
			};

			floorDatas[3].RoomAssets.Add(group);
			group = new RoomGroup()
			{
				stickToHallChance = 0.45f,
				minRooms = 3,
				maxRooms = 5,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(2)],
				name = "Bathroom",
				light = [new() { selection = bathLightPre }]
			};
			floorDatas[2].RoomAssets.Add(group);

			// *******************************************************
			// *******************************************************
			// ******************* Abandoned Rooms *******************
			// *******************************************************
			// *******************************************************			

			sets = RegisterRoom("AbandonedRoom", new(0.59765625f, 0.19921875f, 0f),
				ObjectCreators.CreateDoorDataObject("OldDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "oldDoorOpen.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "oldDoorClosed.png"))));

			room = GetAllAssets(GetRoomAsset("AbandonedRoom"), 250, 45, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "MapBG_AbandonedRoom.png")));
			room[0].selection.AddRoomFunctionToContainer<ShowItemsInTheEnd>();
			room.ForEach(x => x.selection.posterChance = 0f);
			sets.container = room[0].selection.roomFunctionContainer;

			group = new RoomGroup()
			{
				stickToHallChance = 0.45f,
				minRooms = 1,
				maxRooms = 1,
				potentialRooms = [.. room],
				name = "AbandonedRoom",
				light = [lightPre],
				ceilingTexture = [ceiling],
				floorTexture = [carpet]
			};
			floorDatas[2].RoomAssets.Add(group);

			//***************************************************
			//***************************************************
			//*************Computer Room Creation ***************
			//***************************************************
			//***************************************************

			//Table
			var table = new GameObject("FancyComputerTable")
			{
				layer = LayerStorage.ignoreRaycast
			};

			table.AddBoxCollider(Vector3.zero, Vector3.one * 10f, false);
			table.AddNavObstacle(Vector3.one * 10f);

			var tableHead = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "computerTableTex.png")), false);
			Object.Destroy(tableHead.GetComponent<Collider>());
			tableHead.transform.SetParent(table.transform);
			tableHead.transform.localPosition = Vector3.up * 3f;
			tableHead.transform.localScale = new(9.9f, 1f, 9.9f);

			TableLegCreator(new(-4f, 1.25f, 4f));
			TableLegCreator(new(4f, 1.25f, -4f));
			TableLegCreator(new(4f, 1.25f, 4f));
			TableLegCreator(new(-4f, 1.25f, -4f));

			void TableLegCreator(Vector3 pos)
			{
				var machineWheel = ObjectCreationExtension.CreateCube(blackTexture, false);
				machineWheel.transform.SetParent(table.transform);
				machineWheel.transform.localPosition = pos;
				machineWheel.transform.localScale = new(1f, 2.5f, 1f);
				Object.Destroy(machineWheel.GetComponent<Collider>());
			}

			table.AddObjectToEditor();

			// Computer

			var computer = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "computer.png")), 25f));
			computer.name = "ComputerBillboard";
			computer.gameObject.AddObjectToEditor();

			// Event machine
			var machine = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "fogMachineFront_Off.png")), 26f), false);
			machine.gameObject.layer = 0;
			machine.name = "EventMachine";
			machine.gameObject.ConvertToPrefab(true);
			var evMac = machine.gameObject.AddComponent<EventMachine>();
			evMac.spriteToChange = machine;
			evMac.sprNoEvents = machine.sprite;
			evMac.sprWorking = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "fogMachineFront_ON.png")), 26f);
			evMac.sprDead = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "fogMachineFront_Ded.png")), 26f);
			machine.gameObject.AddBoxCollider(Vector3.zero, new(6f, 10f, 1f), true);

			sets = RegisterRoom("ComputerRoom", new(0f, 0f, 0.35f),
				ObjectCreators.CreateDoorDataObject("ComputerDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "computerDoorOpened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "computerDoorClosed.png"))));


			room = GetAllAssets(GetRoomAsset("ComputerRoom"), 75, 50, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "MapBG_Computer.png")));
			room[0].selection.AddRoomFunctionToContainer<RandomPosterFunction>().posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "ComputerPoster.png"))])];
			room[0].selection.AddRoomFunctionToContainer<EventMachineSpawner>().machinePre = evMac;

			sets.container = room[0].selection.roomFunctionContainer;

			group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 1,
				maxRooms = 1,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(0)],
				light = [lightPre],
				name = "ComputerRoom"
			};

			floorDatas[0].RoomAssets.Add(group);

			group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 1,
				maxRooms = 2,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(1)],
				name = "ComputerRoom",
				light = [lightPre],
			};

			floorDatas[1].RoomAssets.Add(group);
			group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 1,
				maxRooms = 2,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(3)],
				name = "ComputerRoom",
				light = [lightPre]
			};

			floorDatas[3].RoomAssets.Add(group);

			group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 1,
				maxRooms = 3,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(2)],
				name = "ComputerRoom",
				light = [lightPre]
			};

			floorDatas[2].RoomAssets.Add(group);

			//***************************************************
			//***************************************************
			//************* Dribble's Room **********************
			//***************************************************
			//***************************************************

			var runLine = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("DribbleRoom", "lineStraight.png")), 12.5f), false).AddSpriteHolder(0.1f, 0);
			runLine.gameObject.layer = 0; // default layer
			runLine.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			runLine.transform.parent.name = "StraightRunLine";
			runLine.transform.parent.gameObject.AddObjectToEditor();

			runLine = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("DribbleRoom", "lineCurve.png")), 12.5f), false).AddSpriteHolder(0.1f, 0);
			runLine.gameObject.layer = 0; // default layer
			runLine.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			runLine.transform.parent.name = "CurvedRunLine";
			runLine.transform.parent.gameObject.AddObjectToEditor();

			sets = RegisterRoom("DribbleRoom", new(1f, 0.439f, 0f),
				ObjectCreators.CreateDoorDataObject("DribbleRoomDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("DribbleRoom", "dribbleDoorOpen.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("DribbleRoom", "dribbleDoorClosed.png"))));

			Superintendent.AddAllowedRoom(sets.category);

			room = GetAllAssets(GetRoomAsset("DribbleRoom"), 75, 50);
			room[0].selection.AddRoomFunctionToContainer<RuleFreeZone>();
			room[0].selection.AddRoomFunctionToContainer<PlayerRunCornerFunction>();

			sets.container = room[0].selection.roomFunctionContainer;

			room.ForEach(x =>
			{
				x.selection.wallTex = saloonWall.selection;
				x.selection.ceilTex = ceiling.selection;
			});

			AddAssetsToNpc<Dribble>(room);

			// ************************************************************
			// ************************************************************
			// ******************* Storage Room (Sweep) *******************
			// ************************************************************
			// ************************************************************

			var sweepCloset = GenericExtensions.FindResourceObject<GottaSweep>().potentialRoomAssets[0].selection;

			ClearNpcAssets<GottaSweep>();

			// Shelf creation
			var darkWood = Object.Instantiate(man.Get<Texture2D>("woodTexture"));
			darkWood.name = "Times_darkWood";
			var shelf = new GameObject("ClosetShelf");
			shelf.gameObject.AddBoxCollider(Vector3.zero, new(4f, 10f, 15f), false);
			shelf.gameObject.AddNavObstacle(new(4.2f, 10f, 16.3f));
			shelf.layer = LayerStorage.ignoreRaycast;

			var shelfBody = ObjectCreationExtension.CreateCube(darkWood.ApplyLightLevel(-25f), false);
			shelfBody.transform.SetParent(shelf.transform);
			shelfBody.transform.localPosition = Vector3.up * 4f;
			shelfBody.transform.localScale = new(4f, 0.7f, 15f);
			Object.Destroy(shelfBody.GetComponent<Collider>());
			

			ShelfLegCreator(new(-1.5f, 2.3f, 6.5f));
			ShelfLegCreator(new(1.5f, 2.3f, -6.5f));
			ShelfLegCreator(new(-1.5f, 2.3f, -6.5f));
			ShelfLegCreator(new(1.5f, 2.3f, 6.5f));

			void ShelfLegCreator(Vector3 pos)
			{
				var shelfLeg = ObjectCreationExtension.CreatePrimitiveObject(PrimitiveType.Cylinder, blackTexture);
				shelfLeg.transform.SetParent(shelf.transform);
				shelfLeg.transform.localPosition = pos;
				shelfLeg.transform.localScale = new(0.8f, 2.3f, 0.8f);
				Object.Destroy(shelfLeg.GetComponent<Collider>());
			}

			shelf.gameObject.AddObjectToEditor();

			room = GetAllAssets(GetRoomAsset("Closet"), sweepCloset.maxItemValue, 100, sweepCloset.roomFunctionContainer);
			room[0].selection.AddRoomFunctionToContainer<HighCeilingRoomFunction>().ceilingHeight = 1;

			room.ForEach(x => {
				x.selection.posters = sweepCloset.posters;
				x.selection.posterChance = sweepCloset.posterChance;
				x.selection.windowChance = sweepCloset.windowChance;
				x.selection.windowObject =sweepCloset.windowObject;
				x.selection.ceilTex = sweepCloset.ceilTex;
				x.selection.wallTex = sweepCloset.wallTex;
				x.selection.florTex = sweepCloset.florTex;
				x.selection.lightPre = sweepCloset.lightPre;
			});

			AddAssetsToNpc<GottaSweep>(room);

			ClearNpcAssets<ZeroPrize>();
			AddAssetsToNpc<ZeroPrize>(room);

			// ***********************************************
			// ***********************************************
			// ******************* Kitchen *******************
			// ***********************************************
			// ***********************************************

			// Kitchen "table"
			shelf = new GameObject("KitchenCabinet");
			shelf.gameObject.AddBoxCollider(Vector3.zero, new(10f, 10f, 10f), false);
			//shelf.gameObject.AddNavObstacle(new(10f, 10f, 10f)); Not required as the player can't go through anyways. This is for guarantee that wandering npcs just don't go through walls after going past these structures
			shelf.layer = LayerStorage.ignoreRaycast;

			shelfBody = ObjectCreationExtension.CreateCube(Object.Instantiate(man.Get<Texture2D>("plasticTexture")).ApplyLightLevel(-45f), false);
			shelfBody.transform.SetParent(shelf.transform);
			shelfBody.transform.localPosition = Vector3.up * 2.5f;
			shelfBody.transform.localScale = new(9.9f, 1f, 9.9f);
			Object.Destroy(shelfBody.GetComponent<Collider>());

			

			shelfBody = ObjectCreationExtension.CreateCube(man.Get<Texture2D>("plasticTexture"), false);
			shelfBody.transform.SetParent(shelf.transform);
			shelfBody.transform.localPosition = Vector3.up * 0.7f;
			shelfBody.transform.localScale = new(7.5f, 4f, 7.5f);
			Object.Destroy(shelfBody.GetComponent<Collider>());

			shelf.gameObject.AddObjectToEditor();

			// Joe

			var joe = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "JoeChef.png")), 29f)).AddSpriteHolder(0f, LayerStorage.iClickableLayer).transform.parent;
			joe.name = "JoeChef";
			joe.gameObject.AddBoxCollider(Vector3.zero, Vector3.one * 10f, true);
			joe.gameObject.AddObjectToEditor();

			var joeChef = joe.gameObject.AddComponent<JoeChef>();
			joeChef.audMan = joe.gameObject.CreatePropagatedAudioManager(175f, 356f);
			joeChef.kitchenAudMan = joe.gameObject.CreatePropagatedAudioManager(76f, 155f);
			joeChef.audWelcome = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("Kitchen", "Joe_Welcome.wav")), "Vfx_Joe_Welcome", SoundType.Voice, Color.white);
			joeChef.audScream = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("Kitchen", "Joe_Scream.wav")), "Vfx_Joe_Scream", SoundType.Voice, Color.white);
			joeChef.audKitchen = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("Kitchen", "KitchenThing.wav")), string.Empty, SoundType.Voice, Color.white);
			joeChef.audKitchen.subtitle = false;

			var joeHolderRenderer = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(new Vector3(-3.3f, 2f, 0f), 0);
			var joeHolder = joeHolderRenderer.transform.parent;
			joeHolder.transform.SetParent(joe.transform);
			joeHolder.transform.localPosition = Vector3.zero;
			joeHolder.gameObject.AddComponent<BillboardRotator>();
			joeChef.itemRenderer = joeHolderRenderer;

			JoeChef.AddFood(ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value, 15);
			JoeChef.AddFood(ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value, 45);

			sets = RegisterRoom("Kitchen", Color.white,
				ObjectCreators.CreateDoorDataObject("KitchenDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "kitchenDoorOpened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "kitchenDoorClosed.png"))));


			room = GetAllAssets(GetRoomAsset("Kitchen"), 75, 35, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "MapBG_Kitchen.png")));

			Object.Destroy(room[0].selection.roomFunctionContainer.gameObject); // It doesn't need one, it's empty

			group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 0,
				maxRooms = 1,
				potentialRooms = [.. room],
				name = "Kitchen",
				light = [new() { selection = EmptyGameObject.transform }],
				ceilingTexture = [ceiling],
				wallTexture = [normWall]
			};

			for (int i = 1; i < floorDatas.Count; i++)
				floorDatas[i].RoomAssets.Add(group);


			// ================================================ Special Room Creation ====================================================

			// *******************************************************
			// *******************************************************
			// ******************* Basketball Area *******************
			// *******************************************************
			// *******************************************************	

			// Hoop

			var hoop = Object.Instantiate(GenericExtensions.FindResourceObjectByName<RendererContainer>("HoopBase"));
			hoop.transform.localScale = Vector3.one * 3f;
			hoop.GetComponent<CapsuleCollider>().radius = 1;
			hoop.GetComponent<NavMeshObstacle>().radius = 1;
			hoop.name = "BasketHoop";
			hoop.gameObject.AddObjectToEditor();

			// Grand Stand

			var grandStand = ObjectCreationExtension.CreateCube(TextureExtensions.CreateSolidTexture(1, 1, Color.gray), false);
			grandStand.name = "GrandStand";
			grandStand.AddObjectToEditor();
			grandStand.transform.localScale = new(8f, 8f, 45f);
			grandStand.gameObject.AddNavObstacle(new(1f, 10f, 1f));

			// Basket Machine

			var basketMachine = new GameObject("BasketMachine");
			basketMachine.AddNavObstacle(new(4.5f, 10f, 4.5f));
			basketMachine.AddBoxCollider(Vector3.zero, new(4f, 10f, 4f), false);

			var machineBody = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "machineTexture.png")), false);
			machineBody.transform.SetParent(basketMachine.transform);
			machineBody.transform.localScale = new(4f, 5f, 4f);
			machineBody.transform.localPosition = Vector3.up * 3.2f;
			Object.Destroy(machineBody.GetComponent<Collider>());

			void WheelCreator(Vector3 pos)
			{
				var machineWheel = ObjectCreationExtension.CreatePrimitiveObject(PrimitiveType.Cylinder, blackTexture);
				machineWheel.transform.SetParent(basketMachine.transform);
				machineWheel.transform.localPosition = pos;
				machineWheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				machineWheel.transform.localScale = new(1f, 0.4f, 1f);
				Object.Destroy(machineWheel.GetComponent<Collider>());
			}

			WheelCreator(new(-2f, 0.5f, 0f));
			WheelCreator(new(2f, 0.5f, 0f));

			// Cannon
			var machineCannon = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "cannonTexture.png")));
			machineCannon.transform.SetParent(basketMachine.transform);
			machineCannon.transform.localScale = new(1f, 1f, 3f);
			machineCannon.transform.localPosition = new(-0.5f, 4.13f, 0.55f);
			Object.Destroy(machineCannon.GetComponent<Collider>());

			basketMachine.AddObjectToEditor();

			var shooter = basketMachine.AddComponent<BasketBallCannon>();
			shooter.basketPre = (ITM_Basketball)ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("Basketball"), plug.Info).value.item;
			shooter.audMan = basketMachine.CreatePropagatedAudioManager(65, 200);
			shooter.audBoom = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("BasketballArea", "shootBoom.wav")), string.Empty, SoundType.Voice, Color.white);
			shooter.audBoom.subtitle = false;
			shooter.audTurn = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("BasketballArea", "turn.wav")), string.Empty, SoundType.Voice, Color.white);
			shooter.audTurn.subtitle = false;
			shooter.cannon = machineCannon.transform;

			// Basketball Pile

			var basketballPile = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "basketLotsOfBalls.png")), 65f));
			basketballPile.name = "BasketballPile";
			basketballPile.gameObject.AddObjectToEditor();

			// BaldiBall (Easter Egg)
			var baldiBall = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "BaldiBall.png")), 17f));
			baldiBall.name = "BaldiBall";
			baldiBall.gameObject.ConvertToPrefab(true);

			// Huge line
			var line = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "bigLine.png")), 2f), false).AddSpriteHolder(0.1f, 0);
			line.gameObject.layer = 0; // default layer
			line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			line.transform.parent.name = "BasketBallBigLine";
			line.transform.parent.gameObject.AddObjectToEditor();

			sets = RegisterSpecialRoom("BasketballArea", new(1f, 0.339f, 0f));

			room = GetAllAssets(GetRoomAsset("BasketballArea"), 2, 55);
			var swap = new BasicObjectSwapData() { chance = 0.01f, potentialReplacements = [new() { selection = baldiBall.transform, weight = 100 }], prefabToSwap = basketballPile.transform };
			var floorTex = AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "dirtyGrayFloor.png"));
			AddTextureToEditor("dirtyGrayFloor", floorTex);

			room.ForEach(x =>
			{
				x.selection.basicSwaps.Add(swap);
				x.selection.keepTextures = true;
				x.selection.florTex = floorTex;
				x.selection.wallTex = saloonWall.selection;
				x.selection.ceilTex = ceiling.selection;
			});


			room[0].selection.AddRoomFunctionToContainer<SpecialRoomSwingingDoorsBuilder>().swingDoorPre = man.Get<SwingDoor>("swingDoorPre");
			room[0].selection.AddRoomFunctionToContainer<HighCeilingRoomFunction>().ceilingHeight = 9;
			room[0].selection.AddRoomFunctionToContainer<RuleFreeZone>();

			sets.container = room[0].selection.roomFunctionContainer;


			floorDatas[1].SpecialRooms.AddRange(room);
			floorDatas[3].SpecialRooms.AddRange(room);
			floorDatas[2].SpecialRooms.AddRange(room.ConvertAssetWeights(55));

			// **********************************************
			// **********************************************
			// ******************* Forest *******************
			// **********************************************
			// **********************************************

			// Tree
			var tree = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Forest", "forestTree.png")), 8f)).AddSpriteHolder(10.76f, 0).transform.parent;
			tree.gameObject.AddBoxCollider(Vector3.zero, new(0.8f, 10f, 0.8f), false);
			tree.gameObject.AddNavObstacle(new(1.9f, 10f, 1.9f));

			var treeRaycastBlock = new GameObject("TreeRaycastHitbox");
			treeRaycastBlock.AddBoxCollider(Vector3.zero, new(4f, 10f, 4f), false);
			treeRaycastBlock.layer = LayerStorage.blockRaycast;
			treeRaycastBlock.transform.SetParent(tree.transform);
			treeRaycastBlock.transform.localPosition = Vector3.zero;

			tree.name = "Foresttree";
			tree.gameObject.AddObjectToEditor();

			var treeEasterEgg = tree.DuplicatePrefab();
			((SpriteRenderer)treeEasterEgg.GetComponent<RendererContainer>().renderers[0]).sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Forest", "forestTreeEasterEgg.png")), 8f);

			// Campfire
			var campFire = tree.DuplicatePrefab();
			((SpriteRenderer)campFire.GetComponent<RendererContainer>().renderers[0]).sprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Forest", "FireStatic.png")), 27f);
			Object.Destroy(campFire.Find("TreeRaycastHitbox").gameObject); // Not needed for the campfire

			campFire.transform.GetChild(0).localPosition = Vector3.up * 1.2f;
			campFire.name = "Campfire";

			var lgtSrc = campFire.gameObject.AddComponent<LightSourceObject>();
			lgtSrc.colorStrength = 5;
			lgtSrc.colorToLight = new(1f, 0.65f, 0f);

			campFire.gameObject.AddObjectToEditor();
			var audSource = campFire.gameObject.CreateAudioSource(40, 85);
			audSource.spatialBlend = 1f;
			audSource.rolloffMode = AudioRolloffMode.Custom;
			audSource.clip = AssetLoader.AudioClipFromFile(GetRoomAsset("Forest", "fire.wav"));
			audSource.loop = true;

			// BearTrap
			var trapRender = ObjectCreationExtensions.CreateSpriteBillboard(man.Get<Sprite>("BeartrapOpened")).AddSpriteHolder(1f, 0);
			var trap = trapRender.transform.parent.gameObject.AddComponent<PersistentBearTrap>();
			trap.name = "Beartrap";

			trap.gameObject.AddObjectToEditor();
			trap.gameObject.AddBoxCollider(Vector3.zero, new(1.9f, 10f, 1.9f), true);
			trap.gameObject.AddNavObstacle(Vector3.zero, new(2.6f, 10f, 2.6f));

			trap.audMan = trap.gameObject.CreatePropagatedAudioManager(75f, 105f);
			trap.audCatch = man.Get<SoundObject>("BeartrapCatch");
			trap.sprClosed = man.Get<Sprite>("BeartrapClosed");
			trap.sprOpen = trapRender.sprite;
			trap.renderer = trapRender;

			sets = RegisterSpecialRoom("Forest", new(0f, 0.45f, 0f));

			room = GetAllAssets(GetRoomAsset("Forest"), 75, 1);
			//Swap for 99 trees
			 swap = new BasicObjectSwapData() { chance = 0.01f, potentialReplacements = [new() { selection = treeEasterEgg.transform, weight = 100 }], prefabToSwap = tree.transform };
			floorTex = AssetLoader.TextureFromFile(GetRoomAsset("Forest", "treeWall.png"));
			AddTextureToEditor("forestWall", floorTex);

			room.ForEach(x =>
			{
				x.selection.basicSwaps.Add(swap);
				x.selection.keepTextures = true;
				x.selection.florTex = grass;
				x.selection.wallTex = floorTex;
				x.selection.ceilTex = ObjectCreationExtension.transparentTex;
			});


			room[0].selection.AddRoomFunctionToContainer<SpecialRoomSwingingDoorsBuilder>().swingDoorPre = man.Get<SwingDoor>("swingDoorPre");
			room[0].selection.AddRoomFunctionToContainer<RuleFreeZone>();
			room[0].selection.AddRoomFunctionToContainer<WallSoftCoverRoomFunction>();

			var highCeil =  room[0].selection.AddRoomFunctionToContainer<HighCeilingRoomFunction>(); // The first is from the playground
			highCeil.ceilingHeight = 2;
			highCeil.usesSingleCustomWall = true;
			var tex = TextureExtensions.CreateSolidTexture(1, 1, new(0.12156f, 0.12156f, 0.478431f));
			highCeil.customWallProximityToCeil = [tex];
			highCeil.customCeiling = tex;

			var ambience = room[0].selection.AddRoomFunctionToContainer<AmbienceRoomFunction>();
			ambience.source = room[0].selection.roomFunctionContainer.gameObject.CreateAudioSource(35, 125);
			ambience.source.volume = 0;
			ambience.source.clip = AssetLoader.AudioClipFromFile(GetRoomAsset("Forest", "Crickets.wav"));
			ambience.source.loop = true;

			var vignetteCanvas = ObjectCreationExtensions.CreateCanvas();
			vignetteCanvas.gameObject.SetActive(false);
			vignetteCanvas.transform.SetParent(room[0].selection.roomFunctionContainer.transform);
			var vignette = ObjectCreationExtensions.CreateImage(vignetteCanvas, AssetLoader.TextureFromFile(GetRoomAsset("Forest", "darkOverlay.png")));

			var vignetteFunc = room[0].selection.AddRoomFunctionToContainer<VignetteRoomFunction>();
			vignetteFunc.vignette = vignetteCanvas;
			vignetteFunc.fovModifier = -25f;

			sets.container = room[0].selection.roomFunctionContainer;

			floorDatas[2].SpecialRooms.AddRange(room.ConvertAssetWeights(60));


			// ================================================ Base Game Room Variants ====================================================

			

			//Classrooms
			var classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialClassRooms.Length != 0).potentialClassRooms[0];
			room = GetAllAssets(GetRoomAsset("Class"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer);

			room.ForEach(x => {
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
				x.selection.keepTextures = false;
			});

			floorDatas[0].Classrooms.AddRange(room.Where(x => x.selection.activity.prefab.GetType() == typeof(NoActivity))); // why not "is NoActivity"? Well, probably because the game doesn't have the right NET to work like that in runtime
			var activityRooms = room.Where(x => x.selection.activity.prefab.GetType() != typeof(NoActivity));
			for (int i = 1; i < floorDatas.Count; i++)
				floorDatas[i].Classrooms.AddRange(activityRooms);

			// ****** Focus Room (A classroom variant, but with a new npc) ******
			RegisterRoom("FocusRoom", new(0f, 1f, 0.5f),
				ObjectCreators.CreateDoorDataObject("FocusRoomDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "Focus_Room_Door_Opened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "Focus_Room_Door_Closed.png"))));

			var student = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "studentFocused.png")), 25f));
			student.name = "FocusedStudent";
			student.gameObject.AddObjectToEditor();

			var focusedStudent = student.gameObject.AddComponent<FocusedStudent>();
			focusedStudent.audMan = student.gameObject.CreatePropagatedAudioManager(95f, 125f);
			focusedStudent.audAskSilence = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("FocusRoom", "Student_Please.wav")), "Vfx_FocusStd_Disturbed1", SoundType.Voice, Color.white);
			focusedStudent.audAskSilence2 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("FocusRoom", "Student_Please2.wav")), "Vfx_FocusStd_Disturbed2", SoundType.Voice, Color.white);
			focusedStudent.audDisturbed = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("FocusRoom", "Student_scream.wav")), "Vfx_FocusStd_Scream1", SoundType.Voice, Color.white);
			focusedStudent.audDisturbed.additionalKeys = [new() { key = "Vfx_FocusStd_Scream2", time = 1.209f }];

			focusedStudent.renderer = student;
			focusedStudent.sprSpeaking = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "studentSpeaking.png")), 25f);
			focusedStudent.sprScreaming = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "studentScreaming.png")), 25f);
			focusedStudent.sprNormal = student.sprite;

			room = GetAllAssets(GetRoomAsset("FocusRoom"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer);

			room.ForEach(x =>
			{
				x.selection.wallTex = classWeightPre.selection.wallTex;
				x.selection.florTex = classWeightPre.selection.florTex;
				x.selection.ceilTex = classWeightPre.selection.ceilTex;
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.mapMaterial = classWeightPre.selection.mapMaterial;
				x.selection.lightPre = classWeightPre.selection.lightPre;
				x.selection.keepTextures = false;
			});

			for (int i = 1; i < floorDatas.Count; i++)
				floorDatas[i].Classrooms.AddRange(room);


			//Faculties
			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialFacultyRooms.Length != 0).potentialFacultyRooms[0];
			room = GetAllAssets(GetRoomAsset("Faculty"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer);

			room.ForEach(x => {
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Faculties.AddRange(room.FilterRoomAssetsByFloor(i));
			

			//Offices
			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialOffices.Length != 0).potentialOffices[0];
			room = GetAllAssets(GetRoomAsset("Office"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer);

			room.ForEach(x => {
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Offices.AddRange(room.FilterRoomAssetsByFloor(i));

			// Hall
			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialPrePlotSpecialHalls.Length != 0).potentialPrePlotSpecialHalls[0];

			room = GetAllAssets(GetRoomAsset("PrevHalls"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, true);

			room.ForEach(x => {
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Halls.AddRange(room.FilterRoomAssetsByFloor(i).ConvertAll<KeyValuePair<WeightedRoomAsset, bool>>(x => new(x, false))); // Pre halls

			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialPostPlotSpecialHalls.Length != 0).potentialPostPlotSpecialHalls[0];

			room = GetAllAssets(GetRoomAsset("AfterHalls"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, true);

			room.ForEach(x => {
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Halls.AddRange(room.FilterRoomAssetsByFloor(i).ConvertAll<KeyValuePair<WeightedRoomAsset, bool>>(x => new(x, true)));

			static void AddAssetsToNpc<N>(List<WeightedRoomAsset> assets) where N : NPC
			{
				NPCMetaStorage.Instance.All().Do(x =>
				{
					if (x.value is not N)
						return;

					foreach (var npc in x.prefabs)
						npc.Value.potentialRoomAssets = npc.Value.potentialRoomAssets.AddRangeToArray([.. assets]);
				});

			}

			static void ClearNpcAssets<N>() where N : NPC
			{
				NPCMetaStorage.Instance.All().Do(x =>
				{
					if (x.value is not N)
						return;

					foreach (var npc in x.prefabs)
						npc.Value.potentialRoomAssets = [];
					
				});
			}
		}

		static string GetRoomAsset(string roomName, string asset = "") => Path.Combine(BasePlugin.ModPath, "rooms", roomName, asset);

		static void AddTextureToEditor(string name, Texture2D tex)
		{
			PlusLevelLoaderPlugin.Instance.textureAliases.Add(name, tex);
			man.Add("editorTexture_" + name, tex);
		}

		static void AddObjectToEditor(this GameObject obj)
		{
			PlusLevelLoaderPlugin.Instance.prefabAliases.Add(obj.name, obj);
			man.Add($"editorPrefab_{obj.name}", obj);
			obj.ConvertToPrefab(true);
		}

		static RoomSettings RegisterRoom(string roomName, Color color, StandardDoorMats mat)
		{
			var settings = new RoomSettings(EnumExtensions.ExtendEnum<RoomCategory>(roomName), RoomType.Room, color, mat);
			PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
			return settings;
		}

		static RoomSettings RegisterRoom(string roomName, RoomCategory en, Color color, StandardDoorMats mat)
		{
			var settings = new RoomSettings(en, RoomType.Room, color, mat);
			PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
			return settings;
		}

		static RoomSettings RegisterSpecialRoom(string roomName, Color color)
		{
			var settings = new RoomSettings(RoomCategory.Special, RoomType.Room, color, man.Get<StandardDoorMats>("ClassDoorSet"));
			PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
			return settings;
		}

		static List<WeightedRoomAsset> GetAllAssets(string path, int maxValue, int assetWeight, bool isOffLimits = false, RoomFunctionContainer cont = null, bool isAHallway = false, bool secretRoom = false, Texture2D mapBg = null)
		{
			List<WeightedRoomAsset> assets = [];
			RoomFunctionContainer container = cont;
			foreach (var file in Directory.GetFiles(path))
			{
				try
				{
					var asset = RoomFactory.CreateAssetFromPath(file, maxValue, isOffLimits, container, isAHallway, secretRoom, mapBg);
					assets.Add(new() { selection = asset, weight = assetWeight });
					_moddedAssets.Add(asset);
					if (!container)
						container = asset.roomFunctionContainer;
				}
				catch { } // supress exception
			}

			return assets;
		}
		static List<WeightedRoomAsset> FilterRoomAssetsByFloor(this List<WeightedRoomAsset> assets, int floor)
		{
			var newAss = new List<WeightedRoomAsset>(assets);
			System.Exception e = null;
			try
			{
				for (int i = 0; i < newAss.Count; i++)
				{
					string[] rawData = newAss[i].selection.name.Split('!');
					string[] sdata = rawData[1].Split(',');
					int[] data = new int[sdata.Length];

					for (int z = 0; z < sdata.Length; z++) // Converts the string numbers into integers
						data[z] = int.Parse(sdata[z]);

					if (!data.Contains(floor)) // Finally remove the asset if it isn't for the intended floor
						newAss.RemoveAt(i--);
					
					if (rawData.Length > 2)
					{
						int val = int.Parse(rawData[2]);
						newAss[i].selection.maxItemValue = val;
						if (val >= 200)
							newAss[i].weight = 10; // Weight of a standard rare faculty
					}
				}
			}
			catch (System.IndexOutOfRangeException ex)
			{
				Debug.LogError("Failed to get data from the room collection due to invalid format in the data!");
				e = ex;
			}
			catch (System.FormatException ex)
			{
				Debug.LogError("Failed to get data from the room collection due to invalid format in the numbers!");
				e = ex;
			}
			finally
			{
				if (e != null)
					Debug.LogException(e);
			}

			return newAss;
		}

		static List<WeightedRoomAsset> ConvertAssetWeights(this List<WeightedRoomAsset> assets, int newWeight)
		{
			for (int i = 0; i < assets.Count; i++)
				assets[i] = new() { selection = assets[i].selection, weight = newWeight };
			return assets;
		}

		readonly static List<RoomAsset> _moddedAssets = [];
	}
}
