

using BBTimes.Plugin;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomNPCData : CustomBaseData // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
		protected string SoundPath => System.IO.Path.Combine(BasePlugin.ModPath, "npcs", Name, "Audios");

		public Character[] npcsBeingReplaced = [];

		public NPC Npc { get; internal set; }

		public int replacementWeight = 0;
	}
}
