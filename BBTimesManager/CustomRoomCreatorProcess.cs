using BBTimes.CustomContent.CustomItems;
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
			var lightPre = GenericExtensions.FindResourceObjects<RoomAsset>().First(x => x.category == RoomCategory.Class).lightPre;
			var carpet = GenericExtensions.FindResourceObjectByName<Texture2D>("Carpet");
			var ceiling = GenericExtensions.FindResourceObjectByName<Texture2D>("CeilingNoLight");
			var saloonWall = GenericExtensions.FindResourceObjectByName<Texture2D>("SaloonWall");
			var blackTexture = TextureExtensions.CreateSolidTexture(1, 1, Color.black); // It'll be stretched anyways lol

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


			var room = GetAllAssets(GetRoomAsset("Bathroom"), bathLightPre, 50, 15, 155, 45);
			var fun = room[0].selection.AddRoomFunctionToContainer<PosterAsideFromObject>();
			fun.targetPrefabName = "sink";
			fun.posterPre = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "mirror.png"))]);

			room.Do(x =>
			{
				x.selection.SetPotentialPosters(0f);
			});
			sets.container = room[0].selection.roomFunctionContainer;

			var group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Standard,
				stickToHallChance = 1f,
				generateDoors = true,
				minRooms = 0,
				maxRooms = 2,
				potentialAssets = [room[0]],
				priority = RoomGroupPriority.BeforeExtraRooms,
				textureGroupName = "Bathroom"
			};

			floorDatas[0].RoomAssets.Add(new() { name = "Bathroom" }, group);

			group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Standard,
				stickToHallChance = 0.7f,
				generateDoors = true,
				minRooms = 1,
				maxRooms = 2,
				potentialAssets = [room[0], room[1]],
				priority = RoomGroupPriority.BeforeExtraRooms,
				textureGroupName = "Bathroom"
			};

			floorDatas[1].RoomAssets.Add(new() { name = "Bathroom" }, group);
			floorDatas[3].RoomAssets.Add(new() { name = "Bathroom" }, group);
			group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Standard,
				stickToHallChance = 0.45f,
				generateDoors = true,
				minRooms = 3,
				maxRooms = 5,
				potentialAssets = [.. room],
				priority = RoomGroupPriority.BeforeExtraRooms,
				textureGroupName = "Bathroom"
			};
			floorDatas[2].RoomAssets.Add(new() { name = "Bathroom" }, group);

			// *******************************************************
			// *******************************************************
			// ******************* Abandoned Rooms *******************
			// *******************************************************
			// *******************************************************			

			sets = RegisterRoom("AbandonedRoom", new(0.59765625f, 0.19921875f, 0f),
				ObjectCreators.CreateDoorDataObject("OldDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "oldDoorOpen.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "oldDoorClosed.png"))));

			room = GetAllAssets(GetRoomAsset("AbandonedRoom"), lightPre, 50, 0, 250, 45);
			room[0].selection.AddRoomFunctionToContainer<ShowItemsInTheEnd>();
			room.Do(x =>
			{
				x.selection.SetPotentialPosters(0f);
			});
			sets.container = room[0].selection.roomFunctionContainer;

			group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Standard,
				stickToHallChance = 0.45f,
				generateDoors = true,
				minRooms = 1,
				maxRooms = 1,
				potentialAssets = [.. room],
				priority = RoomGroupPriority.AfterAll,
				textureGroupName = "AbandonedRoom"
			};
			floorDatas[2].RoomAssets.Add(new()
			{
				name = "AbandonedRoom",
				potentialCeilTextures = [new() { selection = ceiling, weight = 50 }],
				potentialFloorTextures = [new() { selection = carpet, weight = 50 }]
			},
				group);

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


			room = GetAllAssets(GetRoomAsset("ComputerRoom"), lightPre, 45, 0, 75, 50);
			room[0].selection.AddRoomFunctionToContainer<RandomPosterFunction>().posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", "ComputerPoster.png"))])];
			room[0].selection.AddRoomFunctionToContainer<EventMachineSpawner>().machinePre = evMac;

			sets.container = room[0].selection.roomFunctionContainer;

			group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Standard,
				stickToHallChance = 1f,
				generateDoors = true,
				minRooms = 1,
				maxRooms = 1,
				potentialAssets = [room[0], room[1]],
				priority = RoomGroupPriority.BeforeOffice,
				textureGroupName = "ComputerRoom"
			};

			floorDatas[0].RoomAssets.Add(new() { name = "ComputerRoom" }, group);

			group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Exits,
				stickToHallChance = 1f,
				generateDoors = true,
				minRooms = 1,
				maxRooms = 2,
				potentialAssets = [room[0], room[2]],
				priority = RoomGroupPriority.BeforeOffice,
				textureGroupName = "ComputerRoom"
			};

			floorDatas[1].RoomAssets.Add(new() { name = "ComputerRoom" }, group);
			floorDatas[3].RoomAssets.Add(new() { name = "ComputerRoom" }, group);

			group = new RoomTypeGroup()
			{
				spawnMethod = RoomGroupSpawnMethod.Exits,
				stickToHallChance = 1f,
				generateDoors = true,
				minRooms = 1,
				maxRooms = 3,
				potentialAssets = [.. room],
				priority = RoomGroupPriority.BeforeOffice,
				textureGroupName = "ComputerRoom"
			};

			floorDatas[2].RoomAssets.Add(new() { name = "ComputerRoom" }, group);

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

			room = GetAllAssets(GetRoomAsset("DribbleRoom"), lightPre, 45, 0, 75, 50);
			room[0].selection.AddRoomFunctionToContainer<RuleFreeZone>();
			room[0].selection.AddRoomFunctionToContainer<PlayerRunCornerFunction>();

			sets.container = room[0].selection.roomFunctionContainer;

			room.Do(x =>
			{
				x.selection.wallTex = saloonWall;
				x.selection.ceilTex = ceiling;
			});

			AddAssetsToNpc<Dribble>(room);





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

			room = GetAllAssets(GetRoomAsset("BasketballArea"), null, 75, 1, 2, 55);
			var swap = new BasicObjectSwapData() { chance = 0.01f, potentialReplacements = [new() { selection = baldiBall.transform, weight = 100 }], prefabToSwap = basketballPile.transform };
			var floorTex = AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "dirtyGrayFloor.png"));
			AddTextureToEditor("dirtyGrayFloor", floorTex);

			room.Do(x =>
			{
				x.selection.basicSwaps.Add(swap);
				x.selection.keepTextures = true;
				x.selection.florTex = floorTex;
				x.selection.wallTex = saloonWall;
				x.selection.ceilTex = ceiling;
			});


			room[0].selection.AddRoomFunctionToContainer<SpecialRoomSwingingDoorsBuilder>().swingDoorPre = man.Get<SwingDoor>("swingDoorPre");
			room[0].selection.AddRoomFunctionToContainer<HighCeilingRoomFunction>().ceilingHeight = 9;
			room[0].selection.AddRoomFunctionToContainer<RuleFreeZone>();

			sets.container = room[0].selection.roomFunctionContainer;


			floorDatas[1].SpecialRooms.AddRange(room);
			floorDatas[3].SpecialRooms.AddRange(room);
			floorDatas[2].SpecialRooms.AddRange(room.ConvertAssetWeights(55));

			// ================================================ Base Game Room Variants ====================================================

			//Classrooms
			var classPre = GenericExtensions.FindResourceObjects<RoomAsset>().First(x => x.category == RoomCategory.Class);
			var classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialClassRooms.Length != 0).potentialClassRooms[0];
			room = GetAllAssets(GetRoomAsset("Class"), lightPre, classPre.spawnWeight, classPre.minItemValue, classPre.maxItemValue, classWeightPre.weight, classPre.offLimits, classPre.roomFunctionContainer);

			floorDatas[0].Classrooms.AddRange(room.Where(x => x.selection.activity.prefab.GetType() == typeof(NoActivity))); // why not "is NoActivity"? Well, probably because the game doesn't have the right NET to work like that in runtime
			var activityRooms = room.Where(x => x.selection.activity.prefab.GetType() != typeof(NoActivity));
			for (int i = 1; i < floorDatas.Count; i++)
				floorDatas[i].Classrooms.AddRange(activityRooms);

			//Faculties
			classPre = GenericExtensions.FindResourceObjects<RoomAsset>().First(x => x.category == RoomCategory.Faculty);
			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialFacultyRooms.Length != 0).potentialFacultyRooms[0];
			room = GetAllAssets(GetRoomAsset("Faculty"), lightPre, classPre.spawnWeight, classPre.minItemValue, classPre.maxItemValue, classWeightPre.weight, classPre.offLimits, classPre.roomFunctionContainer);

			floorDatas.Do(x => x.Faculties.AddRange(room));

			//Offices
			classPre = GenericExtensions.FindResourceObjects<RoomAsset>().First(x => x.category == RoomCategory.Office);
			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialOffices.Length != 0).potentialOffices[0];
			room = GetAllAssets(GetRoomAsset("Office"), lightPre, classPre.spawnWeight, classPre.minItemValue, classPre.maxItemValue, classWeightPre.weight, classPre.offLimits, classPre.roomFunctionContainer);

			floorDatas.Do(x => x.Offices.AddRange(room));




			static void AddAssetsToNpc<N>(List<WeightedRoomAsset> assets) where N : NPC
			{
				NPCMetaStorage.Instance.All().Do(x =>
				{
					foreach (var npc in x.prefabs)
						if (npc.Value is N)
							npc.Value.potentialRoomAssets = npc.Value.potentialRoomAssets.AddRangeToArray([.. assets]);
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

		static RoomSettings RegisterSpecialRoom(string roomName, Color color)
		{
			var settings = new RoomSettings(RoomCategory.Special, RoomType.Room, color, man.Get<StandardDoorMats>("ClassDoorSet"));
			PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
			return settings;
		}


		static RoomSettings RegisterCustomHall(string roomName)
		{
			var settings = new RoomSettings(RoomCategory.Hall, RoomType.Hall, Color.white, man.Get<StandardDoorMats>("ClassDoorSet"));
			PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
			return settings;
		}

		static List<WeightedRoomAsset> GetAllAssets(string path, Transform lightPre, int spawnWeight, int minValue, int maxValue, int assetWeight, bool isOffLimits = false, RoomFunctionContainer cont = null)
		{
			List<WeightedRoomAsset> assets = [];
			RoomFunctionContainer container = cont;
			foreach (var file in Directory.GetFiles(path))
			{
				try
				{
					var asset = CustomRoomExtensions.GetAssetFromPath(file, spawnWeight, lightPre, minValue, maxValue, isOffLimits, container);
					assets.Add(new() { selection = asset, weight = assetWeight });
					_moddedAssets.Add(asset);
					if (!container)
						container = asset.roomFunctionContainer;
				}
				catch { } // supress exception
			}

			return assets;
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
