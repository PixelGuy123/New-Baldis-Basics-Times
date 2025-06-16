using System;
using BBTimes.Manager;
using BepInEx.Bootstrap;
using UnityEngine;

namespace BBTimes.Plugin
{
	public static class Storage
	// This storage will contain all global variables that are gonna be disabled by any mod that requires them to be disabled (REMINDER: NOT SETTINGS, THESE ARE STORED IN THE PLUGIN CLASS)
	// Plus, this storage will contain other global variables that are used by the mod for miscellaneous purposes
	{
		public static bool HasCrispyPlus => Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.crispyplus");

		public static bool IsChristmas
		{
			get
			{
				if (BBTimesManager.plug.forceChristmasMode.Value)
					return true;
				var now = DateTime.Now;

				var minDate = new DateTime(now.Year, 12, 1);
				var maxDate = new DateTime(now.Year, 12, 31);

				return now >= minDate && now <= maxDate;
			}
		}

		// ------------------- Constant Strings ---------------------

		public const string
		HOTCHOCOLATE_ATTR_TAG = "hotchocolateactive",
		HARDHAT_ATTR_TAG = "protectedhead",
		FOOD_TAG = "food",
		DRINK_TAG = "drink";
		public const string ChristmasSpecial_TimesTag = "Times_SpecialTags_ChristmasSpecial";

		// ------------------- Constant Floats ---------------------
		public const float GaugeSprite_PixelsPerUnit = 1f;

		// ------------------- Readonly Vector2s ---------------------
		public static readonly Vector2 Const_RefScreenSize = new(480f, 360f);
	}


}
