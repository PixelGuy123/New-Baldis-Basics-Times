using BBTimes.Plugin;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomItemData : CustomBaseData // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
		protected override string SoundPath => System.IO.Path.Combine(BasePlugin.ModPath, "items", Name, "Audios");
		protected override string TexturePath => System.IO.Path.Combine(BasePlugin.ModPath, "items", Name, "Textures");

		public ItemObject myItmObj;
	}
}
