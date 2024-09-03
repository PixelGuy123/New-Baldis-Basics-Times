﻿using BBTimes.CustomComponents;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(PlayerMovement), "PlayerMove")]
	internal class PlayerCheatPatch
	{
		static void Prefix(PlayerMovement __instance)
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				var dat = __instance.pm.ec.GetComponent<EnvironmentControllerData>();
				foreach (var ev in dat.OngoingEvents)
					if (ev != null)
						__instance.pm.ec.StopCoroutine(ev);

				dat.OngoingEvents.Clear();
				if (__instance.pm.ec.events.Count == 0)
				{
					Debug.LogWarning("No event detected!");
					return;
				}
				__instance.pm.ec.StartCoroutine(__instance.pm.ec.EventTimer(__instance.pm.ec.events[0], 5f));
				__instance.pm.ec.events.RemoveAt(0);
			}
		}
	}
}
