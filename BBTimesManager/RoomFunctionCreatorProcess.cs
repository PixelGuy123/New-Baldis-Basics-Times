using MTM101BaldAPI.AssetTools;
using BBTimes.Helpers;
using BBTimes.CustomContent.RoomFunctions;
using System.IO;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using BBTimes.Extensions.ObjectCreationExtensions;
using HarmonyLib;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateRoomFunctions() // This is specifically for base game rooms, custom rooms can add their room functions by other ways
		{
			var classicWindow = CreatorExtensions.CreateWindow("classicWindow",
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("ClassicWindow.png"))),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("ClassicWindow_Broken.png"))),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("ClassicWindow_Mask.png"))));
			// Random Window Function for cafe
			AddFunctionToEveryRoom<RandomWindowFunction>(CafeteriaPrefix).window = classicWindow; // Yeah, it creates a window here too. But the WindowCreationprocess is for those that are supposed to spawn naturally;
			AddFunctionToEveryRoom<RandomWindowFunction>(PlaygroundPrefix).window = classicWindow;

			// Random poster functon for class and office
			PosterObject[] poster = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("wall_clock.png")))])];

			AddFunctionToEveryRoom<RandomPosterFunction>(ClassPrefix).posters = poster;
			AddFunctionToEveryRoom<RandomPosterFunction>(OfficePrefix).posters = poster;
			AddFunctionToEveryRoom<RandomPosterFunction>(CafeteriaPrefix).posters = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Cafeteria", "CafeteriaRules.png"))]),
				ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(GetRoomAsset("Cafeteria", "food_allergy_.png"))])];

			// High ceiling function
			var highCeil = AddFunctionToEveryRoom<HighCeilingRoomFunction>(CafeteriaPrefix);
			highCeil.ceilingHeight = 5;
			highCeil.customCeiling = ObjectCreationExtension.blackTex;
			highCeil.customWallProximityToCeil = [AssetLoader.TextureFromFile(GetRoomAsset("Cafeteria", "wallFadeInBlack.png"))];
			highCeil.chanceToHappen = 0.8f;
			highCeil.customLight = man.Get<GameObject>("prefab_cafeHangingLight").transform;

			highCeil = AddFunctionToEveryRoom<HighCeilingRoomFunction>(LibraryPrefix);
			highCeil.targetTransformNamePrefix = "Bookshelf";
			highCeil.targetTransformOffset = 9f;
			highCeil.customLight = man.Get<GameObject>("prefab_libraryHangingLight").transform;
			highCeil.usesSingleCustomWall = true;
			highCeil.customWallProximityToCeil = [AssetLoader.TextureFromFile(GetRoomAsset("Library", GetAssetName("libraryWallSheet.png")))];


			// Random Corner Object
			List<WeightedTransform> transformsList = [
				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("lamp.png"))), 25f))
				.AddSpriteHolder(out _, 2.9f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform, weight = 75 },

				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("lightBulb.png"))), 65f))
				.AddSpriteHolder(out _, 5.1f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform, weight = 35 },

				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, GetAssetName("lampShaped.png"))), 25f))
				.AddSpriteHolder(out _, 3.1f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform, weight = 55 },
				];

			TextureExtensions.LoadSpriteSheet(3, 1, 40f, MiscPath, TextureFolder, GetAssetName("SugaLamps.png")).Do(x =>
			{
				transformsList.Add(new()
				{
					selection = ObjectCreationExtensions.CreateSpriteBillboard(x)
				.AddSpriteHolder(out _, 3.1f, LayerStorage.ignoreRaycast)
				.gameObject.SetAsPrefab(true)
				.AddBoxCollider(Vector3.zero, new Vector3(0.8f, 10f, 0.8f), false).transform,
					weight = 38
				});
			});

			WeightedTransform[] transforms = [.. transformsList];

			var cos = AddFunctionToEveryRoom<CornerObjectSpawner>(FacultyPrefix);
			cos.lightPower = 0;
			cos.minAmount = 2;
			cos.maxAmount = 3;
			cos.randomObjs = transforms;
			cos.randomChance = 0.6f;
			cos.nonSafeEntityCell = true;

			cos = AddFunctionToEveryRoom<CornerObjectSpawner>(OfficePrefix);
			cos.lightPower = 0;
			cos.minAmount = 1;
			cos.maxAmount = 3;
			cos.randomObjs = transforms;
			cos.nonSafeEntityCell = true;


			static R AddFunctionToEveryRoom<R>(string prefix) where R : RoomFunction
			{
				var r = Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => x.name.StartsWith(prefix));
				var comp = r.roomFunctionContainer.gameObject.AddComponent<R>();
				r.roomFunctionContainer.AddFunction(comp);
				return comp;
			}
		}

		internal static List<R> AddFunctionToEverythingExcept<R>(params RoomCategory[] exceptions) where R : RoomFunction =>
			AddFunctionToEverythingExcept<R>((x) => true, exceptions);
		internal static List<R> AddFunctionToEverythingExcept<R>(System.Predicate<RoomAsset> predicate, params RoomCategory[] exceptions) where R : RoomFunction
		{
			List<R> l = [];
			foreach (var room in Resources.FindObjectsOfTypeAll<RoomAsset>())
			{
				if (room.roomFunctionContainer && !exceptions.Contains(room.category) && !room.roomFunctionContainer.GetComponent<R>() && predicate(room))
				{
					var comp = room.roomFunctionContainer.gameObject.AddComponent<R>();
					room.roomFunctionContainer.AddFunction(comp);
					l.Add(comp);
				}
			}
			return l;
		}

		const string CafeteriaPrefix = "Cafeteria", OfficePrefix = "Office", ClassPrefix = "Class", LibraryPrefix = "Library", PlaygroundPrefix = "Playground", FacultyPrefix = "Faculty";
	}
}
