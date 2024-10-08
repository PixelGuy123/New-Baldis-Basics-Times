using BepInEx.Bootstrap;

namespace BBTimes.Plugin
{
	public static class BooleanStorage // This storage will contain all booleans that are gonna be disabled by any mod that requires them to be disabled (REMINDER: NOT SETTINGS, THESE ARE STORED IN THE PLUGIN CLASS)
	{
		public static bool HasCrispyPlus => Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.crispyplus");
	}
}
