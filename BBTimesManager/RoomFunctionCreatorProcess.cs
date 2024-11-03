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
using BBTimes.CustomContent.Objects;

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

			// -------------------- DUST SHROOM CREATION ----------------------------

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

			var sprites = TextureExtensions.LoadSpriteSheet(2, 1, 33f, BasePlugin.ModPath, "objects", "DustShroom", GetAssetName("shroom.png"));
			var shroomObject = ObjectCreationExtensions.CreateSpriteBillboard(sprites[0]).AddSpriteHolder(out var shroomRenderer, 1.25f, LayerStorage.ignoreRaycast);

			var shroom = shroomObject.gameObject.AddComponent<DustShroom>();
			shroom.name = "DustShroom";
			shroomRenderer.name = "DustShroomRenderer";
			shroom.gameObject.AddBoxCollider(Vector3.up * 5f, new(1f, 10f, 1f), true); // Touching hitbox
			shroom.gameObject.AddObjectToEditor();

			shroom.renderer = shroomRenderer;

			shroom.audMan = shroom.gameObject.CreatePropagatedAudioManager(55f, 65f);
			shroom.audUsed = ObjectCreators.CreateSoundObject(
				AssetLoader.AudioClipFromFile(Path.Combine(BasePlugin.ModPath, "objects", "DustShroom", GetAssetName("deflate.wav"))),
				"Vfx_DustShroom_Deflate", SoundType.Voice, Color.white);

			shroom.raycastBlockingCollider = new GameObject("RaycastBlocker").AddBoxCollider(Vector3.up * 5f, new(4f, 10f, 4f), false);
			shroom.raycastBlockingCollider.gameObject.layer = LayerStorage.blockRaycast;
			shroom.raycastBlockingCollider.transform.SetParent(shroom.transform);
			shroom.raycastBlockingCollider.transform.localPosition = Vector3.zero;
			shroom.raycastBlockingCollider.enabled = false;

			shroom.sprUsed = sprites[1];
			shroom.sprActive = sprites[0];

			var system = new GameObject("Dusts").AddComponent<ParticleSystem>();
			system.transform.SetParent(shroom.transform);
			system.transform.localPosition = Vector3.up * shroom.renderer.transform.localPosition.y;
			system.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			var partsRenderer = system.GetComponent<ParticleSystemRenderer>();
			partsRenderer.material = new Material(ObjectCreationExtension.defaultDustMaterial) { mainTexture = AssetLoader.TextureFromFile(Path.Combine(BasePlugin.ModPath, "objects", "DustShroom", GetAssetName("dust.png"))) };

			var main = system.main;
			main.gravityModifierMultiplier = -0.65f;
			main.startLifetimeMultiplier = 5f;
			main.startSpeedMultiplier = 2f;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			main.startSize = 1f;
			main.startRotation = new(1f, 90f);

			var size = system.sizeOverLifetime;
			size.enabled = true;

			var minMaxCurve = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
				new Keyframe(0f, 1f),
				new Keyframe(0.15f, 2.5f),
				new Keyframe(0.3f, 3.5f),
				new Keyframe(5f, 8f)
				));

			size.x = minMaxCurve;
			size.y = minMaxCurve;
			size.z = minMaxCurve;


			var emission = system.emission;
			emission.rateOverTimeMultiplier = 35f;
			emission.enabled = false;

			var vel = system.velocityOverLifetime;
			vel.enabled = true;
			vel.space = ParticleSystemSimulationSpace.World;
			vel.x = new(-3f, 3f);
			vel.y = new(4f, 12f);
			vel.z = new(-3f, 3f);

			var rot = system.rotationOverLifetime;
			rot.enabled = true;
			rot.x = new(2f, 5f);

			shroom.particles = system;

			var rendCont = shroom.GetComponent<RendererContainer>();
			rendCont.renderers = rendCont.renderers.AddToArray(partsRenderer);

			var rngSpawner = AddFunctionToEveryRoom<RandomObjectSpawner>(PlaygroundPrefix);
			rngSpawner.objectPlacer = ObjectCreationExtensions.SetANewObjectPlacer(shroomObject.gameObject, CellCoverage.Down, TileShape.Open, TileShape.Corner, TileShape.Straight, TileShape.Single).
				SetMinAndMaxObjects(4, 8)
				.SetTilePreferences(false, true, true);

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
