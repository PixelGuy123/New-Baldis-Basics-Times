namespace BBTimes.Misc
{
	public enum SchoolTexture
	{
		Null = 0,
		Floor = 1,
		Wall = 2,
		Ceiling = 3
	}

	public static class SchoolTextureExtensions
	{
		public static SchoolTexture GetSchoolTextureFromName(this string name) =>
			name.ToLower() switch
			{
				"floor" => SchoolTexture.Floor,
				"wall" => SchoolTexture.Wall,
				"ceil" or "ceiling" => SchoolTexture.Ceiling, // I NEVER HEARD OF THIS or KEYWORD OMFG
				_ => SchoolTexture.Null
			};
	}
}
