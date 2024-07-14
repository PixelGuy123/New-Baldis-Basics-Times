using BBTimes.CustomContent.Builders;
using BBTimes.CustomContent.MapIcons;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.ModPatches;
using BBTimes.Plugin;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;

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
			Sprite[] sprs = TextureExtensions.LoadSpriteSheet(2, 2, ObjectCreationExtension.defaultMapIconPixelsPerUnit, IconPath, "buticons.png");
			GameButtonSpawnPatch.butIconPre = [
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[0], "ButtonMapIcon_North"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[1], "ButtonMapIcon_West"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[2], "ButtonMapIcon_South"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[3], "ButtonMapIcon_East")
				];
			// map icon for Event Machine
			EventMachineSpawner.iconPre = ObjectCreationExtension.CreateMapIcon<MapIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, "fogMachineIcon.png")), "EventMachineIcon");
		}

		static string IconPath => Path.Combine(BasePlugin.ModPath, "icons");
	}
}
