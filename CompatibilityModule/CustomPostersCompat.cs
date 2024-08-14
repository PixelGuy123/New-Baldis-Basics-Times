using LuisRandomness.BBPCustomPosters;
using MTM101BaldAPI.AssetTools;
using System.IO;
using BBTimes.Manager;

namespace BBTimes.CompatibilityModule
{
	internal class CustomPostersCompat
	{
		internal static void Loadup() =>
			CustomPostersPlugin.AddBuiltInPackFromDirectory(BBTimesManager.plug, Path.Combine(AssetLoader.GetModPath(BBTimesManager.plug), "posters", "custompostermod"));
	}
}
