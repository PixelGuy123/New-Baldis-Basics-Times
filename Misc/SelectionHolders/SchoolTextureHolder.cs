﻿using UnityEngine;
using PixelInternalAPI.Classes;

namespace BBTimes.Misc.SelectionHolders
{
	public class SchoolTextureHolder(Texture2D tex, int weight, RoomCategory whereCanSpawn, SchoolTexture texType) : SelectionHolder<Texture2D, RoomCategory>(tex, weight, [whereCanSpawn])
	{
		readonly SchoolTexture _tex = texType;
		public SchoolTexture TextureType => _tex;
	}
	
}
