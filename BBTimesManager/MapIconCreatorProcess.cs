using BBTimes.CustomContent.Builders;
using BBTimes.CustomContent.MapIcons;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.ModPatches;
using BBTimes.Plugin;
using MTM101BaldAPI.AssetTools;
using System.IO;

namespace BBTimes.Manager
{
    internal static partial class BBTimesManager
	{
		static void GetIcons()
		{
			// map icon for mathmachine notebooks
			MathMachinePatches.rightSprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(IconPath, "hiddenNotebookIcon.png")), ObjectCreationExtension.defaultMapIconPixelsPerUnit);
			// map icon for trapdoors
			TrapDoorBuilder.icon = ObjectCreationExtension.CreateMapIcon<MapIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, "trapDoorIcon.png")), "TrapdoorMapIcon");
			// map icon for buttons
			GameButtonSpawnPatch.butIconPre = [
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, "but_North.png")), "ButtonMapIcon_North"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, "but_West.png")), "ButtonMapIcon_West"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, "but_South.png")), "ButtonMapIcon_South"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, "but_East.png")), "ButtonMapIcon_East")
				];
		}

		static string IconPath => Path.Combine(BasePlugin.ModPath, "icons");
	}
}
