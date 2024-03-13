using BBTimes.CustomComponents;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateCubeMaps()
		{
			var F2Map = ObjectCreationExtension.CubemapFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cubemap_twilight.png")));
			var F3Map = ObjectCreationExtension.CubemapFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cubemap_night.png")));



			// Add lightings outside for GameManagers
			foreach (var man in Resources.FindObjectsOfTypeAll<MainGameManager>())
			{
				var comp = man.GetComponent<MainGameManagerExtraComponent>();

				if (man.name.StartsWith("Lvl1"))
				{
					comp.mapForToday = ObjectCreationExtension.defaultCubemap;
					continue;
				}
				if (man.name.StartsWith("Lvl2"))
				{
					comp.outsideLighting = new Color(0.5f, 0.5f, 0.5f, 1f);
					comp.mapForToday = F2Map;
					continue;
				}
				if (man.name.StartsWith("Lvl3"))
				{
					comp.mapForToday = F3Map; // Temporary!
					comp.outsideLighting = new Color(0.25f, 0.25f, 0.25f, 1f);
					continue;
				}
			}
		}
	}
}
