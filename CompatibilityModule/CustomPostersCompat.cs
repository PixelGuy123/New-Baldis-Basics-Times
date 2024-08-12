using BBTimes.Plugin;
using LuisRandomness.BBPCustomPosters;
using MTM101BaldAPI.AssetTools;
using System.IO;

namespace BBTimes.CompatibilityModule
{
	internal class CustomPostersCompat
	{
		internal static void Loadup() =>
			CustomPostersPlugin.AddBuiltInPackFromDirectory(BasePlugin.i, Path.Combine(AssetLoader.GetModPath(BasePlugin.i), "posters", "custompostermod"));
	}
}
