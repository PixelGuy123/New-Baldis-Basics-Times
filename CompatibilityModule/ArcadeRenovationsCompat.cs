using BaldiEndless;
using BBTimes.CustomComponents;
using BBTimes.Manager;
using HarmonyLib;
using System.Collections.Generic;

namespace BBTimes.CompatibilityModule
{
	internal class ArcadeRenovationsCompat
	{
		internal static void Loadup()
		{
			// ************************ F1 Additions **************************
			EndlessFloorsPlugin.AddGeneratorAction(BBTimesManager.plug.Info, (data) =>
			{
				if (Singleton<CoreGameManager>.Instance.sceneObject.levelNo >= 0)
					ImplementFromIndex(0);
				

				// ******************* F2 Additions *******************
				if (Singleton<CoreGameManager>.Instance.sceneObject.levelNo >= 15)
				{
					ImplementFromIndex(1);
					ImplementFromIndex(3);
				}

				// **************** F3 Additions ****************
				if (Singleton<CoreGameManager>.Instance.sceneObject.levelNo >= 35)
					ImplementFromIndex(2);
				

				void ImplementFromIndex(int i)
				{
					// Custom Rooms
					BBTimesManager.floorDatas[i].RoomAssets.ForEach(room =>
					{
						RoomCategory rCat = room.potentialRooms[0].selection.category;

						if (!data.roomAssets.TryGetValue(rCat, out var assets))
						{
							assets = [];
							data.roomAssets.Add(rCat, assets);
						}

						for (int z = 0; z < room.potentialRooms.Length; z++)
							assets.Add(room.potentialRooms[z]);
					});
					List<RoomCategory> invalidCats = [];

					foreach (var cat in data.roomAssets)
						if (cat.Value.Count == 0)
							invalidCats.Add(cat.Key);

					invalidCats.ForEach(cat => data.roomAssets.Remove(cat));

					BBTimesManager.floorDatas[i].Classrooms.ForEach(data.classRoomAssets.Add);
					BBTimesManager.floorDatas[i].Faculties.ForEach(data.facultyRoomAssets.Add);
					BBTimesManager.floorDatas[i].Offices.ForEach(data.officeRoomAssets.Add);
					BBTimesManager.floorDatas[i].Halls.Do(x => data.hallInsertions.Add(x.Key));
					BBTimesManager.floorDatas[i].SpecialRooms.ForEach(data.specialRoomAssets.Add);
					// Below contains stuff that Arcade adds by default, so unnecessary
					//BBTimesManager.floorDatas[i].NPCs.ForEach(npc => 
					//{
					//	var dat = npc.selection.GetComponent<INPCPrefab>();
					//	if (BBTimesManager.plug.enableReplacementNPCsAsNormalOnes.Value || dat == null || dat.GetReplacementNPCs() == null || dat.GetReplacementNPCs().Length == 0)
					//		data.npcs.Add(npc);
					//	else
					//		data.forcedNpcs.Add(npc.selection);
					//});
					BBTimesManager.floorDatas[i].Events.ForEach(data.randomEvents.Add);
					BBTimesManager.floorDatas[i].Items.ForEach(data.items.Add);
					//BBTimesManager.floorDatas[i].ForcedObjectBuilders.ForEach(data.forcedObjectBuilders.Add);
					//BBTimesManager.floorDatas[i].WeightedObjectBuilders.ForEach(data.objectBuilders.Add);
				}
			});
		}
	}
}


