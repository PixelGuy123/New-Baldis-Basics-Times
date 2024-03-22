namespace BBTimes.Plugin
{
	public static class BooleanStorage // This storage will contain all booleans that are gonna be disabled by any mod that requires them to be disabled (REMINDER: NOT SETTINGS, THESE ARE STORED IN THE PLUGIN CLASS)
	{
		public static bool endGameMusic = true; // Endless floors has this aswell
		public static bool endGameAnimation = true; // Endless floors has red animation aswell
		public static bool cutsceneEnd = true; // The cutscene end (since endless floors is F3, this could be disabled and only shown in F99 maybe)
	}
}
