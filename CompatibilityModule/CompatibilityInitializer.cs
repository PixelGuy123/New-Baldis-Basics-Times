﻿using BepInEx.Bootstrap;

namespace BBTimes.CompatibilityModule
{
    internal static class CompatibilityInitializer
	{
		internal static void InitializeOnLoadMods()
		{
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.grapplinghooktweaks"))
				GrapplingHookTweaksCompat.Loadup();
		}
		internal static void InitializeOnAwake()
		{
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.custommusics"))
				CustomMusicsCompat.Loadup();
			if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.customvendingmachines"))
				CustomVendingMachinesCompat.Loadup();
			if (Chainloader.PluginInfos.ContainsKey("io.github.luisrandomness.bbp_custom_posters"))
				CustomPostersCompat.Loadup();
		}
	}
}