

using BBTimes.Plugin;

namespace BBTimes.CustomComponents.CustomDatas
{
    public class CustomObjectPrefabData : CustomBaseData // A basic "mutable" class just for the sole purpose of storing extra info for items
    {
		protected string SoundPath => System.IO.Path.Combine(BasePlugin.ModPath, "objects", Name, "Audios");
		protected string TexturePath => System.IO.Path.Combine(BasePlugin.ModPath, "objects", Name, "Textures");
	}
}
