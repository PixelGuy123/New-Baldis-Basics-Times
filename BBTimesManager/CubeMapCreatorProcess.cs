using BBTimes.CustomComponents;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using PixelInternalAPI.Extensions;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateCubeMaps()
		{
			var F2Map = TextureExtensions.CubemapFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cubemap_twilight.png")));
			var F3Map = TextureExtensions.CubemapFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "cubemap_night.png")));



			// Add lightings outside for GameManagers
			foreach (var man in Resources.FindObjectsOfTypeAll<SceneObject>())
			{
				var comp = man.manager.GetComponent<MainGameManagerExtraComponent>();
				if (comp == null) continue;
				//if (man.levelTitle == "F1") By default, it's the *default* cube map
				//{
				//	comp.mapForToday = ObjectCreationExtension.defaultCubemap;
				//	continue;
				//}
				if (man.levelTitle == "F2")
				{
					comp.outsideLighting = new Color(0.7f, 0.7f, 0.7f, 1f);
					man.skybox = F2Map;
					continue;
				}
				if (man.levelTitle == "F3")
				{
					man.skybox = F3Map;
					comp.outsideLighting = new Color(0.45f, 0.45f, 0.45f, 1f);
					continue;
				}
			}
		}
	}
}
