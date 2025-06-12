using BBTimes.CustomComponents;
using BBTimes.Plugin;
using HarmonyLib;

namespace BBTimes.ModPatches.NpcPatches
{
	[HarmonyPatch(typeof(Entity), "Squish")]
	internal class EntityPatch
	{
		private static bool Prefix(Entity __instance)
		{
			var p = __instance.GetComponent<PlayerAttributesComponent>();
			if (p && p.HasAttribute(Storage.HARDHAT_ATTR_TAG))
				return false;
			return true;
		}
	}
}
