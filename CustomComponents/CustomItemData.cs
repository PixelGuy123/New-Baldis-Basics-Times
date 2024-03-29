﻿using BBTimes.Plugin;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomItemData : CustomBaseData // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
		protected string SoundPath => System.IO.Path.Combine(BasePlugin.ModPath, "items", name, "Audios");

		public Items myEnum;
	}
}
