using BBPlusCustomMusics.Plugin.Public;

namespace BBTimes.CompatibilityModule
{
	internal class CustomMusicsCompat
	{
		internal static void Loadup()
		{
			MusicRegister.AddMIDIsFromDirectory(MidiDestiny.Schoolhouse, BasePlugin.ModPath, "misc", "Audios", "School");
			MusicRegister.AddMIDIsFromDirectory(MidiDestiny.Elevator, BasePlugin.ModPath, "misc", "Audios", "Elevator");
			MusicRegister.AddMusicFilesFromDirectory(SoundDestiny.Ambience, BasePlugin.ModPath, "misc", "Audios", "Ambiences");
			MusicRegister.AddMusicFilesFromDirectory(SoundDestiny.Playtime, BasePlugin.ModPath, "misc", "Audios", "Playtime");
			MusicRegister.AddMusicFilesFromDirectory(SoundDestiny.JohnnyStore, BasePlugin.ModPath, "misc", "Audios", "Store");
		}
	}
}
