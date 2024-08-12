using BBPlusCustomMusics;
using BBTimes.Plugin;

namespace BBTimes.CompatibilityModule
{
	internal class CustomMusicsCompat
	{
		internal static void Loadup()
		{
			CustomMusicPlug.AddMidisFromDirectory(false, BasePlugin.ModPath, "misc", "Audios", "School");
			CustomMusicPlug.AddMidisFromDirectory(true, BasePlugin.ModPath, "misc", "Audios", "Elevator");
		}
	}
}
