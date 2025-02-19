﻿using BBTimes.CustomComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.Events;
using BBTimes.CustomContent.Misc;
using BBTimes.CustomContent.NPCs;
using BBTimes.CustomContent.Objects;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
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
using NewPlusDecorations;
using BBTimes.Plugin;
using TMPro;
using MTM101BaldAPI.OBJImporter;
using BBTimes.CustomComponents.NpcSpecificComponents;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateCustomRooms()
		{
			var lightPre = new WeightedTransform() { selection = Resources.FindObjectsOfTypeAll<RoomAsset>().Last(x => x.category == RoomCategory.Class).lightPre }; // Last because it should be the original asset
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
			bathStall.AddContainer(bathStall.GetComponent<MeshRenderer>());

			bathStall.transform.localScale = new(9.9f, 10f, 1f);
			bathStall.AddObjectToEditor();

			var bathSprites = TextureExtensions.LoadSpriteSheet(2, 1, 25f, GetRoomAsset("Bathroom", "BathDoor.png"));
			var bathDoor = new GameObject("bathDoor");
			bathDoor.gameObject.AddBoxCollider(Vector3.up * 5f, new(9.9f, 10f, 1f), true);
			bathDoor.layer = LayerStorage.iClickableLayer;

			var bathDoorRenderer = ObjectCreationExtensions.CreateSpriteBillboard(bathSprites[0], false);
			bathDoorRenderer.transform.SetParent(bathDoor.transform);
			bathDoorRenderer.transform.localPosition = Vector3.up * 5f;
			bathDoorRenderer.name = "BathDoorVisual";
			bathDoorRenderer.transform.localScale = new(0.976f, 1f, 1f);
			bathDoor.AddContainer(bathDoorRenderer);
			Object.Destroy(bathDoorRenderer.GetComponent<RendererContainer>());

			var bathDoorCollider = new GameObject("BathdoorCollider");
			bathDoorCollider.transform.SetParent(bathDoor.transform);
			bathDoorCollider.transform.localPosition = Vector3.zero;

			var collider = bathDoorCollider.gameObject.AddBoxCollider(Vector3.zero, new(9.9f, 10f, 0.7f), false);
			bathDoor.gameObject.AddObjectToEditor();

			var genDor = bathDoor.gameObject.AddComponent<GenericDoor>();
			genDor.audMan = bathDoor.gameObject.CreatePropagatedAudioManager(135f, 195f);
			genDor.audOpen = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("Bathroom", "bathDoorOpen.wav")), "Sfx_Doors_StandardOpen", SoundType.Voice, Color.white);
			genDor.audClose = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("Bathroom", "bathDoorShut.wav")), "Sfx_Doors_StandardShut", SoundType.Voice, Color.white);
			genDor.colliders = [collider];
			genDor.closed = bathSprites[0];
			genDor.open = bathSprites[1];
			genDor.renderer = bathDoorRenderer;


			bathSprites = TextureExtensions.LoadSpriteSheet(2, 1, 41.5f, GetRoomAsset("Bathroom", "sink.png"));
			var bathSink = ObjectCreationExtensions.CreateSpriteBillboard(bathSprites[0])
				.AddSpriteHolder(out var bathSinkRenderer, 2f, LayerStorage.iClickableLayer);
			bathSinkRenderer.name = "sink";
			bathSink.name = "sink";
			bathSink.gameObject.AddObjectToEditor();

			var bathSinkFunction = bathSink.gameObject.AddComponent<TimedFountain>();
			bathSinkFunction.renderer = bathSinkRenderer;
			bathSinkFunction.sprEnabled = bathSprites[0];
			bathSinkFunction.sprDisabled = bathSprites[1];
			bathSinkFunction.refillValue = 15f;

			bathSinkFunction.gameObject.layer = LayerStorage.iClickableLayer;
			bathSinkFunction.gameObject.AddBoxCollider(Vector3.zero, new(0.8f, 10f, 0.8f), true);
			bathSinkFunction.audMan = bathSinkFunction.gameObject.CreatePropagatedAudioManager(15f, 35f);
			bathSinkFunction.audSip = man.Get<SoundObject>("audSlurp");

			collider = new GameObject("sinkCollider").AddBoxCollider(Vector3.zero, new(0.6f, 10f, 0.6f), false);
			collider.gameObject.AddNavObstacle(new(1f, 10f, 1f));
			collider.gameObject.layer = LayerStorage.ignoreRaycast;
			collider.transform.SetParent(bathSink.transform);
			collider.transform.localPosition = Vector3.zero;

			

			var toilet = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "toilet.png")), 35f)).AddSpriteHolder(out var toiletRenderer, 2f, LayerStorage.ignoreRaycast);
			toiletRenderer.name = "toiletRenderer";
			toilet.name = "Toilet";
			toilet.gameObject.AddBoxCollider(Vector3.zero, new(0.8f, 10f, 0.8f), false);
			toilet.gameObject.AddNavObstacle(new(1.2f, 10f, 1.2f));

			toilet.gameObject.AddObjectToEditor();


			var bathLightPre = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "long_hanginglamp.png")), 50f))
				.AddSpriteHolder(out _, 8.98f).transform;
			bathLightPre.name = "hangingLongLight";
			bathLightPre.gameObject.ConvertToPrefab(true);

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

			var renderer = table.AddContainer(tableHead.GetComponent<MeshRenderer>());

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
				renderer.renderers = renderer.renderers.AddToArray(machineWheel.GetComponent<MeshRenderer>());
			}

			

			table.AddObjectToEditor();

			// Computer

			var computer = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", GetAssetName("computer.png"))), 25f));
			computer.name = "ComputerBillboard";
			computer.gameObject.AddObjectToEditor();

			// Event machine
			Sprite[] sprs = TextureExtensions.LoadSpriteSheet(3, 1, 26f, GetRoomAsset("ComputerRoom", GetAssetName("eventMachine.png")));
			var machine = ObjectCreationExtensions.CreateSpriteBillboard(sprs[1], false);
			machine.gameObject.layer = 0;
			machine.name = "EventMachine";
			machine.gameObject.ConvertToPrefab(true);
			var evMac = machine.gameObject.AddComponent<EventMachine>();
			evMac.spriteToChange = machine;
			evMac.sprNoEvents = machine.sprite;
			evMac.sprWorking = sprs[2];
			evMac.sprDead = sprs[0];
			// Audio Setup from event machine
			evMac.audBalAngry = [
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("ComputerRoom", "Bal_NoEvent1.wav")), "Vfx_BAL_NoEvent0_0", SoundType.Voice, Color.green),
				ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("ComputerRoom", "Bal_NoEvent2.wav")), "Vfx_BAL_NoEvent1_0", SoundType.Voice, Color.green),
				];

			evMac.audBalAngry[0].additionalKeys = [new() { key = "Vfx_BAL_NoEvent0_1", time = 2.556f }];
			evMac.audBalAngry[1].additionalKeys = [new() { key = "Vfx_BAL_NoEvent1_1", time = 1.841f }];

			machine.gameObject.AddBoxCollider(Vector3.forward * -1.05f, new(6f, 10f, 1f), true);

			// ComputerTeleporter
			sprs = TextureExtensions.LoadSpriteSheet(3, 2, 12.5f, GetRoomAsset("ComputerRoom", GetAssetName("ComputerTeleporter.png")));
			var machineComputer = ObjectCreationExtensions.CreateSpriteBillboard(sprs[5]).AddSpriteHolder(out machine, 5f, LayerStorage.ignoreRaycast);
			machine.name = "Sprite";

			var teleporter = machineComputer.gameObject.AddComponent<ComputerTeleporter>();
			teleporter.name = "ComputerTeleporter";

			teleporter.gameObject.AddObjectToEditor();
			teleporter.gameObject.AddBoxCollider(Vector3.up * 5f, new(5f, 10f, 5f), true);
			teleporter.sprDisabled = machine.sprite;

			teleporter.audMan = teleporter.gameObject.CreatePropagatedAudioManager(maxDistance: 40f);
			teleporter.loopingAudMan = teleporter.gameObject.CreatePropagatedAudioManager(maxDistance: 40f);
			teleporter.audTeleport = man.Get<SoundObject>("teleportAud");
			teleporter.audLoop = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("ComputerRoom", GetAssetName("teleportationNoises.wav"))), "Vfx_CompTel_Working", SoundType.Voice, Color.white);

			teleporter.animComp = teleporter.gameObject.AddComponent<AnimationComponent>();
			teleporter.animComp.renderers = [machine];
			teleporter.animComp.animation = [.. sprs.Take(5)];
			teleporter.animComp.speed = 11.5f;

			// Item Descriptor
			var descriptorObj = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("ComputerRoom", GetAssetName("item_descriptor.png"))), 13f))
				.AddSpriteHolder(out var descriptorRenderer, -0.15f, LayerStorage.iClickableLayer);
			descriptorObj.name = "TimesItemDescriptor";
			descriptorRenderer.name = "TimesItemDescriptor_Renderer";

			descriptorObj.gameObject.AddObjectToEditor();

			descriptorObj.gameObject.AddBoxCollider(Vector3.zero, new(2.5f, 5f, 2.5f), true);
			var descriptor = descriptorObj.gameObject.AddComponent<ItemDescriptor>();
			descriptor.audMan = descriptorObj.gameObject.CreatePropagatedAudioManager(45f, 65f);
			descriptor.audTap = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("ComputerRoom", GetAssetName("descriptor_beep.wav"))), string.Empty, SoundType.Voice, Color.white);
			descriptor.audTap.subtitle = false;

			descriptor.text = new GameObject("DescriptorText", typeof(BillboardRotator)).AddComponent<TextMeshPro>();
			descriptor.text.transform.SetParent(descriptorObj.transform);
			descriptor.text.transform.localPosition = Vector3.up * 2f;
			descriptor.text.fontSize = 5.5f;
			descriptor.text.richText = true; // Allow html stuff... I hope this is the right setting
			descriptor.text.color = new(0.16796875f, 0.16796875f, 0.31640625f);
			descriptor.text.alignment = TextAlignmentOptions.Center;
			descriptor.text.wordSpacing = 5;
			descriptor.text.rectTransform.sizeDelta = new(8.15f, 5f);
			descriptor.text.text = Singleton<LocalizationManager>.Instance.GetLocalizedText("PST_ItemDescriptor_NoDescription");

			//***************************************************
			//***************************************************
			//************* Dribble's Room **********************
			//***************************************************
			//***************************************************
			var runLine = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("DribbleRoom", "lineStraight.png")), 12.5f), false).AddSpriteHolder(out var runLineRenderer, 0.1f, 0);
			//runLine.gameObject.layer = 0; // default layer
			runLineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			runLine.name = "StraightRunLine";
			runLine.gameObject.AddObjectToEditor();

			runLine = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("DribbleRoom", "lineCurve.png")), 12.5f), false).AddSpriteHolder(out runLineRenderer, 0.1f, 0);
			//runLine.gameObject.layer = 0; // default layer
			runLineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			runLine.name = "CurvedRunLine";
			runLine.gameObject.AddObjectToEditor();

			// ***********************************************
			// ***********************************************
			// ******************* Kitchen *******************
			// ***********************************************
			// ***********************************************

			// Kitchen "table"
			var shelf = new GameObject("KitchenCabinet");
			shelf.gameObject.AddBoxCollider(Vector3.zero, new(10f, 10f, 10f), false);
			//shelf.gameObject.AddNavObstacle(new(10f, 10f, 10f)); Not required as the player can't go through anyways. This is for guarantee that wandering npcs just don't go through walls after going past these structures
			shelf.layer = LayerStorage.ignoreRaycast;
			var renderers = new Renderer[2];

			var shelfBody = ObjectCreationExtension.CreateCube(Object.Instantiate(man.Get<Texture2D>("plasticTexture")).ApplyLightLevel(-45f), false);
			shelfBody.transform.SetParent(shelf.transform);
			shelfBody.transform.localPosition = Vector3.up * 2.5f;
			shelfBody.transform.localScale = new(9.9f, 1f, 9.9f);
			Object.Destroy(shelfBody.GetComponent<Collider>());
			renderers[0] = shelfBody.GetComponent<MeshRenderer>();


			shelfBody = ObjectCreationExtension.CreateCube(man.Get<Texture2D>("plasticTexture"), false);
			shelfBody.transform.SetParent(shelf.transform);
			shelfBody.transform.localPosition = Vector3.up * 0.7f;
			shelfBody.transform.localScale = new(7.5f, 4f, 7.5f);
			Object.Destroy(shelfBody.GetComponent<Collider>());
			renderers[1] = shelfBody.GetComponent<MeshRenderer>();

			shelf.AddContainer(renderers);

			shelf.gameObject.AddObjectToEditor();

			// Joe

			var joe = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "JoeChef.png")), 29f)).AddSpriteHolder(out _, 0f, LayerStorage.iClickableLayer);
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

			var joeHolder = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(out var joeHolderRenderer, new Vector3(-3.3f, 2f, 0f), 0);
			joeHolder.transform.SetParent(joe.transform);
			joeHolder.transform.localPosition = Vector3.zero;
			joeHolder.gameObject.AddComponent<BillboardRotator>();
			joeChef.itemRenderer = joeHolderRenderer;

			JoeChef.AddFood(ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value, 15);
			JoeChef.AddFood(ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value, 45);
			JoeChef.AddFood(ItemMetaStorage.Instance.FindByEnum(Items.Apple).value, 5);

			// Joe's Sign
			var joeSign = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "JoeChefSign.png")), 15f)).AddSpriteHolder(out _, 0f, LayerStorage.ignoreRaycast);
			joeSign.name = "JoeSign";
			joeSign.gameObject.AddBoxCollider(Vector3.zero, Vector3.one, true);
			joeSign.gameObject.AddObjectToEditor();


			// ================================================ Modded Special Room Creation ====================================================

			// *******************************************************
			// *******************************************************
			// ******************* Basketball Area *******************
			// *******************************************************
			// *******************************************************	

			// Hoop

			var hoop = Object.Instantiate(GenericExtensions.FindResourceObjectByName<RendererContainer>("HoopBase"));
			hoop.transform.localScale = Vector3.one * 3f;
			var hoopCapsuleCollider = hoop.GetComponent<CapsuleCollider>();
			hoopCapsuleCollider.radius = 0.35f;
			hoopCapsuleCollider.center = new(0, 4.751f, 3.5f);

			var hoopNavObstacle = hoop.GetComponent<NavMeshObstacle>();
			hoopNavObstacle.radius = 0.75f;
			hoopNavObstacle.center = new(0f, 4.751f, 3.5f);

			hoop.name = "BasketHoop";
			hoop.gameObject.AddObjectToEditor();

			// Grand Stand

			var grandStand = ObjectCreationExtension.CreateCube(TextureExtensions.CreateSolidTexture(1, 1, Color.gray), false);
			grandStand.name = "GrandStand";
			grandStand.AddObjectToEditor();
			grandStand.transform.localScale = new(8f, 8f, 45f);
			grandStand.gameObject.AddNavObstacle(new(1f, 10f, 1f));
			grandStand.AddContainer(grandStand.GetComponent<MeshRenderer>());

			// Basket Machine

			var basketMachine = new GameObject("BasketMachine");
			basketMachine.AddNavObstacle(new(4.5f, 10f, 4.5f));
			basketMachine.AddBoxCollider(Vector3.zero, new(4f, 10f, 4f), false);

			var machineBody = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "machineTexture.png")), false);
			machineBody.transform.SetParent(basketMachine.transform);
			machineBody.transform.localScale = new(4f, 5f, 4f);
			machineBody.transform.localPosition = Vector3.up * 3.2f;
			Object.Destroy(machineBody.GetComponent<Collider>());

			// Cannon
			var machineCannon = ObjectCreationExtension.CreateCube(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "cannonTexture.png")));
			machineCannon.transform.SetParent(basketMachine.transform);
			machineCannon.transform.localScale = new(1f, 1f, 3f);
			machineCannon.transform.localPosition = new(-0.5f, 4.13f, 0.55f);
			Object.Destroy(machineCannon.GetComponent<Collider>());

			renderer = basketMachine.AddContainer(machineBody.GetComponent<MeshRenderer>(), machineCannon.GetComponent<MeshRenderer>());

			void WheelCreator(Vector3 pos)
			{
				var machineWheel = ObjectCreationExtension.CreatePrimitiveObject(PrimitiveType.Cylinder, blackTexture);
				machineWheel.transform.SetParent(basketMachine.transform);
				machineWheel.transform.localPosition = pos;
				machineWheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
				machineWheel.transform.localScale = new(1f, 0.4f, 1f);
				Object.Destroy(machineWheel.GetComponent<Collider>());
				renderer.renderers = renderer.renderers.AddToArray(machineWheel.GetComponent<MeshRenderer>());
			}

			WheelCreator(new(-2f, 0.5f, 0f));
			WheelCreator(new(2f, 0.5f, 0f));

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
			var line = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "bigLine.png")), 2f), false).AddSpriteHolder(out runLineRenderer, 0.1f, 0);
			//line.gameObject.layer = 0; // default layer
			runLineRenderer.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			line.name = "BasketBallBigLine";
			line.gameObject.AddObjectToEditor();

			// **********************************************
			// **********************************************
			// ******************* Forest *******************
			// **********************************************
			// **********************************************

			// Tree
			var tree = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(GetRoomAsset("Forest", "forestTree.png")), 8f)).AddSpriteHolder(out _, 10.76f, 0);
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
			Object.Destroy(campFire.transform.Find("TreeRaycastHitbox").gameObject); // Not needed for the campfire

			campFire.transform.GetChild(0).localPosition = Vector3.up * 1.2f;
			campFire.name = "Campfire";

			var lgtSrc = campFire.gameObject.AddComponent<LightSourceObject>();
			lgtSrc.colorStrength = 5;
			lgtSrc.colorToLight = new(1f, 0.65f, 0f);

			var campFireAud = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("Forest", "fire.wav")), string.Empty, SoundType.Voice, Color.white);
			campFireAud.subtitle = false;
			campFire.gameObject.AddObjectToEditor();
			var audSource = campFire.gameObject.CreatePropagatedAudioManager(40f, 80f)
				.AddStartingAudiosToAudioManager(true, campFireAud);

			// BearTrap
			var trap = ObjectCreationExtensions.CreateSpriteBillboard(man.Get<Sprite[]>("Beartrap")[1]).AddSpriteHolder(out var trapRender, 1f, 0).gameObject.AddComponent<PersistentBearTrap>();
			trap.name = "Beartrap";

			trap.gameObject.AddObjectToEditor();
			trap.gameObject.AddBoxCollider(Vector3.zero, new(1.9f, 10f, 1.9f), true);
			trap.gameObject.AddNavObstacle(Vector3.zero, new(2.6f, 10f, 2.6f));

			trap.audMan = trap.gameObject.CreatePropagatedAudioManager(75f, 105f);
			trap.audCatch = man.Get<SoundObject>("BeartrapCatch");
			trap.sprClosed = man.Get<Sprite[]>("Beartrap")[0];
			trap.sprOpen = trapRender.sprite;
			trap.renderer = trapRender;

			// ********************************************************
			// ********************************************************
			// ******************* SNOWY PLAYGROUND *******************
			// ********************************************************
			// ********************************************************

			var snowyTree = Object.Instantiate(GenericExtensions.FindResourceObjectByName<RendererContainer>("TreeCG"));
			snowyTree.name = "SnowyPlaygroundTree";
			snowyTree.gameObject.AddObjectToEditor();
			var snowyTreeRenderer = (MeshRenderer)snowyTree.renderers[0];
			snowyTreeRenderer.material.SetTexture("_MainTex", AssetLoader.TextureFromFile(GetRoomAsset("SnowyPlayground", "snowyTreeCG.png")));

			var snowPile = new OBJLoader().Load(
				GetRoomAsset("SnowyPlayground", "SnowPile.obj"),
				GetRoomAsset("SnowyPlayground", "SnowPile.mtl"),
				ObjectCreationExtension.defaultMaterial);
			snowPile.name = "SnowPile";
			SetupObjCollisionAndScale(snowPile, new(9.7f, 10f, 9.7f), 0.2f);

			snowPile.AddObjectToEditor();

			var snowPileComp = snowPile.AddComponent<SnowPile>();
			snowPileComp.collider = snowPile.AddBoxCollider(Vector3.up * 7.5f, new(5f, 10f, 5f), true);
			snowPileComp.mainRenderer = snowPile.transform.GetChild(0).gameObject; // The renderer

			snowPileComp.audMan = snowPile.CreatePropagatedAudioManager(55f, 75f);
			snowPileComp.audPop = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("SnowyPlayground", "lowPop.wav")), "Sfx_Effects_Pop", SoundType.Effect, Color.white);

			var system = new GameObject("snowPileParticles").AddComponent<ParticleSystem>();
			system.transform.SetParent(snowPileComp.transform);
			system.transform.localPosition = Vector3.up * 1.25f;
			system.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = AssetLoader.TextureFromFile(GetRoomAsset("SnowyPlayground", "blueSnowBall.png")) };

			var main = system.main;
			main.gravityModifierMultiplier = 0.95f;
			main.startLifetimeMultiplier = 9f;
			main.startSpeedMultiplier = 4f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(0.5f, 2f);

			var emission = system.emission;
			emission.enabled = false;

			var vel = system.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.Local;
			vel.x = new(-15f, 15f);
			vel.y = new(25f, 35f);
			vel.z = new(-15f, 15f);

			snowPileComp.snowPopParts = system;

			// ********** Shovel Code ************

			var shovel = new OBJLoader().Load(
				GetRoomAsset("SnowyPlayground", "shovel.obj"),
				GetRoomAsset("SnowyPlayground", "shovel.mtl"),
				ObjectCreationExtension.defaultMaterial
				);
			shovel.name = "Shovel_ForSnowPile";
			SetupObjCollisionAndScale(shovel, default, 0.2f, false, false);

			shovel.AddObjectToEditor();

			var snowShovel = shovel.AddComponent<SnowShovel>();
			snowShovel.normalRender = snowShovel.transform.GetChild(0).gameObject;
			snowShovel.clickableCollision = shovel.AddBoxCollider(Vector3.up * 5f, new(5f, 10f, 2.5f), true);

			snowShovel.holdRender = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromFile(GetRoomAsset("SnowyPlayground", "shovelRender.png"), Vector2.one * 0.5f, 23f)).gameObject;
			snowShovel.holdRender.transform.SetParent(snowShovel.transform);
			snowShovel.holdRender.transform.localPosition = Vector3.zero;
			snowShovel.holdRender.name = "SnowShovel2DRender";
			snowShovel.holdRender.gameObject.SetActive(false);

			var snowShovel_timerOffsetObj = new GameObject("Timer_OffsetHolder");
			snowShovel_timerOffsetObj.transform.SetParent(snowShovel.transform);
			snowShovel_timerOffsetObj.transform.localPosition = Vector3.up * 5f;

			snowShovel.timer = Object.Instantiate(man.Get<TextMeshPro>("genericTextMesh"));
			snowShovel.timer.name = "Timer";
			snowShovel.timer.transform.SetParent(snowShovel_timerOffsetObj.transform);
			snowShovel.timer.color = Color.white;
			snowShovel.timer.gameObject.SetActive(false);

			// ******** MTM101 machine ********

			var mtm101 = ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromFile(GetRoomAsset("SnowyPlayground", "mtm101.png"), Vector2.one * 0.5f, 23f)).AddSpriteHolder(out var mtm101renderer, 5.5f, LayerStorage.iClickableLayer);
			mtm101.name = "MysteryTresentMaker";
			mtm101renderer.name = "MysteryTresentMakerRenderer";
			mtm101.gameObject.AddBoxCollider(Vector3.up * 5f, new(3.5f, 10f, 3.5f), true);
			mtm101.gameObject.AddObjectToEditor();

			var mtm = mtm101.gameObject.AddComponent<MysteryTresentMaker>();
			mtm.audMan = mtm101.gameObject.CreatePropagatedAudioManager(66f, 100f);
			mtm.audInsert = GenericExtensions.FindResourceObject<MathMachine>().audWin;

			var treSprites = TextureExtensions.LoadSpriteSheet(4, 3, 17f, GetRoomAsset("SnowyPlayground", "tresentSheet.png"));

			mtm.tresentPre = ObjectCreationExtensions.CreateSpriteBillboard(treSprites[0]).AddSpriteHolder(out var tresentRender, 1.5f, LayerStorage.standardEntities)
				.gameObject.SetAsPrefab(true).AddComponent<Tresent>();
			mtm.tresentPre.name = "Tresent";
			tresentRender.name = "TresentRenderer";
			var tresentRenderbase = tresentRender.AddSpriteHolder(out _, 1.9f).transform;
			tresentRenderbase.SetParent(mtm.tresentPre.transform);
			tresentRenderbase.localPosition = Vector3.down * 5f;

			mtm.tresentPre.renderer = tresentRender;
			mtm.tresentPre.sprsPrepareExplosion = [.. treSprites.Take(9)];
			mtm.tresentPre.sprsExploding = [.. treSprites.Skip(9).Take(3)];

			mtm.tresentPre.bonkerPre = (ITM_Hammer)man.Get<Item>("times_itemPrefab_Hammer");

			mtm.tresentPre.audMan = mtm.tresentPre.gameObject.CreatePropagatedAudioManager(35f, 65f);
			mtm.tresentPre.audThrow = man.Get<SoundObject>("audGenericThrow");
			mtm.tresentPre.audExplode = man.Get<SoundObject>("audPop");

			mtm.tresentPre.audPrepExplode = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("SnowyPlayground", "tresent_prepBreak.wav")), string.Empty, SoundType.Effect, Color.white);
			mtm.tresentPre.audPrepExplode.subtitle = false;

			mtm.tresentPre.text = Object.Instantiate(man.Get<TextMeshPro>("genericTextMesh"), mtm.tresentPre.transform);
			mtm.tresentPre.text.name = "TresentText";

			mtm.tresentPre.text.color = Color.white;
			mtm.tresentPre.text.fontSize = 7f;
			mtm.tresentPre.text.gameObject.SetActive(false);
			

			mtm.tresentPre.entity = mtm.tresentPre.gameObject.CreateEntity(1f, rendererBase: tresentRenderbase);

			// Tresent particles
			mtm.tresentPre.confettiParts = new GameObject("TresentConfetti").AddComponent<ParticleSystem>();
			mtm.tresentPre.confettiParts.transform.SetParent(tresentRenderbase);
			mtm.tresentPre.confettiParts.transform.localPosition = Vector3.zero;
			mtm.tresentPre.confettiParts.GetComponent<ParticleSystemRenderer>().material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = AssetLoader.TextureFromFile(GetRoomAsset("SnowyPlayground", "numberFettis.png")) };

			main = mtm.tresentPre.confettiParts.main;
			main.gravityModifierMultiplier = 0.35f;
			main.startLifetimeMultiplier = 12f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = new(0.8f, 1.5f);

			emission = mtm.tresentPre.confettiParts.emission;
			emission.enabled = false;

			vel = mtm.tresentPre.confettiParts.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.Local;
			vel.x = new(-12f, 12f);
			vel.y = new(6f, 14f);
			vel.z = new(-12f, 12f);

			var an = mtm.tresentPre.confettiParts.textureSheetAnimation;
			an.enabled = true;
			an.numTilesX = 4;
			an.numTilesY = 4;
			an.animation = ParticleSystemAnimationType.WholeSheet;
			an.fps = 0f;
			an.timeMode = ParticleSystemAnimationTimeMode.FPS;
			an.cycleCount = 1;
			an.startFrame = new(0, 15); // 2x2

			var col = mtm.tresentPre.confettiParts.collision;
			col.enabled = true;
			col.type = ParticleSystemCollisionType.World;
			col.enableDynamicColliders = false;

			// ======================== Focus Room ===========================
			var studentSprs = TextureExtensions.LoadSpriteSheet(3, 1, 25f, GetRoomAsset("FocusRoom", "focusStd.png"));
			var student = ObjectCreationExtensions.CreateSpriteBillboard(studentSprs[0]);
			student.name = "FocusedStudent";
			student.gameObject.AddObjectToEditor();

			var focusedStudent = student.gameObject.AddComponent<FocusedStudent>();
			focusedStudent.audMan = student.gameObject.CreatePropagatedAudioManager(95f, 125f);
			focusedStudent.audAskSilence = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("FocusRoom", "Student_Please.wav")), "Vfx_FocusStd_Disturbed1", SoundType.Voice, new(0f, 0.65f, 0f));
			focusedStudent.audAskSilence2 = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("FocusRoom", "Student_Please2.wav")), "Vfx_FocusStd_Disturbed2", SoundType.Voice, new(0f, 0.65f, 0f));
			focusedStudent.audDisturbed = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("FocusRoom", "Student_scream.wav")), "Vfx_FocusStd_Scream1", SoundType.Voice, new(0f, 0.65f, 0f));
			focusedStudent.audDisturbed.additionalKeys = [new() { key = "Vfx_FocusStd_Scream2", time = 1.209f }];

			focusedStudent.renderer = student;
			focusedStudent.sprSpeaking = studentSprs[1];
			focusedStudent.sprScreaming = studentSprs[2];
			focusedStudent.sprNormal = student.sprite;
			// ======================== Art Room ============================
			var vaseSprs = TextureExtensions.LoadSpriteSheet(2, 1, 17f, GetRoomAsset("ExibitionRoom", "Vase.png"));
			var vase = ObjectCreationExtensions.CreateSpriteBillboard(vaseSprs[0]).AddSpriteHolder(out var vaseRenderer, 0f, LayerStorage.ignoreRaycast);
			vase.gameObject.AddBoxCollider(Vector3.zero, new(4.5f, 5f, 4.5f), true);
			vase.gameObject.AddNavObstacle(new(6.5f, 5f, 6.5f));
			vase.name = "SensitiveVase";
			vaseRenderer.name = "VaseSprite";
			vase.gameObject.AddObjectToEditor();

			var vaseObj = vase.gameObject.AddComponent<Vase>();
			vaseObj.audBreak = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(GetRoomAsset("ExibitionRoom", "break.wav")), "Vfx_Vase_Break", SoundType.Voice, Color.white);
			vaseObj.audMan = vase.gameObject.CreatePropagatedAudioManager(50f, 85f);
			vaseObj.renderer = vaseRenderer;
			vaseObj.sprBroken = vaseSprs[1];

			// ================================================ Misc Assets to Load =======================================================
			// Corner Lamps
			List<WeightedTransform> transformsList = [
				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("lamp.png"))), 25f))
				.AddSpriteHolder(out _, 2.9f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform, weight = 75 },

				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("lightBulb.png"))), 65f))
				.AddSpriteHolder(out _, 5.1f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform, weight = 35 },

				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("lampShaped.png"))), 25f))
				.AddSpriteHolder(out _, 3.1f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform, weight = 55 },
				];


			TextureExtensions.LoadSpriteSheet(3, 1, 40f, MiscPath, TextureFolder, GetAssetName("SugaLamps.png")).Do(x =>
			{
				transformsList.Add(new()
				{
					selection = ObjectCreationExtensions.CreateSpriteBillboard(x)
				.AddSpriteHolder(out _, 3.1f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform,
					weight = 38
				});
			});

			for (int i = 0; i < transformsList.Count; i++)
			{
				transformsList[i].selection.name = "TimesGenericCornerLamp_" + (i + 1);
				transformsList[i].selection.gameObject.AddObjectToEditor();
			}

			man.Add("prefabs_cornerLamps", transformsList);

			// ================================================ Modded Room Generator Application ==========================================
			// Bathrooms
			var sets = RegisterRoom("Bathroom", new(0.85f, 0.85f, 0.85f, 1f),
				ObjectCreators.CreateDoorDataObject("BathDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "bathDoorOpened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "bathDoorClosed.png"))));

			Superintendent.AddAllowedRoom(sets.category);


			var room = GetAllAssets(GetRoomAsset("Bathroom"), 45, 25, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "MapBG_Bathroom.png")));
			var fun = room[0].selection.AddRoomFunctionToContainer<PosterAsideFromObject>();
			fun.targetPrefabName = "sink";
			fun.posterPre = ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Bathroom", "mirror.png"))]);

			var cover = room[0].selection.AddRoomFunctionToContainer<CoverRoomFunction>();
			cover.hardCover = true;
			cover.coverage = CellCoverage.North | CellCoverage.East | CellCoverage.West | CellCoverage.South;

			room.ForEach(x =>
			{
				x.selection.posterChance = 0f;
				x.selection.lightPre = bathLightPre;
			});
			sets.container = room[0].selection.roomFunctionContainer;

			var group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 0,
				maxRooms = 1,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(0)],
				name = "Bathroom",
				light = [new() { selection = bathLightPre }],
			};

			floorDatas[0].RoomAssets.Add(group);

			group = new RoomGroup()
			{
				stickToHallChance = 0.7f,
				minRooms = 1,
				maxRooms = 1,
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
				minRooms = 1,
				maxRooms = 3,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(2)],
				name = "Bathroom",
				light = [new() { selection = bathLightPre }]
			};
			floorDatas[2].RoomAssets.Add(group);

			// Abandoned Room
			sets = RegisterRoom("AbandonedRoom", new(0.59765625f, 0.19921875f, 0f),
				ObjectCreators.CreateDoorDataObject("OldDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "oldDoorOpen.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "oldDoorClosed.png"))));

			room = GetAllAssets(GetRoomAsset("AbandonedRoom"), 250, 45, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "MapBG_AbandonedRoom.png")));
			room[0].selection.AddRoomFunctionToContainer<ShowItemsInTheEnd>();
			room[0].selection.AddRoomFunctionToContainer<RandomPosterFunction>().posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("AbandonedRoom", "abandonedRoomTut.png"))])];
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

			AddCategoryForNPCToSpawn(sets.category, typeof(TickTock), typeof(Phawillow));

			// Computer Room

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
				minRooms = 0,
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
				minRooms = 2,
				maxRooms = 2,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(3)],
				name = "ComputerRoom",
				light = [lightPre]
			};

			floorDatas[3].RoomAssets.Add(group);

			group = new RoomGroup()
			{
				stickToHallChance = 1f,
				minRooms = 2,
				maxRooms = 4,
				potentialRooms = [.. room.FilterRoomAssetsByFloor(2)],
				name = "ComputerRoom",
				light = [lightPre]
			};

			floorDatas[2].RoomAssets.Add(group);

			// Dribble Room

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

			// Sweep's Closet
			var sweepCloset = GenericExtensions.FindResourceObject<GottaSweep>().potentialRoomAssets[0].selection;

			room = GetAllAssets(GetRoomAsset("Closet"), sweepCloset.maxItemValue, 100, sweepCloset.roomFunctionContainer);
			room[0].selection.AddRoomFunctionToContainer<HighCeilingRoomFunction>().ceilingHeight = 1;
			room[0].selection.AddRoomFunctionToContainer<RandomPosterFunction>().posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Closet", "sweepSad.png"))])];

			room.ForEach(x =>
			{
				x.selection.posters = sweepCloset.posters;
				x.selection.posterChance = sweepCloset.posterChance;
				x.selection.windowChance = sweepCloset.windowChance;
				x.selection.windowObject = sweepCloset.windowObject;
				x.selection.ceilTex = sweepCloset.ceilTex;
				x.selection.wallTex = sweepCloset.wallTex;
				x.selection.florTex = sweepCloset.florTex;
				x.selection.lightPre = sweepCloset.lightPre;
			});

			AddAssetsToNpc<GottaSweep>(room);
			AddAssetsToNpc<ZeroPrize>([new() { selection = sweepCloset, weight = 100 }, .. room]);
			AddAssetsToNpc<CoolMop>([new() { selection = sweepCloset, weight = 100 }, .. room]);
			AddAssetsToNpc<Mopliss>([new() { selection = sweepCloset, weight = 100 }, .. room]);
			AddAssetsToNpc<VacuumCleaner>([new() { selection = sweepCloset, weight = 100 }, .. room]);

			// Dr Reflex Office
			sweepCloset = GenericExtensions.FindResourceObject<DrReflex>().potentialRoomAssets[0].selection;
			room = GetAllAssets(GetRoomAsset("Reflex"), sweepCloset.maxItemValue, 100, sweepCloset.roomFunctionContainer);

			room.ForEach(x =>
			{
				x.selection.posters = sweepCloset.posters;
				x.selection.posterChance = sweepCloset.posterChance;
				x.selection.windowChance = sweepCloset.windowChance;
				x.selection.windowObject = sweepCloset.windowObject;
				x.selection.ceilTex = sweepCloset.ceilTex;
				x.selection.wallTex = sweepCloset.wallTex;
				x.selection.florTex = sweepCloset.florTex;
				x.selection.lightPre = sweepCloset.lightPre;
			});

			AddAssetsToNpc<DrReflex>(room);

			// Kitchen
			sets = RegisterRoom("Kitchen", new(0.59765625f, 0.796875f, 0.99609375f, 1f),
				ObjectCreators.CreateDoorDataObject("KitchenDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "kitchenDoorOpened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "kitchenDoorClosed.png"))));


			room = GetAllAssets(GetRoomAsset("Kitchen"), 75, 35, mapBg: AssetLoader.TextureFromFile(GetRoomAsset("Kitchen", "MapBG_Kitchen.png")));
			var lunchObj = GenericExtensions.FindResourceObjectByName<RendererContainer>("Decor_Lunch").transform;
			BasicObjectSwapData[] swaps = [
				new()
				{
					chance = 0.75f,
					potentialReplacements = [new() { selection = ModifyExistingBillboard(DecorsPlugin.Get<GameObject>("editorPrefab_SaltAndHot"), Vector3.up * 1.2f).transform, weight = 100 }],
					prefabToSwap = lunchObj
				},
				new()
				{
					chance = 0.1f,
					potentialReplacements = [new() { selection = man.Get<GameObject>("editorPrefab_SecretBread").transform, weight = 25 }, new() { selection = man.Get<GameObject>("editorPrefab_TimesKitchenSteak").transform, weight = 100 }],
					prefabToSwap = lunchObj
				}
				];

			room.ForEach(x => x.selection.basicSwaps.AddRange(swaps));

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

			// Super mystery room
			sets = RegisterRoom("SuperMystery", new(1f, 0.439f, 0f),
				ObjectCreators.CreateDoorDataObject("SuperMysteryDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("SuperMystery", "FakeMysteryRoom_Open.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("SuperMystery", "FakeMysteryRoom.png"))));

			Superintendent.AddAllowedRoom(sets.category);

			room = GetAllAssets(GetRoomAsset("SuperMystery"), 75, 50, secretRoom: true);
			room[0].selection.AddRoomFunctionToContainer<RuleFreeZone>();

			var exclamations = TextureExtensions.LoadSpriteSheet(4, 2, 25f, GetRoomAsset("SuperMystery", "exclamations.png"));
			var transforms = new Transform[exclamations.Length];
			for (int i = 0; i < transforms.Length; i++)
			{
				transforms[i] = ObjectCreationExtensions.CreateSpriteBillboard(exclamations[i]).transform;
				transforms[i].gameObject.ConvertToPrefab(true);
			}

			room[0].selection.AddRoomFunctionToContainer<EnvironmentObjectSpawner>().randomTransforms = transforms;

			sets.container = room[0].selection.roomFunctionContainer;

			room.ForEach(x => { x.selection.maxItemValue = 999; x.selection.posterChance = 0; x.selection.MysteryRoomCover(); });

			AddAssetsToEvent<SuperMysteryRoom>(room);

			// ================================================ Modded Special Room Creation ====================================================
			// GYM
			sets = RegisterSpecialRoom("BasketballArea", Color.cyan);

			room = GetAllAssets(GetRoomAsset("BasketballArea"), 2, 55, mapBg: BooleanStorage.HasCrispyPlus ? AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "mapIcon_basket.png")) : null, squaredShape: true);
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
			room[0].selection.AddRoomFunctionToContainer<RandomPosterFunction>().posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("BasketballArea", "basketAreaWarning.png"))])];

			sets.container = room[0].selection.roomFunctionContainer;


			floorDatas[1].SpecialRooms.AddRange(room);
			floorDatas[3].SpecialRooms.AddRange(room);
			floorDatas[2].SpecialRooms.AddRange(room.ConvertAssetWeights(55));

			// Forest Area
			sets = RegisterSpecialRoom("Forest", Color.cyan);

			room = GetAllAssets(GetRoomAsset("Forest"), 75, 1, mapBg: BooleanStorage.HasCrispyPlus ? AssetLoader.TextureFromFile(GetRoomAsset("Forest", "mapIcon_trees.png")) : null, squaredShape: true);
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
			room[0].selection.AddRoomFunctionToContainer<CoverRoomFunction>().coverage = (CellCoverage)~0; // Reverse of 0

			var highCeil = room[0].selection.AddRoomFunctionToContainer<HighCeilingRoomFunction>(); // The first is from the playground
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

			// SNOWY PLAYGROUND

			var playgroundRoomRef = GenericExtensions.FindResourceObjects<RoomAsset>().First(x => x.name.StartsWith("Playground"));
			var playgroundClonedRoomContainer = Object.Instantiate(playgroundRoomRef.roomFunctionContainer);
			playgroundClonedRoomContainer.name = "SnowPlayground_Container";
			playgroundClonedRoomContainer.gameObject.ConvertToPrefab(true);

			var snowFunc = playgroundClonedRoomContainer.gameObject.AddComponent<FallingParticlesFunction>();

			snowFunc.particleTexture = AssetLoader.TextureFromFile(GetRoomAsset("SnowyPlayground", "snowFlake.png"));

			playgroundClonedRoomContainer.AddFunction(snowFunc);

			var objCornerSpawn = playgroundClonedRoomContainer.gameObject.AddComponent<CornerObjectSpawner>();
			objCornerSpawn.randomObjs = [new() { selection = mtm.transform, weight = 100 }];

			playgroundClonedRoomContainer.AddFunction(objCornerSpawn);

			var slipFunc = playgroundClonedRoomContainer.gameObject.AddComponent<SlipperyMaterialFunction>();
			slipFunc.slipMatPre = BBTimesManager.man.Get<SlippingMaterial>("SlipperyMatPrefab").SafeDuplicatePrefab(true);
			((SpriteRenderer)slipFunc.slipMatPre.GetComponent<RendererContainer>().renderers[0]).sprite = AssetLoader.SpriteFromFile(GetRoomAsset("SnowyPlayground", "icePatch.png"), Vector2.one * 0.5f, 22f);
			slipFunc.slipMatPre.force = 65f;
			slipFunc.slipMatPre.antiForceReduceFactor = 0.75f;
			slipFunc.slipMatPre.name = "SnowyIcePatch";

			slipFunc.minMax = new(3, 5);

			playgroundClonedRoomContainer.AddFunction(slipFunc);

			sets = RegisterSpecialRoom("SnowyPlayground", Color.cyan);

			int snowyWeight = BooleanStorage.IsChristmas ? 165 : 75;

			room = GetAllAssets(GetRoomAsset("SnowyPlayground"), snowyWeight, 1, cont: playgroundClonedRoomContainer, mapBg: BooleanStorage.HasCrispyPlus ? AssetLoader.TextureFromFile(GetRoomAsset("SnowyPlayground", "mapIcon_snow.png")) : null, squaredShape: true);
			floorTex = AssetLoader.TextureFromFile(GetRoomAsset("SnowyPlayground", "snowyPlaygroundFloor.png"));
			AddTextureToEditor("snowyPlaygroundFloor", floorTex);

			room.ForEach(x =>
			{
				x.selection.keepTextures = true;
				x.selection.florTex = floorTex;
				x.selection.wallTex = playgroundRoomRef.wallTex;
				x.selection.ceilTex = playgroundRoomRef.ceilTex;
			});

			sets.container = playgroundClonedRoomContainer;

			floorDatas[0].SpecialRooms.AddRange(room);
			floorDatas[1].SpecialRooms.AddRange(room.ConvertAssetWeights(Mathf.FloorToInt(snowyWeight * 0.85f)));
			floorDatas[3].SpecialRooms.AddRange(room);
			floorDatas[2].SpecialRooms.AddRange(room.ConvertAssetWeights(Mathf.FloorToInt(snowyWeight * 0.65f)));


			// ================================================ Base Game Room Variants ====================================================



			//Classrooms

			WeightedRoomAsset classWeightPre = FindRoomGroupOfName("Class");
			
			room = GetAllAssets(GetRoomAsset("Class"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer);
			if (!plug.enableBigRooms.Value)
				RemoveBigRooms(room, 6.56f);

			room.ForEach(x =>
			{
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
				x.selection.keepTextures = false;
				x.selection.basicSwaps = classWeightPre.selection.basicSwaps;
			});

			floorDatas[0].Classrooms.AddRange(room.Where(x => x.selection.activity.prefab.GetType() == typeof(NoActivity)).ToList().FilterRoomAssetsByFloor()); // why not "is NoActivity"? Well, probably because the game doesn't have the right NET to work like that in runtime
			var activityRooms = room.Where(x => x.selection.activity.prefab.GetType() != typeof(NoActivity)).ToList().FilterRoomAssetsByFloor();
			for (int i = 1; i < floorDatas.Count; i++)
				floorDatas[i].Classrooms.AddRange(activityRooms);


			// ****** Focus Room (A classroom variant, but with a new npc) ******
			sets = RegisterRoom("FocusRoom", new(0f, 1f, 0.5f),
				ObjectCreators.CreateDoorDataObject("FocusRoomDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "Focus_Room_Door_Opened.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("FocusRoom", "Focus_Room_Door_Closed.png"))));

			Superintendent.AddAllowedRoom(sets.category);

			room = GetAllAssets(GetRoomAsset("FocusRoom"), classWeightPre.selection.maxItemValue, classWeightPre.weight / 2, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: false);
			// Workaround to not have to edit every focus room layout lol
			var redCouchprefab = Resources.FindObjectsOfTypeAll<RendererContainer>().First(x => x.name == "RedCouch");
			room.ForEach(foc => foc.selection.basicObjects.ForEach(basO => { if (basO.prefab.name == "Couch") basO.prefab = redCouchprefab.transform; }));

			RegisterFalseClass();

			// ****** Art Room *******
			sets = RegisterRoom("ExibitionRoom", new(0.54296875f, 0.18359375f, 0.95703125f),
				ObjectCreators.CreateDoorDataObject("ExibitionRoomDoor",
				AssetLoader.TextureFromFile(GetRoomAsset("ExibitionRoom", "ExibitClassStandard_Open.png")),
				AssetLoader.TextureFromFile(GetRoomAsset("ExibitionRoom", "ExibitClassStandard_Closed.png"))));

			Superintendent.AddAllowedRoom(sets.category);

			room = GetAllAssets(GetRoomAsset("ExibitionRoom"), classWeightPre.selection.maxItemValue, classWeightPre.weight / 2, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: false);

			RegisterFalseClass();			

			void RegisterFalseClass()
			{
				if (!plug.enableBigRooms.Value)
					RemoveBigRooms(room, 6.655f);

				room.ForEach(x =>
				{
					x.selection.name.Replace("Class", sets.category.ToString()); // lol
					x.selection.doorMats = sets.doorMat;
					x.selection.category = sets.category;
					x.selection.color = sets.color;
					x.selection.wallTex = classWeightPre.selection.wallTex;
					x.selection.florTex = classWeightPre.selection.florTex;
					x.selection.ceilTex = classWeightPre.selection.ceilTex;
					x.selection.posters = classWeightPre.selection.posters;
					x.selection.posterChance = classWeightPre.selection.posterChance;
					x.selection.windowChance = classWeightPre.selection.windowChance;
					x.selection.windowObject = classWeightPre.selection.windowObject;
					x.selection.mapMaterial = classWeightPre.selection.mapMaterial;
					x.selection.lightPre = classWeightPre.selection.lightPre;
					x.selection.basicSwaps = classWeightPre.selection.basicSwaps;
				});

				for (int i = 1; i < floorDatas.Count; i++)
					floorDatas[i].Classrooms.AddRange(room);
			}

			//Faculties

			classWeightPre = FindRoomGroupOfName("Faculty");
			room = GetAllAssets(GetRoomAsset("Faculty"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: false);

			room.ForEach(x =>
			{
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
				x.selection.basicSwaps = classWeightPre.selection.basicSwaps;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Faculties.AddRange(room.FilterRoomAssetsByFloor(i));


			//Offices
			classWeightPre = FindRoomGroupOfName("Office");
			room = GetAllAssets(GetRoomAsset("Office"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: false);
			if (!plug.enableBigRooms.Value)
				RemoveBigRooms(room, 7.65f);

			var globePrefab = GenericExtensions.FindResourceObjectByName<RendererContainer>("Decor_Globe").transform;

			classWeightPre.selection.basicSwaps.Add(new()
			{
				chance = 0.75f,
				potentialReplacements = [
							new() { selection = DecorsPlugin.Get<GameObject>("editorPrefab_SmallPottedPlant").transform },
							new() { selection = DecorsPlugin.Get<GameObject>("editorPrefab_TableLightLamp").transform },
							new() { selection = DecorsPlugin.Get<GameObject>("editorPrefab_FancyOfficeLamp").transform },
							new() { selection = DecorsPlugin.Get<GameObject>("editorPrefab_TheRulesBook").transform }
							],
				prefabToSwap = globePrefab
			});

			room.ForEach(x =>
			{
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
				x.selection.basicSwaps = classWeightPre.selection.basicSwaps;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Offices.AddRange(room.FilterRoomAssetsByFloor(i));

			// Hall (PREV HALL DOESN'T EXIST ANYMORE CURRENTLY)
			//classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialPrePlotSpecialHalls.Length != 0).potentialPrePlotSpecialHalls[0];

			//room = GetAllAssets(GetRoomAsset("PrevHalls"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: true, isAHallway: true);

			//room.ForEach(x =>
			//{
			//	x.selection.posters = classWeightPre.selection.posters;
			//	x.selection.posterChance = classWeightPre.selection.posterChance;
			//	x.selection.windowChance = classWeightPre.selection.windowChance;
			//	x.selection.windowObject = classWeightPre.selection.windowObject;
			//	x.selection.lightPre = classWeightPre.selection.lightPre;
			//});

			//for (int i = 0; i < floorDatas.Count; i++)
			//	floorDatas[i].Halls.AddRange(room.FilterRoomAssetsByFloor(i).ConvertAll<KeyValuePair<WeightedRoomAsset, bool>>(x => new(x, false))); // Pre halls

			classWeightPre = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.potentialPostPlotSpecialHalls.Length != 0).potentialPostPlotSpecialHalls[0];

			room = GetAllAssets(GetRoomAsset("AfterHalls"), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: false, isAHallway: true);

			room.ForEach(x =>
			{
				x.selection.posters = classWeightPre.selection.posters;
				x.selection.posterChance = classWeightPre.selection.posterChance;
				x.selection.windowChance = classWeightPre.selection.windowChance;
				x.selection.windowObject = classWeightPre.selection.windowObject;
				x.selection.lightPre = classWeightPre.selection.lightPre;
			});

			for (int i = 0; i < floorDatas.Count; i++)
				floorDatas[i].Halls.AddRange(room.FilterRoomAssetsByFloor(i).ConvertAll<KeyValuePair<WeightedRoomAsset, bool>>(x => new(x, true)));

			//================================ Special Rooms ========================================

			AddSpecialRoomCollectionWithName("Cafeteria");
			AddSpecialRoomCollectionWithName("Playground");
			AddSpecialRoomCollectionWithName("Library");

			static void AddSpecialRoomCollectionWithName(string name)
			{
				WeightedRoomAsset classWeightPre = null;
				foreach (var ld in Resources.FindObjectsOfTypeAll<LevelObject>())
				{
					var r = ld.potentialSpecialRooms.FirstOrDefault(x => x.selection.roomFunctionContainer.name.StartsWith(name));
					if (r != null)
					{
						classWeightPre = r;
						break;
					}
				}
				if (classWeightPre == null)
					throw new System.ArgumentException("Could not find a special room instance with " + name);

				var room = GetAllAssets(GetRoomAsset(name), classWeightPre.selection.maxItemValue, classWeightPre.weight, classWeightPre.selection.offLimits, classWeightPre.selection.roomFunctionContainer, keepTextures: classWeightPre.selection.keepTextures, squaredShape: true);

				room.ForEach(x =>
				{
					x.selection.posters = classWeightPre.selection.posters;
					x.selection.posterChance = classWeightPre.selection.posterChance;
					x.selection.windowChance = classWeightPre.selection.windowChance;
					x.selection.windowObject = classWeightPre.selection.windowObject;
					x.selection.lightPre = classWeightPre.selection.lightPre;
					x.selection.basicSwaps = classWeightPre.selection.basicSwaps;
				});
				for (int i = 0; i < floorDatas.Count; i++)
					floorDatas[i].SpecialRooms.AddRange(room.FilterRoomAssetsByFloor(i));


			}

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

			static List<E> AddAssetsToEvent<E>(List<WeightedRoomAsset> assets) where E : RandomEvent
			{
				var l = new List<E>();
				foreach (var x in RandomEventMetaStorage.Instance.All())
				{
					if (x.value is E e)
					{
						l.Add(e);
						e.potentialRoomAssets = e.potentialRoomAssets.AddRangeToArray([.. assets]);
					}
				}
				return l;
			}

			static void AddCategoryForNPCToSpawn(RoomCategory cat, params System.Type[] npcs)
			{
				NPCMetaStorage.Instance.All().Do(x =>
				{
					if (!npcs.Contains(x.value.GetType()))
						return;

					foreach (var npc in x.prefabs)
						npc.Value.spawnableRooms.Add(cat);
				});
			}

			static void RemoveBigRooms(List<WeightedRoomAsset> assets, float averageGiven)
			{
				for (int i = 0; i < assets.Count; i++)
				{
					if (assets[i].selection.GetRoomSize().Magnitude() >= averageGiven) // Between 6x6 and 7x7
					{
						Object.Destroy(assets[i].selection); // Remove the asset since it's never going to be used anyways (free up memory)
						assets.RemoveAt(i--);
					}
				}
			}

			static WeightedRoomAsset FindRoomGroupOfName(string name)
			{
				foreach (var lvl in Resources.FindObjectsOfTypeAll<LevelObject>())
				{
					var lvlGroup = lvl.roomGroup.FirstOrDefault(x => x.name == name);
					if (lvlGroup != null)
						return lvlGroup.potentialRooms[0];
					
				}
				return null;
			}

			GameObject SetupObjCollisionAndScale(GameObject obj, Vector3 navMeshSize, float newScale, bool automaticallyContainer = true, bool addMeshCollider = true)
			{
				obj.transform.localScale = Vector3.one;
				if (navMeshSize != default)
					obj.gameObject.AddNavObstacle(navMeshSize);

				var childRef = new GameObject(obj.name + "_Renderer");
				childRef.transform.SetParent(obj.transform);
				childRef.transform.localPosition = Vector3.zero;

				var childs = obj.transform.AllChilds();
				childs.ForEach(c =>
				{
					if (c == childRef.transform)
						return;
					c.SetParent(childRef.transform);
					c.transform.localPosition = Vector3.zero;
					c.transform.localScale = Vector3.one * newScale;
					if (addMeshCollider)
						c.gameObject.AddComponent<MeshCollider>();
				});

				if (automaticallyContainer)
					obj.AddContainer(obj.GetComponentsInChildren<MeshRenderer>());


				return obj;
			}

			RendererContainer ModifyExistingBillboard(GameObject billboard, Vector3 newOffset)
			{
				var clone = billboard.GetComponent<RendererContainer>().DuplicatePrefab();
				clone.name = "Times_Modified_" + billboard.name;
				foreach (var renderer in clone.renderers)
					renderer.transform.localPosition = newOffset;
				return clone;
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

		//static RoomSettings RegisterRoom(string roomName, RoomCategory en, Color color, StandardDoorMats mat)
		//{
		//	var settings = new RoomSettings(en, RoomType.Room, color, mat);
		//	PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
		//	return settings;
		//}

		static RoomSettings RegisterSpecialRoom(string roomName, Color color)
		{
			var settings = new RoomSettings(RoomCategory.Special, RoomType.Room, color, man.Get<StandardDoorMats>("ClassDoorSet"));
			PlusLevelLoaderPlugin.Instance.roomSettings.Add(roomName, settings);
			return settings;
		}

		static List<WeightedRoomAsset> GetAllAssets(string path, int maxValue, int assetWeight, bool isOffLimits = false, RoomFunctionContainer cont = null, bool isAHallway = false, bool secretRoom = false, Texture2D mapBg = null, bool keepTextures = true, bool squaredShape = false)
		{
			List<WeightedRoomAsset> assets = [];
			RoomFunctionContainer container = cont;
			foreach (var file in Directory.GetFiles(path))
			{
				if (File.ReadAllBytes(file).Length == 0) continue; // if the cbld file is empty, it means it has been "removed". This is to make sure that anyone who extracts newer versions don't include these layouts.
				try
				{
					var asset = RoomFactory.CreateAssetsFromPath(file, maxValue, isOffLimits, container, isAHallway, secretRoom, mapBg, keepTextures, squaredShape);
					assets.AddRange(asset.ConvertAll(x => new WeightedRoomAsset() { selection = x, weight = assetWeight }));
					_moddedAssets.AddRange(asset);
					if (!container)
						container = asset[0].roomFunctionContainer;

					for (int i = 0; i < asset.Count; i++)
						RoomAssetMetaStorage.Instance.Add(new RoomAssetMeta(plug.Info, asset[i]));
				}
				catch (KeyNotFoundException e)
				{
					Debug.LogWarning("------------- Warning: actual exception found during room loading --------------");
					Debug.LogWarning("Current path: " + file);
					Debug.LogException(e);
					//using (BinaryReader reader = new(File.OpenRead(file)))
					//{
					//	var asset = LevelExtensions.ReadLevel(reader);
					//	Debug.Log("Prefabs:");
					//	asset.rooms[1].prefabs.ForEach(x => Debug.Log(x.prefab));
					//	Debug.Log("---");
					//	Debug.Log(asset.rooms[1].type);
					//	Debug.Log(asset.rooms[1].textures.floor + "," + asset.rooms[1].textures.wall + "," + asset.rooms[1].textures.ceiling);
					//}
				}
				catch { }

			}

			return assets;
		}
		static List<WeightedRoomAsset> FilterRoomAssetsByFloor(this List<WeightedRoomAsset> assets) =>
			assets.FilterRoomAssetsByFloor(-1);
		static List<WeightedRoomAsset> FilterRoomAssetsByFloor(this List<WeightedRoomAsset> assets, int floor)
		{
			var newAss = new List<WeightedRoomAsset>(assets);
			System.Exception e = null;
			try
			{
				for (int i = 0; i < newAss.Count; i++)
				{

					string[] rawData = newAss[i].selection.name.Split('!');

					if (rawData.Length > 2)
					{
						if (int.TryParse(rawData[2], out int val))
						{
							newAss[i].selection.maxItemValue = val;
							if (val >= 200)
								newAss[i].weight = 10; // Weight of a standard rare faculty
						}
						if (rawData.Length > 3)
						{
							if (int.TryParse(rawData[3], out val))
								newAss[i].weight = val;
						}
					}

					if (floor >= 0)
					{
						string[] sdata = rawData[1].Split(',');
						int[] data = new int[sdata.Length];

						for (int z = 0; z < sdata.Length; z++) // Converts the string numbers into integers
							if (int.TryParse(sdata[z], out int val))
								data[z] = val;

						if (!data.Contains(floor)) // Finally remove the asset if it isn't for the intended floor
						{
							newAss.RemoveAt(i--);
							if (newAss.Count == 0)
								break;
						}
					}
				}
			}
			catch (System.IndexOutOfRangeException ex)
			{
				Debug.LogError("Failed to get data from the room collection due to invalid format in the data!");
				e = ex;
			}
			catch (System.ArgumentOutOfRangeException ex)
			{
				Debug.LogError("Failed to get data from the room collection due to invalid format in the data!");
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
