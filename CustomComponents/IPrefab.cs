﻿namespace BBTimes.CustomComponents
{
	public interface IPrefab
	{
		public string TexturePath { get; }
		public string SoundPath { get; }
		public string Name { get; set; }
	}
}
