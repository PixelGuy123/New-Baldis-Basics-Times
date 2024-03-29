﻿using MTM101BaldAPI.AssetTools;
using BBTimes.CreatorHelpers;
using BBTimes.CustomContent.RoomFunctions;
using System.IO;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using HarmonyLib;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateRoomFunctions()
		{
			// Random Window Function for cafe
			AddFunctionToEveryRoom<RandomWindowFunction>("Cafeteria").window = CreatorExtensions.CreateWindow("classicWindow", // Little tricks to make this function
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "ClassicWindow.png")),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "ClassicWindow_Broken.png")),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "ClassicWindow_Mask.png"))); // Yeah, it creates a window here too. But the WindowCreationprocess is for those that are supposed to spawn naturally;

			// Random poster functon for class and office
			PosterObject[] poster = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "wall_clock.png"))])];

			AddFunctionToEveryRoom<RandomPosterFunction>("Class").posters = poster;
			AddFunctionToEveryRoom<RandomPosterFunction>("Office").posters = poster;

			// High ceiling function
			var highCeil = AddFunctionToEveryRoom<HighCeilingRoomFunction>("Cafeteria");
			highCeil.height = 5;
			highCeil.customCeiling = ObjectCreationExtension.blackTex;
			highCeil.customWallProximityToCeil = [AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "rooms", "Cafeteria", "wallFadeInBlack.png"))];
			highCeil.chanceToHappen = 0.8f;
			highCeil.customLight = man.Get<GameObject>("prefab_cafeHangingLight").transform;

			highCeil = AddFunctionToEveryRoom<HighCeilingRoomFunction>("Library");
			highCeil.height = 1;
			highCeil.targetTransformNamePrefix = "Bookshelf";
			highCeil.targetTransformOffset = 9f;
			// highCeil.customWallProximityToCeil = [Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => x.name.StartsWith("Library")).wallTex];
			var libraryTex = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Wall" && x.GetInstanceID() > 0); // Any instance id > 0 is a prefab (I checked that!)

			Resources.FindObjectsOfTypeAll<RoomAsset>().DoIf(x => x.name.StartsWith("Library"), x => x.wallTex = libraryTex);



			static R AddFunctionToEveryRoom<R>(string prefix) where R : RoomFunction
			{
				var r = Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => x.name.StartsWith(prefix));
				var comp = r.roomFunctionContainer.gameObject.AddComponent<R>();
				r.roomFunctionContainer.AddFunction(comp);
				return comp;
			}
		}
	}
}
