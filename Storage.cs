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
		ATTR_FREEZE_STAMINA_UPDATE_TAG = "disableStaminaUpdate",
		ATTR_FREEZE_PLAYER_MOVEMENT_TAG = "disablePlayerMovement",
		FOOD_TAG = "food",
		DRINK_TAG = "drink";
		public const string ChristmasSpecial_TimesTag = "Times_SpecialTags_ChristmasSpecial";

		// -------------------- GUIDs ------------------
		public const string
		guid_Advanced = "mrsasha5.baldi.basics.plus.advanced",
		guid_LevelStudio = "mtm101.rulerp.baldiplus.levelstudio",
		guid_LevelLoader = "mtm101.rulerp.baldiplus.levelstudioloader",
		guid_HookTweaks = "pixelguy.pixelmodding.baldiplus.grapplinghooktweaks",
		guid_CustomMusics = "pixelguy.pixelmodding.baldiplus.custommusics",
		guid_CustomVendingMachines = "pixelguy.pixelmodding.baldiplus.customvendingmachines",
		guid_CustomPosters = "io.github.uncertainluei.baldiplus.customposters",
		guid_Mtm101API = "mtm101.rulerp.bbplus.baldidevapi",
		guid_PixelIntAPI = "pixelguy.pixelmodding.baldiplus.pixelinternalapi",
		guid_ExtraFunSettings = "rost.moment.baldiplus.funsettings",
		guid_AnimationsPlus = "pixelguy.pixelmodding.baldiplus.newanimations",
		guid_DecorationsPlus = "pixelguy.pixelmodding.baldiplus.newdecors",
		guid_CustomMainMenusAPI = "pixelguy.pixelmodding.baldiplus.custommainmenusapi";

		// ------------------- Constant Floats ---------------------
		public const float GaugeSprite_PixelsPerUnit = 1f;

		// ------------------- Readonly Vector2s ---------------------
		public static readonly Vector2 Const_RefScreenSize = new(480f, 360f);
	}


}
