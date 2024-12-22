using BBTimes.Manager;
using BepInEx.Bootstrap;
using System;

namespace BBTimes.Plugin
{
	public static class BooleanStorage // This storage will contain all booleans that are gonna be disabled by any mod that requires them to be disabled (REMINDER: NOT SETTINGS, THESE ARE STORED IN THE PLUGIN CLASS)
	{
		public static bool HasCrispyPlus => Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.crispyplus");

		public static bool IsChristmas { 
			get {
				if (BBTimesManager.plug.forceChristmasMode.Value)
					return true;
				var now = DateTime.Now;

				var minDate = new DateTime(now.Year, 12, 22);
				var maxDate = new DateTime(now.Year, 12, 31);

				return now >= minDate && now <= maxDate;
			} 
		}
	}
}
