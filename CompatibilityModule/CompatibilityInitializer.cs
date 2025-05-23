using BBTimes.CompatibilityModule.GrapplingHookTweaksCompats;
using BBTimes.Manager;
using BepInEx.Bootstrap;

namespace BBTimes.CompatibilityModule
{
	internal static class CompatibilityInitializer
	{
		internal static void InitializeOnLoadMods()
		{
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.grapplinghooktweaks"))
				GrapplingHookTweaksCompat.Loadup();
		}
		internal static void InitializePostOnLoadMods()
		{
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.stackableitems"))
				StackableItemsCompat.Loadup();
			// if (BBTimesManager.plug.HasInfiniteFloors && !BBTimesManager.plug.disableArcadeRennovationsSupport.Value)
			// 	ArcadeRenovationsCompat.Loadup();
		}
		internal static void InitializeOnAwake()
		{
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.custommusics"))
				CustomMusicsCompat.Loadup();
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.customvendingmachines"))
				CustomVendingMachinesCompat.Loadup();
			if (Chainloader.PluginInfos.ContainsKey("io.github.uncertainluei.baldiplus.customposters"))
				CustomPostersCompat.Loadup();
			if (Chainloader.PluginInfos.ContainsKey("baldi.basics.plus.advanced.mod"))
				AdvancedEditionCompat.Loadup();
		}
	}
}
