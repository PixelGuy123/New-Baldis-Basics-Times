using BBTimes.CompatibilityModule.GrapplingHookTweaksCompats;
using BBTimes.Plugin;
using BepInEx.Bootstrap;

namespace BBTimes.CompatibilityModule
{
	internal static class CompatibilityInitializer
	{
		internal static void InitializeOnLoadMods()
		{
			if (Chainloader.PluginInfos.ContainsKey(Storage.guid_HookTweaks))
				GrapplingHookTweaksCompat.Loadup();
		}
		internal static void InitializePostOnLoadMods()
		{
			// if (BBTimesManager.plug.HasInfiniteFloors && !BBTimesManager.plug.disableArcadeRennovationsSupport.Value)
			// 	ArcadeRenovationsCompat.Loadup();
		}
		internal static void InitializeOnAwake()
		{
			if (Chainloader.PluginInfos.ContainsKey(Storage.guid_CustomMusics))
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
