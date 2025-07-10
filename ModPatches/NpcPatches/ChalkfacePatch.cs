using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches.NpcPatches;

[HarmonyPatch(typeof(ChalkFace), "Initialize")]
internal static class ChalkfacePatch
{
    static void Postfix(ChalkFace __instance)
    {
        _list.Clear();
        foreach (var room in __instance.ec.rooms)
        {
            if (allowedClassroomCategories.Contains(room.category))
                _list.Add(room);
        }

        int classroomCount = Mathf.RoundToInt(_list.Count * (__instance.classSpawnPercent / 100f));
        while (classroomCount > 0 && _list.Count != 0)
        {
            int index = Random.Range(0, _list.Count);
            __instance.SpawnBoard(_list[index]);
            _list.RemoveAt(index);
            classroomCount--;
        }
    }
    readonly static List<RoomController> _list = [];
    public static HashSet<RoomCategory> allowedClassroomCategories = [];
}