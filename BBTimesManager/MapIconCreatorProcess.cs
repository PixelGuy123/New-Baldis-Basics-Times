using System.IO;
using BBTimes.CustomContent.Builders;
using BBTimes.CustomContent.MapIcons;
using BBTimes.CustomContent.RoomFunctions;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void GetIcons()
		{
			// map icon for mathmachine notebooks
			MathMachinePatches.rightSprite = AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(IconPath, GetAssetName("hiddenNotebookIcon.png"))), ObjectCreationExtension.defaultMapIconPixelsPerUnit);
			// map icon for trapdoors
			Structure_Trapdoor.icon = ObjectCreationExtension.CreateMapIcon<MapIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, GetAssetName("trapDoorIcon.png"))), "TrapdoorMapIcon");
			// map icon for buttons
			Sprite[] sprs = TextureExtensions.LoadSpriteSheet(2, 2, ObjectCreationExtension.defaultMapIconPixelsPerUnit, IconPath, GetAssetName("buticons.png"));
			GameButtonSpawnPatch.butIconPre = [
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[0], "ButtonMapIcon_North"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[1], "ButtonMapIcon_West"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[2], "ButtonMapIcon_South"),
				ObjectCreationExtension.CreateMapIcon<SelfRotatingIcon>(sprs[3], "ButtonMapIcon_East")
				];
			// map icon for Event Machine
			EventMachineSpawner.iconPre = ObjectCreationExtension.CreateMapIcon<MapIcon>(AssetLoader.TextureFromFile(Path.Combine(IconPath, GetAssetName("fogMachineIcon.png"))), "EventMachineIcon");

			// sprs = TextureExtensions.LoadSpriteSheet(2, 1, ObjectCreationExtension.defaultMapIconPixelsPerUnit, IconPath, GetAssetName("rotoHallIcons.png"));
			// map icon for roto halls
			// UNUSED SINCE 0.10
			// RotoHallPatch.rotoHallIcons = [
			// 	ObjectCreationExtension.CreateMapIcon<TransformOrientedIcon>(sprs[0], "RotohallIcon_Straight"),
			// 	ObjectCreationExtension.CreateMapIcon<TransformOrientedIcon>(sprs[1], "RotohallIcon_Corner"),
			// 	];
			// RotoHallPatch.rotoHallIcons[1].invertRotation = true;
		}

		static string IconPath => Path.Combine(BasePlugin.ModPath, "icons");
	}
}
