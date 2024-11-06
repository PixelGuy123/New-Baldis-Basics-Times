using BaldiLevelEditor;
using BBTimes.CustomContent.CustomItems;
using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PlusLevelFormat;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BBTimes.CompatibilityModule.EditorCompat
{
	[HarmonyPatch]
	[ConditionalPatchMod("mtm101.rulerp.baldiplus.leveleditor")]
	internal class EditorLevelPatch
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
				man.Get<GameObject>("editorPrefab_sink"),
				man.Get<GameObject>("editorPrefab_Toilet")
			];
			MarkRotatingObject(array[0], Vector3.up * array[0].transform.localScale.y / 2f);
			MarkRotatingObject(array[1], Vector3.zero);
			MarkObject(array[2], Vector3.zero);
			MarkObjectRow("fullStall", [
				new ObjectData(array[0], new Vector3(-5f, 5f, 0f), Quaternion.Euler(0f, 90f, 0f)),
				new ObjectData(array[1], new Vector3(0f, 0f, 4f), default),
				new ObjectData(array[0], new Vector3(5f, 5f, 0f), Quaternion.Euler(0f, 90f, 0f))
			]);
			MarkObject(array[3], Vector3.zero);

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

			MarkObject(man.Get<GameObject>("editorPrefab_KitchenCabinet"), Vector3.up);
			MarkRotatingObject(man.Get<GameObject>("editorPrefab_JoeChef"), Vector3.up * 5f);
			MarkObject(man.Get<GameObject>("editorPrefab_FocusedStudent"), Vector3.up * 5f);

			MarkObject(man.Get<GameObject>("editorPrefab_ComputerTeleporter"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_DustShroom"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_SensitiveVase"), Vector3.up * 4.2f);

			// Decorations
			MarkObject(man.Get<GameObject>("editorPrefab_SecretBread"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_TimesKitchenSteak"), Vector3.zero);
			MarkObject(man.Get<GameObject>("editorPrefab_JoeSign"), Vector3.zero);

			// ************************ Items ****************************

			AddItem("basketball", "Basketball");
			AddItem("bell", "Bell");
			AddItem("blowDrier", "BlowDrier");
			AddItem("beehive", "Beehive");
			AddItem("BSED", "BSED");
			AddItem("cherryBsoda", "CherryBsoda");
			AddItem("chocolate", "HotChocolate");
			AddItem("comicallyLargeTrumpet", "ComicallyLargeTrumpet");
			AddPointItem<ITM_DivideYTP>("DivisionPoint");
			AddItem("DoorStopper", "DoorStopper");
			AddItem("electricalGel", "ElectricalGel");
			AddItem("empty", "EmptyWaterBottle");
			AddItem("fidgetSpinner", "FidgetSpinner");
			AddItem("fryingPan", "FryingPan");
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
			AddNPC("adverto", "Adverto");
			AddNPC("bubbly", "Bubbly");
			AddNPC("Camerastand", "Camerastand");
			AddNPC("cheeseMan", "CheeseMan");
			AddNPC("crazyClock", "CrazyClock");
			AddNPCCopy<ClassicGottaSweep>("oldsweep");
			AddNPC("detentionBot", "DetentionBot");
			AddNPC("dribble", "Dribble");
			AddNPC("faker", "Faker");
			AddNPC("gluebotrony", "Glubotrony");
			AddNPC("happyholidays", "HappyHolidays");
			AddNPC("inkArtist", "InkArtist");
			AddNPC("JerryTheAC", "JerryTheAC");
			AddNPC("leapy", "Leapy");
			AddNPC("MGS", "Magicalstudent");
			AddNPC("mopper", "Mopper");
			AddNPC("Mugh", "Mugh");
			AddNPC("officeChair", "OfficeChair");
			AddNPC("pencilBoy", "PencilBoy");
			AddNPC("phawillow", "Phawillow");
			AddNPC("penny", "Penny");
			AddNPC("pix", "Pix");
			AddNPC("pran", "Pran");
			AddNPC("quiker", "Quiker");
			AddNPC("rollBot", "Rollingbot");
			AddNPC("serOran", "SerOran");
			AddNPC("scienceTeacher", "ScienceTeacher");
			AddNPC("stunly", "Stunly");
			AddNPC("superintendent", "Superintendent");
			AddNPC("spj", "Superintendentjr");
			AddNPC("tickTock", "TickTock");
			AddNPC("watcher", "Watcher");
			AddNPC("ZapZap", "ZapZap");

			// ************* Local Methods ***************

			static void MarkRotatingObject(GameObject obj, Vector3 offset)
			{
				markersToAdd.Add(new(obj.name, new(false, null)));
				BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>(obj.name, obj, offset, false));
			}

			static void MarkObject(GameObject obj, Vector3 offset)
			{
				markersToAdd.Add(new(obj.name, new(true, null)));
				BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>(obj.name, obj, offset, false));
			}

			static void MarkObjectRow(string prebuiltToolName, params ObjectData[] objs) =>
				markersToAdd.Add(new(prebuiltToolName, new(false, objs)));

			static void AddItem(string itemName, string itemEnum)
			{
				var en = EnumExtensions.GetFromExtendedName<Items>(itemEnum);
				var itm = ItemMetaStorage.Instance.FindByEnumFromMod(en, BBTimesManager.plug.Info).value;

				BaldiLevelEditorPlugin.itemObjects.Add("times_" + itemEnum, itm);
				itemsToAdd.Add(new(itemEnum, itemName));
			}

			static void AddPointItem<T>(string itemName) where T : Item
			{
				var itm = points.Find(x => x.item is T);
				BaldiLevelEditorPlugin.itemObjects.Add("times_" + itemName, itm);
				itemsToAdd.Add(new(itemName, itemName));
			}

			static void AddNPC(string npcName, string npcEnum)
			{
				var en = EnumExtensions.GetFromExtendedName<Character>(npcEnum);
				var val = NPCMetaStorage.Instance.Find(x => x.character == en && BBTimesManager.plug.Info == x.info).value;

				BaldiLevelEditorPlugin.characterObjects.Add("times_" + npcEnum,
					BaldiLevelEditorPlugin.StripAllScripts(val.gameObject, true)
					);

				npcsToAdd.Add(new(npcEnum, npcName));
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

				BaldiLevelEditorPlugin.characterObjects.Add("times_" + npcName,
					BaldiLevelEditorPlugin.StripAllScripts(npc.gameObject, true)
					);

				npcsToAdd.Add(new(npcName, npcName));
			}

		}

		[HarmonyPatch(typeof(EditorLevel), "InitializeDefaultTextures")]
		[HarmonyPostfix]
		private static void AddRoomTexs(EditorLevel __instance)
		{
			__instance.defaultTextures.Add("Bathroom", new TextureContainer("bathFloor", "bathWall", "bathCeil"));
			__instance.defaultTextures.Add("AbandonedRoom", new TextureContainer("BlueCarpet", "moldWall", "Ceiling"));
			__instance.defaultTextures.Add("BasketballArea", new TextureContainer("dirtyGrayFloor", "SaloonWall", "Ceiling"));
			__instance.defaultTextures.Add("ComputerRoom", new TextureContainer("computerRoomFloor", "computerRoomWall", "computerRoomCeiling"));
			__instance.defaultTextures.Add("DribbleRoom", new TextureContainer("dribbleRoomFloor", "SaloonWall", "Ceiling"));
			__instance.defaultTextures.Add("Forest", new TextureContainer("Grass", "forestWall", "None"));
			__instance.defaultTextures.Add("Kitchen", new TextureContainer("kitchenFloor", "Wall", "Ceiling"));
			__instance.defaultTextures.Add("FocusRoom", new TextureContainer("BlueCarpet", "Wall", "Ceiling"));
			__instance.defaultTextures.Add("SuperMystery", new TextureContainer("redCeil", "redWall", "redFloor"));
			__instance.defaultTextures.Add("ExibitionRoom", new TextureContainer("BlueCarpet", "Wall", "Ceiling"));
		}

		[HarmonyPatch(typeof(PlusLevelEditor), "Initialize")]
		[HarmonyPostfix]
		static void InitializeStuff(PlusLevelEditor __instance)
		{
			string[] files = Directory.GetFiles(Path.Combine(BasePlugin.ModPath, "EditorUI"));
			for (int i = 0; i < files.Length; i++)
				BaldiLevelEditorPlugin.Instance.assetMan.Add("UI/" + Path.GetFileNameWithoutExtension(files[i]), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(files[i]), 40f));

			__instance.toolCats.Find(x => x.name == "items").tools.AddRange(itemsToAdd.ConvertAll(x => new TimesItem(x.Key, x.Value)));
			__instance.toolCats.Find(x => x.name == "characters").tools.AddRange(npcsToAdd.ConvertAll(x => new TimesNPC(x.Key, x.Value)));
			var objectCats = __instance.toolCats.Find(x => x.name == "objects").tools;

			foreach (var objMark in markersToAdd)
			{
				if (objMark.Value.Value == null)
				{
					objectCats.Add(objMark.Value.Key ? new ObjectTool(objMark.Key) : new RotateAndPlacePrefab(objMark.Key));
					continue;
				}
				PrefabLocation[] array = new PrefabLocation[objMark.Value.Value.Length];

				for (int i = 0; i < objMark.Value.Value.Length; i++)
					array[i] = new PrefabLocation(objMark.Value.Value[i].Item1.name, PlusLevelLoader.Extensions.ToData(objMark.Value.Value[i].Item2), PlusLevelLoader.Extensions.ToData(objMark.Value.Value[i].Item3));

				objectCats.Add(new PrebuiltStructureTool(objMark.Key, new EditorPrebuiltStucture(array)));

			}


			__instance.toolCats.Find(x => x.name == "halls").tools.AddRange(
			[
				new TimesRoom("Bathroom"),
				new TimesRoom("AbandonedRoom"),
				new TimesRoom("BasketballArea"),
				new TimesRoom("ComputerRoom"),
				new TimesRoom("DribbleRoom"),
				new TimesRoom("Forest"),
				new TimesRoom("Kitchen"),
				new TimesRoom("FocusRoom"),
				new TimesRoom("SuperMystery"),
				new TimesRoom("ExibitionRoom")
			]);
		}

		static List<KeyValuePair<string, string>> npcsToAdd, itemsToAdd;
		static List<KeyValuePair<string, KeyValuePair<bool, ObjectData[]>>> markersToAdd;

		internal static void AddPoint(ItemObject point) =>
			points.Add(point);

		readonly static List<ItemObject> points = [];
		struct ObjectData(GameObject obj, Vector3 vec, Quaternion rot)
		{
			public GameObject Item1 = obj;

			public Vector3 Item2 = vec;

			public Quaternion Item3 = rot;
		}

		class TimesItem(string obj, string objTex) : ItemTool("times_" + obj)
		{
			public override Sprite editorSprite => BaldiLevelEditorPlugin.Instance.assetMan.ContainsKey("UI/item_" + objTex) ?
				BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/item_" + objTex) : BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/Item_" + objTex);
			readonly string objTex = objTex;
		}
		class TimesNPC(string obj, string objTex) : NpcTool("times_" + obj)
		{
			public override Sprite editorSprite => BaldiLevelEditorPlugin.Instance.assetMan.ContainsKey("UI/npc_" + objTex) ?
				BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/npc_" + objTex) : BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/Npc_" + objTex);
			readonly string objTex = objTex;
		}
		class TimesRoom(string obj) : FloorTool(obj)
		{
			public override Sprite editorSprite => BaldiLevelEditorPlugin.Instance.assetMan.ContainsKey("UI/floor_" + obj) ?
				BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/floor_" + obj) : BaldiLevelEditorPlugin.Instance.assetMan.Get<Sprite>("UI/Floor_" + obj);
			readonly string obj = obj;
		}
	}
}
