using BBTimes.CustomComponents;
using HarmonyLib;
using UnityEngine;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(GottaSweep))]
	internal class GottaSweepPatches
	{
		[HarmonyPatch("VirtualUpdate")]
		private static void Postfix(GottaSweep __instance, AudioManager ___audMan)
		{
			var comp = __instance.GetComponent<GottaSweepComponent>();
			if (!comp.active) return;
			comp.cooldown -= __instance.TimeScale * Time.deltaTime;
			if (comp.cooldown < 0f)
			{
				comp.cooldown += cooldown;
				if (Random.value <= chance)
				{
					___audMan.PlaySingle(comp.aud_sweep);
					comp.cooldown /= Random.Range(2f, 5f);
				}
				
			}
		}

		[HarmonyPatch("StartSweeping")]
		[HarmonyPrefix]
		private static void ActiveNow(GottaSweep __instance)
		{
			var c = __instance.GetComponent<GottaSweepComponent>();
			c.active = true;
			c.cooldown = cooldown;
		}
		[HarmonyPatch("StopSweeping")]
		[HarmonyPrefix]
		private static void DeactiveNow(GottaSweep __instance) =>
			__instance.GetComponent<GottaSweepComponent>().active = false;

		const float cooldown = 10f;
		const float chance = 0.55f;
	}
}
