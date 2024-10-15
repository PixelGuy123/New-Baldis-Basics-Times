using BaldiEndless;
using BBTimes.CustomComponents;
using BBTimes.Manager;
using HarmonyLib;

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

					BBTimesManager.floorDatas[i].Classrooms.ForEach(data.classRoomAssets.Add);
					BBTimesManager.floorDatas[i].Faculties.ForEach(data.FieldTripRoomAssets.Add); // Apparently that's the new Faculty implementation??
					BBTimesManager.floorDatas[i].Offices.ForEach(data.officeRoomAssets.Add);
					BBTimesManager.floorDatas[i].Halls.Do(x => data.hallInsertions.Add(x.Key));
					BBTimesManager.floorDatas[i].SpecialRooms.ForEach(data.specialRoomAssets.Add);
					BBTimesManager.floorDatas[i].NPCs.ForEach(npc =>
					{
						var dat = npc.selection.GetComponent<INPCPrefab>();
						if (dat == null || dat.GetReplacementNPCs() == null || dat.GetReplacementNPCs().Length == 0)
							data.npcs.Add(npc);
						else
							data.forcedNpcs.Add(npc.selection);
					});
					BBTimesManager.floorDatas[i].Events.ForEach(data.randomEvents.Add);
					BBTimesManager.floorDatas[i].Items.ForEach(data.items.Add);
					BBTimesManager.floorDatas[i].ForcedObjectBuilders.ForEach(data.forcedObjectBuilders.Add);
					BBTimesManager.floorDatas[i].WeightedObjectBuilders.ForEach(data.objectBuilders.Add);
				}
			});
		}
	}
}
