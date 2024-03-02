

using BBTimes.Plugin;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomNPCData : CustomBaseData<Character> // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
		protected string SoundPath => System.IO.Path.Combine(BasePlugin.ModPath, "npcs", name, "Audios");

		public Character npcBeingReplaced = Character.Null;
    }
}
