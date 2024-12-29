using BBTimes.Extensions;
using HarmonyLib;

namespace BBTimes.ModPatches.ItemPatches
{
	[HarmonyPatch(typeof(ITM_PrincipalWhistle), "Use")]
	internal static class ITMPrincipalWhistle
	{
		static void Prefix(PlayerManager pm) =>
			pm.ec.CallOutPrincipals(pm.transform.position, ignoreNormalPrincipal: true);
		
	}
}
