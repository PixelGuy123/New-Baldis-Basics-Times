﻿using UnityEngine;
using HarmonyLib;
using BBTimes.CustomContent.MapIcons;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(RotoHall))]
	internal static class RotoHallPatch
	{
		[HarmonyPatch("Setup")]
		[HarmonyPostfix]
		static void CreateIcon(RotoHall __instance, CylinderShape shape, MeshRenderer ___cylinder) =>
			__instance.Ec.map.AddIcon(shape == CylinderShape.Straight ? rotoHallIcons[0] : rotoHallIcons[1], ___cylinder.transform, Color.white);
		

		internal static TransformOrientedIcon[] rotoHallIcons;
	}
}
