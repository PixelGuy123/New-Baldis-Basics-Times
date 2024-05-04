using MTM101BaldAPI.AssetTools;
using BBTimes.Helpers;
using BBTimes.CustomContent.RoomFunctions;
using System.IO;
using UnityEngine;
using MTM101BaldAPI;
using System.Linq;
using BBTimes.Extensions.ObjectCreationExtensions;
using BBTimes.Plugin;
using HarmonyLib;
using BBTimes.Extensions;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using PixelInternalAPI;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateRoomFunctions()
		{
			// Random Window Function for cafe
			AddFunctionToEveryRoom<RandomWindowFunction>(CafeteriaPrefix).window = CreatorExtensions.CreateWindow("classicWindow", // Little tricks to make this function
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "ClassicWindow.png")),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "ClassicWindow_Broken.png")),
				AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "ClassicWindow_Mask.png"))); // Yeah, it creates a window here too. But the WindowCreationprocess is for those that are supposed to spawn naturally;

			// Random poster functon for class and office
			PosterObject[] poster = [ObjectCreators.CreatePosterObject([AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "wall_clock.png"))])];

			AddFunctionToEveryRoom<RandomPosterFunction>(ClassPrefix).posters = poster;
			AddFunctionToEveryRoom<RandomPosterFunction>(OfficePrefix).posters = poster;

			// High ceiling function
			var highCeil = AddFunctionToEveryRoom<HighCeilingRoomFunction>(CafeteriaPrefix);
			highCeil.height = 5;
			highCeil.customCeiling = ObjectCreationExtension.blackTex;
			highCeil.customWallProximityToCeil = [AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "rooms", "Cafeteria", "wallFadeInBlack.png"))];
			highCeil.chanceToHappen = 0.8f;
			highCeil.customLight = man.Get<GameObject>("prefab_cafeHangingLight").transform;

			highCeil = AddFunctionToEveryRoom<HighCeilingRoomFunction>(LibraryPrefix);
			highCeil.height = 1;
			highCeil.targetTransformNamePrefix = "Bookshelf";
			highCeil.targetTransformOffset = 9f;
			// highCeil.customWallProximityToCeil = [Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => x.name.StartsWith("Library")).wallTex];
			var libraryTex = GenericExtensions.FindResourceObjectByName<Texture2D>("Wall"); // Any instance id > 0 is a prefab (I checked that!)

			Resources.FindObjectsOfTypeAll<RoomAsset>().DoIf(x => x.name.StartsWith(LibraryPrefix), x => x.wallTex = libraryTex);

			// Random Corner Object
			WeightedTransform[] transforms = [
				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "lamp.png")), 25f))
				.AddSpriteHolder(2.9f, LayerStorage.ignoreRaycast)
				.transform.parent.gameObject.SetAsPrefab().AddAsGeneratorPrefab()
				.AddBoxCollider(Vector3.zero, new Vector3(2f, 10f, 2f), false).transform, weight = 75 },

				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "lightBulb.png")), 65f))
				.AddSpriteHolder(5.1f, LayerStorage.ignoreRaycast)
				.transform.parent.gameObject.SetAsPrefab().AddAsGeneratorPrefab()
				.AddBoxCollider(Vector3.zero, new Vector3(2f, 10f, 2f), false).transform, weight = 35 },

				new() { selection =  ObjectCreationExtensions.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "lampShaped.png")), 25f))
				.AddSpriteHolder(3.1f, LayerStorage.ignoreRaycast)
				.transform.parent.gameObject.SetAsPrefab().AddAsGeneratorPrefab()
				.AddBoxCollider(Vector3.zero, new Vector3(2f, 10f, 2f), false).transform, weight = 55 },
				];

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

		const string CafeteriaPrefix = "Cafeteria", OfficePrefix = "Office", ClassPrefix = "Class", LibraryPrefix = "Library", FacultyPrefix = "Faculty";
	}
}
