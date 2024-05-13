using HarmonyLib;
using BBTimes.CustomComponents;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Entity), "Squish")]
	internal class EntityPatch
	{
		private static bool Prefix(Entity __instance)
		{
			var p = __instance.GetComponent<PlayerAttributesComponent>();
			if (p && p.HasAttribute("protectedhead"))
				return false;
			return true;
		}
	}
}
