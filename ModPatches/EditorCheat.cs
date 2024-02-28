using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace BBTimes.ModPatches
{

	// Some level editor Iguesss
#if CHEAT

	[HarmonyPatch(typeof(PlayerMovement))]
	internal class Editor
	{
		[HarmonyPatch("PlayerMove")]
		private static void Postfix(PlayerMovement __instance)
		{
			if (Input.GetKeyDown(KeyCode.C))
			{
				currentAsset = ScriptableObject.CreateInstance<RoomAsset>();
				currentAsset.florTex = texs[0];
					currentAsset.ceilTex = texs[1];
					currentAsset.wallTex = texs[2];
				Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText("Created new room asset", 4f);
			}
			if (currentAsset == null) 
				return;

			if (Input.GetKeyDown(KeyCode.V))
			{
				anchor = IntVector2.GetGridPosition(__instance.transform.position);
				Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText($"Anchored updated to position: {anchor.x},{anchor.z}", 4f);
			}

			if (Input.GetKeyDown(KeyCode.J))
			{
				var pos = IntVector2.GetGridPosition(__instance.transform.position);
				Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText($"Your position is: {pos.x},{pos.z}", 4f);
			}
				

			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				binary[0] = binary[0] == '0' ? '1' : '0';
				QuickLog();
			}
			
			if (Input.GetKeyDown(KeyCode.Alpha8)) 
			{ 
				binary[1] = binary[1] == '0' ? '1' : '0';
				QuickLog();
			}

			if (Input.GetKeyDown(KeyCode.Alpha9))
			{ 
				binary[2] = binary[2] == '0' ? '1' : '0';
				QuickLog();
			}

			if (Input.GetKeyDown(KeyCode.Alpha0))
			{ 
				binary[3] = binary[3] == '0' ? '1' : '0';
				QuickLog();
			}

			if (Input.GetKeyDown(KeyCode.B))
				QuickLog();

			//Convert.ToInt32("11011",2);
			//string.Join(string.Empty, binary);

			static void QuickLog() =>
				Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText($"Current binary representation: {string.Join(string.Empty, binary)}", 4f);

			if (Input.GetKeyDown(KeyCode.N))
			{
				var curpos = IntVector2.GetGridPosition(__instance.transform.position);
				int type = Convert.ToInt32(string.Join(string.Empty, binary), 2);
				__instance.pm.ec.CreateCell(type, __instance.pm.ec.transform, curpos, __instance.pm.ec.mainHall);
				currentAsset.cells.Add(new()
				{
					pos = curpos - anchor,
					roomId = 0,
					type = type
				});
			}

			if (Input.GetKeyDown(KeyCode.L))
			{
				if (currentAsset.cells.Count > 0)
					currentAsset.cells.RemoveAt(currentAsset.cells.Count - 1);
				Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText($"Your room has now: {currentAsset.cells.Count} cells", 4f);
			}

			if (Input.GetKeyDown(KeyCode.K))
			{
				var ld = Resources.FindObjectsOfTypeAll<LevelObject>().First(x => x.name == "Main1");
				ld.potentialExtraRooms = ld.potentialExtraRooms.AddToArray(new WeightedRoomAsset() { selection = currentAsset, weight = 50 });
				Singleton<CoreGameManager>.Instance.GetHud(0).ShowEventText("Added asset to floor 1", 4f);
				ld.minExtraRooms = Mathf.Max(ld.potentialExtraRooms.Length, ld.minExtraRooms);
				ld.maxExtraRooms = Mathf.Max(ld.potentialExtraRooms.Length, ld.maxExtraRooms);
			}


		}
		[HarmonyPatch("Start")]
		private static void Prefix()
		{
			var ar = Resources.FindObjectsOfTypeAll<Texture2D>();
			texs[0] = ar.First(x => x.name == "Placeholder_Floor");
			texs[1] = ar.First(x => x.name == "Placeholder_Celing");
			texs[2] = ar.First(x => x.name == "Placeholder_Wall_W");
		}

		static readonly char[] binary = ['0', '0', '0', '0'];

		readonly static Texture2D[] texs = new Texture2D[3];

		static RoomAsset currentAsset;

		static IntVector2 anchor = new(0, 0);
	}

#endif
}
