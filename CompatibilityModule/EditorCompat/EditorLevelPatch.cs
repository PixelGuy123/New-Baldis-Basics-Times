using BaldiLevelEditor;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PlusLevelFormat;
using PlusLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BBTimes.CompatibilityModule.EditorCompat
{
	[HarmonyPatch]
	[ConditionalPatchMod("mtm101.rulerp.baldiplus.leveleditor")]
	internal static class EditorLevelPatch
	{
		[HarmonyPatch(typeof(BasePlugin), "PostSetup")]
		[HarmonyPostfix]
		private static void MakeEditorSeeAssets(AssetManager man)
		{
			markersToAdd = [];
			itemsToAdd = [];
			npcsToAdd = [];

			GameObject[] array = [
				man.Get<GameObject>("editorPrefab_bathStall"),
				man.Get<GameObject>("editorPrefab_bathDoor"),
				man.Get<GameObject>("editorPrefab_sink")
			];
			MarkRotatingObject(array[0], Vector3.up * array[0].transform.localScale.y / 2f);
			MarkRotatingObject(array[1], Vector3.up * array[1].transform.localScale.y / 2f);
			MarkObject(array[2], Vector3.zero);
			MarkObjectRow("fullStall", [
				new ObjectData(array[0], new Vector3(-5f, 5f, 0f), Quaternion.Euler(0f, 90f, 0f)),
				new ObjectData(array[1], new Vector3(0f, 5f, 4f), default),
				new ObjectData(array[0], new Vector3(5f, 5f, 0f), Quaternion.Euler(0f, 90f, 0f))
			]);

			array = [
				man.Get<GameObject>("editorPrefab_BasketHoop"),
				man.Get<GameObject>("editorPrefab_BasketballPile"),
				man.Get<GameObject>("editorPrefab_GrandStand"),
				man.Get<GameObject>("editorPrefab_BasketMachine"),
				man.Get<GameObject>("editorPrefab_BasketBallBigLine")];

			MarkRotatingObject(array[0], Vector3.zero);
			MarkObject(array[1], Vector3.up * 2f);
			MarkRotatingObject(array[2], Vector3.up * (array[2].transform.localScale.y * 0.5f));
			MarkRotatingObject(array[3], Vector3.zero);
			MarkRotatingObject(array[4], Vector3.zero);

			MarkRotatingObject(man.Get<GameObject>("editorPrefab_FancyComputerTable"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_ComputerBillboard"), Vector3.up * 5.5f);

			MarkRotatingObject(man.Get<GameObject>("editorPrefab_StraightRunLine"), Vector3.zero);
			MarkRotatingObject(man.Get<GameObject>("editorPrefab_CurvedRunLine"), Vector3.zero);

			MarkObject(man.Get<GameObject>("editorPrefab_Foresttree"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_Campfire"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_Beartrap"), Vector3.zero);

			array = [man.Get<GameObject>("editorPrefab_ClosetShelf")];
			MarkRotatingObject(array[0], Vector3.zero);
			MarkObjectRow("highShelf",
			[
				new ObjectData(array[0], Vector3.zero, default),
				new ObjectData(array[0], Vector3.up * 4.5f, default),
				new ObjectData(array[0], Vector3.up * 9f, default)
			]);

			MarkObject(man.Get<GameObject>("editorPrefab_KitchenCabinet"), Vector3.up);
			MarkRotatingObject(man.Get<GameObject>("editorPrefab_JoeChef"), Vector3.up * 5f);
			MarkObject(man.Get<GameObject>("editorPrefab_FocusedStudent"), Vector3.up * 5f);

			// ************************ Items ****************************

			AddItem("basketball", "Basketball");
			AddItem("bell", "Bell");
			AddItem("blowDrier", "BlowDrier");
			AddItem("beehive", "Beehive");
			AddItem("BSED", "BSED");
			AddItem("cherryBsoda", "CherryBsoda");
			AddItem("chocolate", "HotChocolate");
			AddItem("comicallyLargeTrumpet", "ComicallyLargeTrumpet");
			AddPointItem<ITM_DivideYTP>("divisionPoint");
			AddItem("empty", "EmptyWaterBottle");
			AddItem("fidgetSpinner", "FidgetSpinner");
			AddItem("gps", "Gps");
			AddItem("gQuarter", "GoldenQuarter");
			AddItem("gsoda", "GSoda");
			AddItem("grabGun", "GrabGun");
			AddItem("gum", "Gum");
			AddItem("hammer", "Hammer");
			AddItem("hardHat", "Hardhat");
			AddItem("headachePill", "Headachepill");
			AddItem("pencil", "Pencil");
			AddItem("pogostick", "Pogostick");
			AddItem("present", "Present");
			AddItem("magnet", "Magnet");
			AddItem("remote", "InvRemControl");
			AddItem("rottenCheese", "RottenCheese");
			AddItem("screwDriver", "Screwdriver");
			AddItem("soapBubbles", "SoapBubbles");
			AddItem("sugarFlavoredZestyBar", "SugarFlavorZestyBar");
			AddItem("superCamera", "SuperCamera");
			AddItem("soap", "Soap");
			AddItem("sp", "SpeedPotion");
			AddItem("trap", "Beartrap");
			AddPointItem<ITM_TimesYTP>("TimesIcon");
			AddItem("throwableTeleporter", "ThrowableTeleporter");
			AddItem("toiletPaper", "ToiletPaper");
			AddItem("water", "WaterBottle");


			// ************************ Npcs *****************************

			AddNPC("0thprize", "ZeroPrize");
			AddNPC("bubbly", "Bubbly");
			AddNPC("Camerastand", "Camerastand");
			AddNPC("crazyClock", "CrazyClock");
			AddNPCCopy<ClassicGottaSweep>("oldsweep");
			AddNPC("dribble", "Dribble");
			AddNPC("faker", "Faker");
			AddNPC("gluebotrony", "Glubotrony");
			AddNPC("leapy", "Leapy");
			AddNPC("MGS", "Magicalstudent");
			AddNPC("Mugh", "Mugh");
			AddNPC("officeChair", "OfficeChair");
			AddNPC("pencilBoy", "PencilBoy");
			AddNPC("phawillow", "Phawillow");
			AddNPC("penny", "Penny");
			AddNPC("pix", "Pix");
			AddNPC("pran", "Pran");
			AddNPC("rollBot", "Rollingbot");
			AddNPC("stunly", "Stunly");
			AddNPC("superintendent", "Superintendent");
			AddNPC("spj", "Superintendentjr");
			AddNPC("watcher", "Watcher");

			// ************* Local Methods ***************

			static void MarkRotatingObject(GameObject obj, Vector3 offset)
			{
				markersToAdd.Add(new RotateAndPlacePrefab(obj.name));
				BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>(obj.name, obj, offset, false));
			}

			static void MarkObject(GameObject obj, Vector3 offset)
			{
				markersToAdd.Add(new ObjectTool(obj.name));
				BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>(obj.name, obj, offset, false));
			}

			static void MarkObjectRow(string prebuiltToolName, ObjectData[] objs)
			{
				PrefabLocation[] array = new PrefabLocation[objs.Length];

				for (int i = 0; i < objs.Length; i++)
					array[i] = new PrefabLocation(objs[i].Item1.name, PlusLevelLoader.Extensions.ToData(objs[i].Item2), PlusLevelLoader.Extensions.ToData(objs[i].Item3));

				markersToAdd.Add(new PrebuiltStructureTool(prebuiltToolName, new EditorPrebuiltStucture(array)));
			}

			static void AddItem(string itemName, string itemEnum)
			{
				var en = EnumExtensions.GetFromExtendedName<Items>(itemEnum);
				var itm = ItemMetaStorage.Instance.FindByEnumFromMod(en, BBTimesManager.plug.Info).value;

				BaldiLevelEditorPlugin.itemObjects.Add(itemName, itm);
				PlusLevelLoaderPlugin.Instance.itemObjects.Add(itemName, itm);
				itemsToAdd.Add(new TimesItem(itemName));
			}

			static void AddPointItem<T>(string itemName) where T : Item
			{
				var itm = points.Find(x => x.item is T);
				BaldiLevelEditorPlugin.itemObjects.Add(itemName, itm);
				PlusLevelLoaderPlugin.Instance.itemObjects.Add(itemName, itm);
				itemsToAdd.Add(new TimesItem(itemName));
			}

			static void AddNPC(string npcName, string npcEnum)
			{
				var en = EnumExtensions.GetFromExtendedName<Character>(npcEnum);
				var val = NPCMetaStorage.Instance.Find(x => x.character == en && BBTimesManager.plug.Info == x.info).value;

				BaldiLevelEditorPlugin.characterObjects.Add(npcName,
					BaldiLevelEditorPlugin.StripAllScripts(val.gameObject, true)
					);

				PlusLevelLoaderPlugin.Instance.npcAliases.Add(npcName, val);

				npcsToAdd.Add(new TimesNPC(npcName));
			}

			static void AddNPCCopy<T>(string npcName) where T : MonoBehaviour
			{
				NPC npc = null;
				foreach (var meta in NPCMetaStorage.Instance.All())
				{
					var p = meta.prefabs.FirstOrDefault(x => x.Value.GetComponent<T>());
					if (p.Value)
					{
						npc = p.Value;
						break;
					}
				}

				if (npc == null)
				{
					Debug.LogWarning("BBTimes: Failed to locate the NPC copy of type: " + typeof(T));
					return;
				}

				BaldiLevelEditorPlugin.characterObjects.Add(npcName,
					BaldiLevelEditorPlugin.StripAllScripts(npc.gameObject, true)
					);

				PlusLevelLoaderPlugin.Instance.npcAliases.Add(npcName, npc);

				npcsToAdd.Add(new TimesNPC(npcName));
			}

		}

		[HarmonyPatch(typeof(EditorLevel), "InitializeDefaultTextures")]
		[HarmonyPostfix]
		private static void AddRoomTexs(EditorLevel __instance)
		{
			__instance.defaultTextures.Add("Bathroom", new TextureContainer("bathCeil", "bathWall", "bathFloor"));
			__instance.defaultTextures.Add("AbandonedRoom", new TextureContainer("BlueCarpet", "moldWall", "Ceiling"));
			__instance.defaultTextures.Add("BasketballArea", new TextureContainer("dirtyGrayFloor", "SaloonWall", "Ceiling"));
			__instance.defaultTextures.Add("ComputerRoom", new TextureContainer("computerRoomFloor", "computerRoomWall", "computerRoomCeiling"));
			__instance.defaultTextures.Add("DribbleRoom", new TextureContainer("dribbleRoomFloor", "SaloonWall", "Ceiling"));
			__instance.defaultTextures.Add("Forest", new TextureContainer("Grass", "forestWall", "None"));
			__instance.defaultTextures.Add("Kitchen", new TextureContainer("kitchenFloor", "Wall", "Ceiling"));
			__instance.defaultTextures.Add("FocusRoom", new TextureContainer("BlueCarpet", "Wall", "Ceiling"));
		}

		[HarmonyPatch(typeof(PlusLevelEditor), "Initialize")]
		[HarmonyPostfix]
		static void InitializeStuff(PlusLevelEditor __instance)
		{
			string[] files = Directory.GetFiles(Path.Combine(BasePlugin.ModPath, "EditorUI"));
			for (int i = 0; i < files.Length; i++)
				BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/" + Path.GetFileNameWithoutExtension(files[i]), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(files[i]), 40f));

			__instance.toolCats.Find(x => x.name == "items").tools.AddRange(itemsToAdd);
			__instance.toolCats.Find(x => x.name == "characters").tools.AddRange(npcsToAdd);
			__instance.toolCats.Find(x => x.name == "objects").tools.AddRange(markersToAdd);
			__instance.toolCats.Find(x => x.name == "halls").tools.AddRange(
			[
				new TimesRoom("Bathroom"),
				new TimesRoom("AbandonedRoom"),
				new TimesRoom("BasketballArea"),
				new TimesRoom("ComputerRoom"),
				new TimesRoom("DribbleRoom"),
				new TimesRoom("Forest"),
				new TimesRoom("Kitchen"),
				new TimesRoom("FocusRoom")
			]);
		}

		internal static List<EditorTool> markersToAdd, itemsToAdd, npcsToAdd;
		internal static List<ItemObject> points = [];
		struct ObjectData(GameObject obj, Vector3 vec, Quaternion rot)
		{
			public GameObject Item1 = obj;

			public Vector3 Item2 = vec;

			public Quaternion Item3 = rot;
		}

		class TimesItem(string obj) : ItemTool(obj)
		{
			public override Sprite editorSprite => BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/item_" + obj);
			readonly string obj = obj;
		}
		class TimesNPC(string obj) : NpcTool(obj)
		{
			public override Sprite editorSprite => BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/npc_" + obj);
			readonly string obj = obj;
		}
		class TimesRoom(string obj) : FloorTool(obj)
		{
			public override Sprite editorSprite => BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/floor_" + obj);
			readonly string obj = obj;
		}
	}
}
